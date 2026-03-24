namespace Ecommerce.Application.Services.Interfaces
{
    /// <summary>
    /// Revoke refresh tokens, bump SessionVersion, clear session fields và xóa Redis auth session — buộc client đăng nhập lại.
    /// </summary>
    public interface IUserSessionInvalidationService
    {
        Task InvalidateAsync(int userId, CancellationToken cancellationToken = default);
    }
}
