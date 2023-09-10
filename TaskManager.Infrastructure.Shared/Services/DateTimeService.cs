using TaskManager.Application.Interfaces;

namespace TaskManager.Infrastructure.Shared.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime CurrentUtc => DateTime.UtcNow;
    }
}
