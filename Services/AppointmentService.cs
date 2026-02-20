using AlmavivaSlotChecker.Data;
using AlmavivaSlotChecker.Data.Entities;
using AlmavivaSlotChecker.Models;

namespace AlmavivaSlotChecker.Services;

public sealed class AppointmentService(AlmavivaClient client, AppDbContext dbContext)
{
    public async Task<(bool Success, string? Error)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await client.LoginAsync(request, cancellationToken);

            dbContext.UserLoginAudits.Add(new UserLoginAudit
            {
                Email = request.Email,
                IsSuccess = success,
                ErrorMessage = success ? null : "Сервис вернул ошибку авторизации"
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            return success
                ? (true, null)
                : (false, "Не удалось выполнить вход. Проверьте email/пароль или endpoint.");
        }
        catch (Exception ex)
        {
            dbContext.UserLoginAudits.Add(new UserLoginAudit
            {
                Email = request.Email,
                IsSuccess = false,
                ErrorMessage = ex.Message
            });
            await dbContext.SaveChangesAsync(cancellationToken);

            return (false, ex.Message);
        }
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
