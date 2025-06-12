using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApi.Models;

/// <summary>
/// 用戶實體類別
/// 用於資料庫儲存用戶資訊，支援電子郵件登入
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// 用戶唯一識別碼
    /// 主鍵，使用 GUID 格式以增強安全性
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 電子郵件地址
    /// 必須唯一，用於登入和忘記密碼功能
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 字元")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 加密後的密碼
    /// 儲存 BCrypt 雜湊值
    /// </summary>
    [Required(ErrorMessage = "密碼為必填欄位")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 顯示名稱
    /// 用於 UI 顯示的友善名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "顯示名稱長度不能超過 100 字元")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 用戶創建時間
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最後登入時間
    /// 可為空值，表示用戶尚未登入過
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// 帳號是否啟用
    /// 可用於停用帳號功能
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 密碼重設 Token
    /// 忘記密碼功能使用，用於驗證重設密碼請求
    /// </summary>
    public string? PasswordResetToken { get; set; }

    /// <summary>
    /// 密碼重設 Token 的過期時間
    /// 確保重設 Token 有時效性，提高安全性
    /// </summary>
    public DateTime? PasswordResetTokenExpiry { get; set; }

    /// <summary>
    /// 此用戶擁有的待辦事項列表
    /// 一對多關聯：一個用戶可以有多個待辦事項
    /// </summary>
    public virtual ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}

/// <summary>
/// 用戶註冊請求 DTO
/// 用於接收用戶註冊時的資料
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 電子郵件地址
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    [Required(ErrorMessage = "密碼為必填欄位")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在 6 到 100 字元之間")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 顯示名稱（可選）
    /// </summary>
    [StringLength(100, ErrorMessage = "顯示名稱長度不能超過 100 字元")]
    public string? DisplayName { get; set; }
}

/// <summary>
/// 用戶登入請求 DTO
/// 用於接收用戶登入時的認證資料
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 電子郵件地址
    /// 使用電子郵件作為登入帳號
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    [Required(ErrorMessage = "密碼為必填欄位")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 忘記密碼請求 DTO
/// 用於接收忘記密碼時的電子郵件
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// 電子郵件地址
    /// 系統將發送重設密碼的連結到此信箱
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// 重設密碼請求 DTO
/// 用於接收重設密碼時的資料
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// 重設密碼的 Token
    /// 從忘記密碼郵件中獲得
    /// </summary>
    [Required(ErrorMessage = "重設 Token 為必填欄位")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件地址
    /// 用於驗證 Token 的有效性
    /// </summary>
    [Required(ErrorMessage = "電子郵件為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 新密碼
    /// </summary>
    [Required(ErrorMessage = "新密碼為必填欄位")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在 6 到 100 字元之間")]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 更新用戶基本資料請求 DTO
/// 用於接收用戶更新電子郵件和顯示名稱的資料
/// </summary>
public class UpdateUserProfileRequest
{
    /// <summary>
    /// 電子郵件地址（可選）
    /// 如果提供則更新電子郵件
    /// </summary>
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 字元")]
    public string? Email { get; set; }

    /// <summary>
    /// 顯示名稱（可選）
    /// 如果提供則更新顯示名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "顯示名稱長度不能超過 100 字元")]
    public string? DisplayName { get; set; }
}

/// <summary>
/// 更換密碼請求 DTO
/// 用於接收用戶更換密碼時的資料
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// 目前密碼
    /// 必須提供目前密碼進行驗證
    /// </summary>
    [Required(ErrorMessage = "目前密碼為必填欄位")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密碼
    /// 必須提供新密碼
    /// </summary>
    [Required(ErrorMessage = "新密碼為必填欄位")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在 6 到 100 字元之間")]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 用戶登入回應 DTO
/// 包含 JWT Token 和用戶基本資訊
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 用戶唯一識別碼
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 電子郵件地址
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 顯示名稱
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// JWT 存取 Token
    /// 用於後續 API 呼叫的身份驗證
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token 類型，通常為 "Bearer"
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token 過期時間
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// 用戶回應 DTO
/// 用於 API 回傳用戶基本資訊（不包含敏感資料）
/// </summary>
public class UserResponse
{
    /// <summary>
    /// 用戶唯一識別碼
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 電子郵件地址
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 顯示名稱
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 用戶創建時間
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// 帳號是否啟用
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// 註冊回應模型
/// 包含註冊結果和詳細錯誤資訊
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// 是否註冊成功
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// 註冊成功時的用戶資訊
    /// </summary>
    public UserResponse? User { get; set; }
    
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 詳細錯誤資訊列表
    /// 例如：["電子郵件已存在"]
    /// </summary>
    public List<string> ErrorDetails { get; set; } = new();
    
    /// <summary>
    /// 建立成功的回應
    /// </summary>
    /// <param name="user">註冊成功的用戶資訊</param>
    /// <returns>成功的註冊回應</returns>
    public static RegisterResponse Success(UserResponse user)
    {
        return new RegisterResponse
        {
            IsSuccess = true,
            User = user
        };
    }
    
    /// <summary>
    /// 建立失敗的回應
    /// </summary>
    /// <param name="errorMessage">主要錯誤訊息</param>
    /// <param name="errorDetails">詳細錯誤資訊</param>
    /// <returns>失敗的註冊回應</returns>
    public static RegisterResponse Failure(string errorMessage, List<string>? errorDetails = null)
    {
        return new RegisterResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails ?? new List<string>()
        };
    }
}

/// <summary>
/// API 標準回應格式
/// 提供統一的 API 回應結構
/// </summary>
/// <typeparam name="T">回應資料的類型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 回應訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 回應資料
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 錯誤詳細資訊（僅在失敗時提供）
    /// </summary>
    public object? Errors { get; set; }
} 