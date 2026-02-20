using AlmavivaSlotChecker.Data;
using AlmavivaSlotChecker.Data.Entities;
using AlmavivaSlotChecker.Models;

namespace AlmavivaSlotChecker.Services;

public sealed class AppointmentService(AlmavivaClient client, AppDbContext dbContext)
{
    public async Task AuditLoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        dbContext.UserLoginAudits.Add(new UserLoginAudit
        {
            Email = request.Email ?? "unknown",
            IsSuccess = request.IsSuccess,
            ErrorMessage = request.ErrorMessage
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SlotCheckResponse> CheckSlotsAsync(SlotCheckRequest request, CancellationToken cancellationToken = default)
    {
        var result = await client.CheckSlotsAsync(request, cancellationToken);

        dbContext.SlotCheckAudits.Add(new SlotCheckAudit
        {
            VisaCenterCode = request.VisaCenterCode,
            ServiceCode = request.ServiceCode,
            HasSlots = result.HasSlots,
            RawResponse = result.RawResponse
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
