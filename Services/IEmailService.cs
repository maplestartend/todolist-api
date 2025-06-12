namespace TodoListApi.Services;

/// <summary>
/// 郵件服務介面
/// 定義發送各種類型郵件的方法
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// 發送密碼重設郵件
    /// </summary>
    /// <param name="toEmail">收件人電子郵件地址</param>
    /// <param name="resetToken">密碼重設 Token</param>
    /// <param name="userName">用戶顯示名稱，用於郵件個人化</param>
    /// <returns>郵件發送是否成功</returns>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName);

    /// <summary>
    /// 發送歡迎郵件
    /// </summary>
    /// <param name="toEmail">收件人電子郵件地址</param>
    /// <param name="userName">用戶顯示名稱</param>
    /// <returns>郵件發送是否成功</returns>
    Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);

    /// <summary>
    /// 發送密碼重設成功通知郵件
    /// </summary>
    /// <param name="toEmail">收件人電子郵件地址</param>
    /// <param name="userName">用戶顯示名稱</param>
    /// <returns>郵件發送是否成功</returns>
    Task<bool> SendPasswordResetSuccessEmailAsync(string toEmail, string userName);
} 