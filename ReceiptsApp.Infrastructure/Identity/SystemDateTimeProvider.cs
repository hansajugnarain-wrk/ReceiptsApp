using ReceiptsApp.Application.Common.Interfaces;

namespace ReceiptsApp.Infrastructure.Identity;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
