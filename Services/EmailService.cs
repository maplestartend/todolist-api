using System.Net;
using System.Net.Mail;
using System.Text;

namespace TodoListApi.Services;

/// <summary>
/// 郵件服務實作
/// 使用 SMTP 發送各種類型的郵件
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    /// <summary>
    /// 郵件服務建構函式
    /// </summary>
    /// <param name="configuration">組態設定，用於取得 SMTP 設定</param>
    /// <param name="logger">日誌記錄器</param>
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// 發送密碼重設郵件
    /// </summary>
    /// <param name="toEmail">收件人電子郵件地址</param>
    /// <param name="resetToken">密碼重設 Token</param>
    /// <param name="userName">用戶顯示名稱，用於郵件個人化</param>
    /// <returns>郵件發送是否成功</returns>
    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName)
    {
        try
        {
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(toEmail)}";
            
            var subject = "待辦事項系統 - 密碼重設";
            var body = GeneratePasswordResetEmailHtml(userName, resetUrl);

            return await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發送密碼重設郵件失敗，收件人: {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// 發送歡迎郵件
    /// </summary>
    /// <param name="toEmail">收件人電子郵件地址</param>
    /// <param name="userName">用戶顯示名稱</param>
    /// <returns>郵件發送是否成功</returns>
    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
    {
        try
        {
            var subject = "歡迎加入待辦事項系統！";
            var body = GenerateWelcomeEmailHtml(userName);

            return await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發送歡迎郵件失敗，收件人: {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// 發送密碼重設成功通知郵件
    /// </summary>
    /// <param name="toEmail">收件人電子郵件地址</param>
    /// <param name="userName">用戶顯示名稱</param>
    /// <returns>郵件發送是否成功</returns>
    public async Task<bool> SendPasswordResetSuccessEmailAsync(string toEmail, string userName)
    {
        try
        {
            var subject = "待辦事項系統 - 密碼重設成功";
            var body = GeneratePasswordResetSuccessEmailHtml(userName);

            return await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發送密碼重設成功郵件失敗，收件人: {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// 發送郵件的核心方法
    /// </summary>
    /// <param name="toEmail">收件人電子郵件地址</param>
    /// <param name="subject">郵件主題</param>
    /// <param name="body">郵件內容</param>
    /// <param name="isHtml">是否為 HTML 格式</param>
    /// <returns>是否成功發送</returns>
    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        try
        {
            // 從組態中讀取 SMTP 設定
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:SmtpUsername"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"] ?? smtpUsername;
            var fromName = _configuration["Email:FromName"] ?? "待辦事項系統";
            var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

            // 驗證必要的 SMTP 設定
            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) ||
                smtpUsername == "your-email@gmail.com" || smtpPassword == "your-app-password")
            {
                _logger.LogError("SMTP 設定不完整或使用預設範例值，無法發送郵件。收件人: {Email}, 主題: {Subject}", toEmail, subject);
                return false;
            }

            // 確保 fromEmail 不為空，使用 smtpUsername 作為備用
            var senderEmail = fromEmail ?? smtpUsername;

            using var client = new SmtpClient(smtpHost, smtpPort);
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            client.EnableSsl = enableSsl;

            using var message = new MailMessage();
            message.From = new MailAddress(senderEmail, fromName, Encoding.UTF8);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;
            message.BodyEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;

            await client.SendMailAsync(message);
            _logger.LogInformation("郵件發送成功，收件人: {Email}, 主題: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發送郵件時發生錯誤，收件人: {Email}, 主題: {Subject}", toEmail, subject);
            return false;
        }
    }

    /// <summary>
    /// 產生密碼重設郵件 HTML 內容
    /// </summary>
    /// <param name="userName">用戶顯示名稱</param>
    /// <param name="resetUrl">重設密碼連結</param>
    /// <returns>HTML 格式的郵件內容</returns>
    private static string GeneratePasswordResetEmailHtml(string userName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>密碼重設</title>
    <style>
        body {{ font-family: 'Microsoft YaHei', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
        .warning {{ background-color: #fff3cd; color: #856404; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>密碼重設</h1>
        </div>
        <div class='content'>
            <h2>您好，{userName}！</h2>
            <p>我們收到了您的密碼重設請求。如果這不是您的操作，請忽略此郵件。</p>
            <p>若要重設您的密碼，請點擊下方按鈕：</p>
            <a href='{resetUrl}' class='button'>重設密碼</a>
            <div class='warning'>
                <strong>重要提醒：</strong>
                <ul>
                    <li>此連結將在 20 分鐘後失效</li>
                    <li>為了您的帳號安全，請勿將此連結分享給他人</li>
                    <li>如果按鈕無法點擊，請複製以下連結到瀏覽器：<br><code>{resetUrl}</code></li>
                </ul>
            </div>
            <p>如果您沒有要求重設密碼，請立即聯繫我們。</p>
        </div>
        <div class='footer'>
            <p>此為系統自動發送的郵件，請勿直接回覆。</p>
            <p>&copy; 2025 待辦事項系統. 版權所有.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// 產生歡迎郵件 HTML 內容
    /// </summary>
    /// <param name="userName">用戶顯示名稱</param>
    /// <returns>HTML 格式的郵件內容</returns>
    private static string GenerateWelcomeEmailHtml(string userName)
    {
        return $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>歡迎加入</title>
    <style>
        body {{ font-family: 'Microsoft YaHei', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .features {{ background-color: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 歡迎加入待辦事項系統！</h1>
        </div>
        <div class='content'>
            <h2>歡迎您，{userName}！</h2>
            <p>感謝您註冊我們的待辦事項系統，您的帳號已經成功創建。</p>
            <div class='features'>
                <h3>您現在可以：</h3>
                <ul>
                    <li>✅ 創建和管理您的待辦事項</li>
                    <li>📝 編輯和標記事項完成狀態</li>
                    <li>🔍 搜尋和過濾您的任務</li>
                    <li>📱 隨時隨地存取您的清單</li>
                </ul>
            </div>
            <p>立即開始使用，讓我們幫助您更好地管理您的任務！</p>
            <p>如果您有任何問題或需要協助，請隨時聯繫我們。</p>
        </div>
        <div class='footer'>
            <p>此為系統自動發送的郵件，請勿直接回覆。</p>
            <p>&copy; 2025 待辦事項系統. 版權所有.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// 產生密碼重設成功通知郵件 HTML 內容
    /// </summary>
    /// <param name="userName">用戶顯示名稱</param>
    /// <returns>HTML 格式的郵件內容</returns>
    private static string GeneratePasswordResetSuccessEmailHtml(string userName)
    {
        return $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>密碼重設成功</title>
    <style>
        body {{ font-family: 'Microsoft YaHei', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .success {{ background-color: #d4edda; color: #155724; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ 密碼重設成功</h1>
        </div>
        <div class='content'>
            <h2>您好，{userName}！</h2>
            <div class='success'>
                <strong>您的密碼已成功重設！</strong>
            </div>
            <p>您的帳號密碼已經成功更新。現在您可以使用新密碼登入系統。</p>
            <p><strong>安全提醒：</strong></p>
            <ul>
                <li>請確保您的新密碼安全且不與其他網站相同</li>
                <li>定期更換密碼以保護帳號安全</li>
                <li>如果您沒有進行此操作，請立即聯繫我們</li>
            </ul>
            <p>感謝您使用我們的服務！</p>
        </div>
        <div class='footer'>
            <p>此為系統自動發送的郵件，請勿直接回覆。</p>
            <p>&copy; 2025 待辦事項系統. 版權所有.</p>
        </div>
    </div>
</body>
</html>";
    }
} 