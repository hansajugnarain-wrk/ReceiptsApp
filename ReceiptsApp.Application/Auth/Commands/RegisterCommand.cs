using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Application.Auth.DTOs;
using ReceiptsApp.Application.Common.Interfaces;
using ReceiptsApp.Application.Common.Models;
using ReceiptsApp.Domain.Entities;
using ReceiptsApp.Domain.Interfaces;

namespace ReceiptsApp.Application.Auth.Commands;

public sealed record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<Result<AuthResponse>>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existingUser is not null)
        {
            _logger.LogInformation("Registration rejected: {Email} already exists", request.Email);
            return Result.Fail<AuthResponse>("An account with that email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash, request.DisplayName);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user registered: {UserId}", user.Id);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAtUtc = _dateTimeProvider.UtcNow.AddHours(1);

        return Result.Ok(new AuthResponse(accessToken, refreshToken, expiresAtUtc, user.Email, user.DisplayName));
    }
}
