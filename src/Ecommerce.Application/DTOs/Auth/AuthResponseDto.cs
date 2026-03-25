namespace Ecommerce.Application.DTOs.Auth;

/// <summary>Phản hồi login/refresh. Sau <c>POST /auth/refresh</c>, giữ nguyên refresh token đã lưu cho tới khi hết hạn hoặc logout.</summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Login: refresh token mới. Refresh: cùng giá trị client đã gửi (không đổi cho tới khi RT hết hạn).</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Thời sống access token (giây).</summary>
    public int ExpiresIn { get; set; }
}
 