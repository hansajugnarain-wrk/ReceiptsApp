using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Application.Auth.DTOs;
using ReceiptsApp.Application.Common.Interfaces;
using ReceiptsApp.Application.Common.Models;
using ReceiptsApp.Domain.Interfaces;

namespace ReceiptsApp.Application.Auth.Commands;

/// <summary>Use case input: authenticate a user with email and password.</summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

/// <summary>
/// Orchestrates the login use case: look up the user, verify the password,
/// record the login, issue tokens. Contains no SQL, no HTTP, no JWT internals —
/// those live behind interfaces implemented in Infrastructure.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return Result.Fail<AuthResponse>("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for deactivated account {Email}", request.Email);
            return Result.Fail<AuthResponse>("This account has been deactivated.");
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAtUtc = _dateTimeProvider.UtcNow.AddHours(1);

        return Result.Ok(new AuthResponse(accessToken, refreshToken, expiresAtUtc, user.Email, user.DisplayName));
    }
}
