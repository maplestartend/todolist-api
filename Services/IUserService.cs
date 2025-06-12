// 引入必要的命名空間
using TodoListApi.Models; // 模型類別

namespace TodoListApi.Services;

/// <summary>
/// 用戶服務的介面定義
/// 定義了所有用戶相關的業務邏輯方法
/// 包括註冊、登入、身份驗證等功能
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 用戶註冊
    /// 建立新的用戶帳號，檢查電子郵件唯一性（允許用戶名稱重複）
    /// </summary>
    /// <param name="request">註冊請求資料</param>
    /// <returns>註冊結果，包含成功資訊或詳細錯誤訊息</returns>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// 用戶登入
    /// 驗證用戶身份並返回登入資訊
    /// </summary>
    /// <param name="request">登入請求資料</param>
    /// <returns>如果登入成功返回登入回應，否則返回 null</returns>
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// 根據 UUID 取得用戶資訊
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <returns>如果找到則返回用戶資訊，否則返回 null</returns>
    Task<UserResponse?> GetUserByIdAsync(Guid userId);
    
    /// <summary>
    /// 根據電子郵件取得用戶資訊
    /// </summary>
    /// <param name="email">電子郵件</param>
    /// <returns>如果找到則返回用戶實體，否則返回 null</returns>
    Task<User?> GetUserByEmailAsync(string email);
    
    /// <summary>
    /// 檢查電子郵件是否已存在
    /// </summary>
    /// <param name="email">要檢查的電子郵件</param>
    /// <returns>如果已存在返回 true，否則返回 false</returns>
    Task<bool> IsEmailExistsAsync(string email);
    
    /// <summary>
    /// 驗證密碼是否正確
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <param name="passwordHash">雜湊後的密碼</param>
    /// <returns>如果密碼正確返回 true，否則返回 false</returns>
    bool VerifyPassword(string password, string passwordHash);
    
    /// <summary>
    /// 雜湊密碼
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <returns>雜湊後的密碼</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// 產生 JWT 存取權杖
    /// </summary>
    /// <param name="user">用戶實體</param>
    /// <returns>JWT 權杖字串</returns>
    string GenerateJwtToken(User user);
    
    /// <summary>
    /// 更新用戶最後登入時間
    /// </summary>
    /// <param name="userId">用戶 UUID</param>
    /// <returns>如果更新成功返回 true，否則返回 false</returns>
    Task<bool> UpdateLastLoginAsync(Guid userId);
    
    /// <summary>
    /// 驗證 JWT 權杖並取得用戶 ID
    /// </summary>
    /// <param name="token">JWT 權杖</param>
    /// <returns>如果權杖有效返回用戶 ID，否則返回 null</returns>
    Guid? ValidateJwtToken(string token);
    
    /// <summary>
    /// 忘記密碼 - 發送重設密碼郵件
    /// 產生重設密碼的 Token 並發送到用戶信箱
    /// </summary>
    /// <param name="request">忘記密碼請求資料</param>
    /// <returns>處理結果，包含成功或失敗訊息</returns>
    Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request);
    
    /// <summary>
    /// 重設密碼
    /// 使用 Token 驗證並重設用戶密碼
    /// </summary>
    /// <param name="request">重設密碼請求資料</param>
    /// <returns>處理結果，包含成功或失敗訊息</returns>
    Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request);
    
    /// <summary>
    /// 產生密碼重設 Token
    /// 生成用於重設密碼的安全 Token
    /// </summary>
    /// <returns>重設密碼的 Token</returns>
    string GeneratePasswordResetToken();
    
    /// <summary>
    /// 驗證密碼重設 Token 是否有效
    /// 檢查 Token 是否存在且未過期
    /// </summary>
    /// <param name="email">電子郵件地址</param>
    /// <param name="token">重設 Token</param>
    /// <returns>如果 Token 有效返回用戶實體，否則返回 null</returns>
    Task<User?> ValidatePasswordResetTokenAsync(string email, string token);
    
    /// <summary>
    /// 更新用戶基本資料
    /// 允許更新電子郵件和顯示名稱
    /// </summary>
    /// <param name="userId">用戶 UUID</param>
    /// <param name="request">更新請求資料</param>
    /// <returns>處理結果，包含更新後的用戶資訊或錯誤訊息</returns>
    Task<ApiResponse<UserResponse>> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);
    
    /// <summary>
    /// 更換用戶密碼
    /// 驗證目前密碼後更新為新密碼
    /// </summary>
    /// <param name="userId">用戶 UUID</param>
    /// <param name="request">更換密碼請求資料</param>
    /// <returns>處理結果，包含成功或失敗訊息</returns>
    Task<ApiResponse<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
} 