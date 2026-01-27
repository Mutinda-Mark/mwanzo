using Microsoft.EntityFrameworkCore;
using mwanzo.Data;
using mwanzo.Models; // Add this to access Models.AuditLog
using System.Security.Claims;

namespace mwanzo.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entity, string entityId, string details = "")
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
            var auditLog = new AuditLog // Now refers to mwanzo.Models.AuditLog
            {
                UserId = userId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        // Remove the nested AuditLog class entirely (it's now in Models/)
    }
}