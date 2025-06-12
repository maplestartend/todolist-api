// 引入必要的命名空間
using Microsoft.EntityFrameworkCore;        // Entity Framework Core 功能
using Microsoft.IdentityModel.Tokens;       // JWT 權杖處理
using System.IdentityModel.Tokens.Jwt;      // JWT 權杖產生和驗證
using System.Linq;                          // LINQ 查詢功能
using System.Security.Claims;               // 聲明 (Claims) 處理
using System.Security.Cryptography;         // 密碼雜湊功能
using System.Text;                          // 文字編碼
using TodoListApi.Data;                     // 資料層
using TodoListApi.Models;                   // 模型類別

namespace TodoListApi.Services;

/// <summary>
/// 用戶服務的實作類別
/// 實作 IUserService 介面，提供所有用戶相關的業務邏輯
/// 包括註冊、登入、身份驗證、JWT 權杖管理等功能
/// </summary>
public class UserService : IUserService
{
    /// <summary>
    /// 資料庫上下文實例
    /// 用於執行所有資料庫相關操作
    /// </summary>
    private readonly TodoDbContext _context;
    
    /// <summary>
    /// 設定管理實例
    /// 用於讀取應用程式設定 (如 JWT 金鑰)
    /// </summary>
    private readonly IConfiguration _configuration;
    
    /// <summary>
    /// 日誌記錄器
    /// 用於記錄服務操作和錯誤
    /// </summary>
    private readonly ILogger<UserService> _logger;
    
    /// <summary>
    /// 郵件服務實例
    /// 用於發送各種類型的郵件
    /// </summary>
    private readonly IEmailService _emailService;

    /// <summary>
    /// 建構函式
    /// 透過依賴注入接收所需的服務
    /// </summary>
    /// <param name="context">資料庫上下文實例</param>
    /// <param name="configuration">設定管理實例</param>
    /// <param name="logger">日誌記錄器</param>
    /// <param name="emailService">郵件服務實例</param>
    public UserService(TodoDbContext context, IConfiguration configuration, ILogger<UserService> logger, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// 用戶註冊
    /// 建立新的用戶帳號，檢查電子郵件的唯一性
    /// 只使用電子郵件作為登入帳號，顯示名稱用於展示
    /// </summary>
    /// <param name="request">註冊請求資料</param>
    /// <returns>註冊結果，包含成功資訊或詳細錯誤訊息</returns>
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // 驗證輸入資料
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("註冊失敗：電子郵件為空");
                return RegisterResponse.Failure("電子郵件為必填欄位", new List<string> { "請提供有效的電子郵件地址" });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("註冊失敗：密碼為空");
                return RegisterResponse.Failure("密碼為必填欄位", new List<string> { "請提供密碼" });
            }

            // 只檢查電子郵件是否已存在（允許用戶名稱重複）
            var emailExists = await IsEmailExistsAsync(request.Email);
            
            if (emailExists)
            {
                _logger.LogWarning("註冊失敗：電子郵件 {Email} 已存在", request.Email);
                return RegisterResponse.Failure("電子郵件已存在", new List<string> { "該電子郵件地址已被使用，請使用其他電子郵件地址" });
            }

            // 建立新用戶實體
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim().ToLowerInvariant(),
                DisplayName = request.DisplayName?.Trim(),
                PasswordHash = HashPassword(request.Password),
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            // 保存到資料庫
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用戶註冊成功：{Email}", user.Email);

            // 返回成功的用戶資訊 (不包含敏感資料)
            var userResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                CreatedDate = user.CreatedDate,
                IsActive = user.IsActive
            };

            return RegisterResponse.Success(userResponse);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "資料庫更新錯誤，註冊用戶：{Email}", request.Email);
            
            // 檢查是否為唯一性約束違反
            if (dbEx.InnerException?.Message?.Contains("duplicate key") == true || 
                dbEx.InnerException?.Message?.Contains("UNIQUE constraint") == true)
            {
                return RegisterResponse.Failure("電子郵件已存在", new List<string> { "該電子郵件地址已被使用" });
            }
            
            return RegisterResponse.Failure("資料庫錯誤，請稍後再試", new List<string> { $"資料庫錯誤：{dbEx.InnerException?.Message ?? dbEx.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶註冊時發生未預期錯誤：{Email}", request.Email);
            return RegisterResponse.Failure("註冊時發生系統錯誤，請稍後再試", new List<string> { $"錯誤類型：{ex.GetType().Name}", $"錯誤訊息：{ex.Message}" });
        }
    }

    /// <summary>
    /// 用戶登入
    /// 驗證用戶身份並返回 JWT 權杖
    /// 優先使用電子郵件查找用戶（唯一），若失敗則嘗試用戶名稱（可能重複）
    /// </summary>
    /// <param name="request">登入請求資料</param>
    /// <returns>如果登入成功返回登入回應，否則返回 null</returns>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            // 只使用電子郵件查找用戶
            var user = await GetUserByEmailAsync(request.Email);

            // 檢查用戶是否存在且帳號啟用
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("登入失敗：用戶不存在或已停用 {Email}", request.Email);
                return null;
            }

            // 驗證密碼
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("登入失敗：密碼錯誤 {Email}", user.Email);
                return null;
            }

            // 更新最後登入時間
            await UpdateLastLoginAsync(user.Id);

            // 產生 JWT 權杖
            var token = GenerateJwtToken(user);
            var expirationTime = DateTime.UtcNow.AddHours(24); // 24 小時過期

            _logger.LogInformation("用戶登入成功：{Email}", user.Email);

            // 返回登入資訊
            return new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresAt = expirationTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶登入時發生錯誤：{Email}", request.Email);
            return null;
        }
    }

    /// <summary>
    /// 根據 UUID 取得用戶資訊
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <returns>如果找到則返回用戶資訊，否則返回 null</returns>
    public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedDate = user.CreatedDate,
            LastLoginDate = user.LastLoginDate,
            IsActive = user.IsActive
        };
    }

    /// <summary>
    /// 根據電子郵件取得用戶資訊
    /// </summary>
    /// <param name="email">電子郵件</param>
    /// <returns>如果找到則返回用戶實體，否則返回 null</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
    }

    /// <summary>
    /// 檢查電子郵件是否已存在
    /// </summary>
    /// <param name="email">要檢查的電子郵件</param>
    /// <returns>如果已存在返回 true，否則返回 false</returns>
    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant());
    }

    /// <summary>
    /// 驗證密碼是否正確
    /// 使用 BCrypt 進行密碼驗證
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <param name="passwordHash">雜湊後的密碼</param>
    /// <returns>如果密碼正確返回 true，否則返回 false</returns>
    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "密碼驗證時發生錯誤");
            return false;
        }
    }

    /// <summary>
    /// 雜湊密碼
    /// 使用 BCrypt 進行密碼雜湊
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <returns>雜湊後的密碼</returns>
    public string HashPassword(string password)
    {
        // 降低雜湊複雜度以提升效能（生產環境建議使用 12）
        // 開發/測試環境使用 8 可以顯著提升速度
        var rounds = _configuration.GetValue<int>("Security:BcryptRounds", 8);
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(rounds));
    }

    /// <summary>
    /// 產生 JWT 存取權杖
    /// </summary>
    /// <param name="user">用戶實體</param>
    /// <returns>JWT 權杖字串</returns>
    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "TodoListApi_Default_Secret_Key_For_Development_Only_Please_Change_In_Production"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email), // 使用電子郵件作為名稱聲明
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("DisplayName", user.DisplayName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "TodoListApi",
            audience: _configuration["Jwt:Audience"] ?? "TodoListApi",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 更新用戶最後登入時間
    /// </summary>
    /// <param name="userId">用戶 UUID</param>
    /// <returns>如果更新成功返回 true，否則返回 false</returns>
    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用戶最後登入時間時發生錯誤：{UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// 驗證 JWT 權杖並取得用戶 ID
    /// </summary>
    /// <param name="token">JWT 權杖</param>
    /// <returns>如果權杖有效返回用戶 ID，否則返回 null</returns>
    public Guid? ValidateJwtToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "TodoListApi_Default_Secret_Key_For_Development_Only_Please_Change_In_Production");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "TodoListApi",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "TodoListApi",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT 權杖驗證失敗");
            return null;
        }
    }
    
    /// <summary>
    /// 忘記密碼 - 發送重設密碼郵件
    /// 產生重設密碼的 Token 並發送到用戶信箱
    /// </summary>
    /// <param name="request">忘記密碼請求資料</param>
    /// <returns>處理結果，包含成功或失敗訊息</returns>
    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            // 根據電子郵件查找用戶
            var user = await GetUserByEmailAsync(request.Email);
            
            if (user == null || !user.IsActive)
            {
                // 為了安全起見，即使用戶不存在也返回成功訊息
                // 避免透露用戶是否存在於系統中
                _logger.LogWarning("忘記密碼請求：用戶不存在或已停用 {Email}", request.Email);
                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "如果該電子郵件地址在我們的系統中，您將收到重設密碼的指示"
                };
            }

            // 產生重設密碼的 Token
            var resetToken = GeneratePasswordResetToken();
            var tokenExpiry = DateTime.UtcNow.AddMinutes(20); // Token 20 分鐘後過期

            // 儲存 Token 到資料庫
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = tokenExpiry;
            await _context.SaveChangesAsync();

            // 發送重設密碼郵件
            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                user.Email, 
                resetToken, 
                user.DisplayName ?? user.Email
            );

            if (!emailSent)
            {
                _logger.LogError("忘記密碼郵件發送失敗：{Email}", user.Email);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "郵件發送失敗，請稍後再試"
                };
            }

            _logger.LogInformation("忘記密碼郵件發送成功：{Email}", user.Email);
            
            return new ApiResponse<object>
            {
                Success = true,
                Message = "重設密碼的指示已發送到您的電子郵件"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "處理忘記密碼請求時發生錯誤：{Email}", request.Email);
            return new ApiResponse<object>
            {
                Success = false,
                Message = "處理請求時發生錯誤，請稍後再試"
            };
        }
    }

    /// <summary>
    /// 重設密碼
    /// 使用 Token 驗證並重設用戶密碼
    /// </summary>
    /// <param name="request">重設密碼請求資料</param>
    /// <returns>處理結果，包含成功或失敗訊息</returns>
    public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            // 驗證重設 Token
            var user = await ValidatePasswordResetTokenAsync(request.Email, request.Token);
            
            if (user == null)
            {
                _logger.LogWarning("重設密碼失敗：無效的 Token 或電子郵件 {Email}", request.Email);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "重設密碼連結無效或已過期"
                };
            }

            // 更新密碼
            user.PasswordHash = HashPassword(request.NewPassword);
            
            // 清除重設 Token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            
            await _context.SaveChangesAsync();

            // 發送密碼重設成功通知郵件
            await _emailService.SendPasswordResetSuccessEmailAsync(
                user.Email, 
                user.DisplayName ?? user.Email
            );

            _logger.LogInformation("密碼重設成功：{Email}", user.Email);
            
            return new ApiResponse<object>
            {
                Success = true,
                Message = "密碼重設成功，請使用新密碼登入"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重設密碼時發生錯誤：{Email}", request.Email);
            return new ApiResponse<object>
            {
                Success = false,
                Message = "重設密碼時發生錯誤，請稍後再試"
            };
        }
    }

    /// <summary>
    /// 產生密碼重設 Token
    /// 生成用於重設密碼的安全 Token
    /// </summary>
    /// <returns>重設密碼的 Token</returns>
    public string GeneratePasswordResetToken()
    {
        // 使用 GUID 和時間戳記生成更安全的 Token
        var guidPart = Guid.NewGuid().ToString("N");
        var timestampPart = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        
        // 組合並進行 Base64 編碼
        var combined = $"{guidPart}-{timestampPart}";
        var bytes = Encoding.UTF8.GetBytes(combined);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    /// <summary>
    /// 驗證密碼重設 Token 是否有效
    /// 檢查 Token 是否存在且未過期
    /// </summary>
    /// <param name="email">電子郵件地址</param>
    /// <param name="token">重設 Token</param>
    /// <returns>如果 Token 有效返回用戶實體，否則返回 null</returns>
    public async Task<User?> ValidatePasswordResetTokenAsync(string email, string token)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    u.Email == email.ToLowerInvariant() && 
                    u.PasswordResetToken == token &&
                    u.PasswordResetTokenExpiry.HasValue &&
                    u.PasswordResetTokenExpiry.Value > DateTime.UtcNow &&
                    u.IsActive);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證密碼重設 Token 時發生錯誤：{Email}", email);
            return null;
        }
    }

    /// <summary>
    /// 更新用戶基本資料
    /// 允許更新電子郵件和顯示名稱
    /// </summary>
    /// <param name="userId">用戶 UUID</param>
    /// <param name="request">更新請求資料</param>
    /// <returns>處理結果，包含更新後的用戶資訊或錯誤訊息</returns>
    public async Task<ApiResponse<UserResponse>> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request)
    {
        try
        {
            // 查找用戶
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            
            if (user == null)
            {
                _logger.LogWarning("更新用戶資料失敗：用戶不存在或已停用 {UserId}", userId);
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "用戶不存在或已停用"
                };
            }

            bool hasChanges = false;

            // 檢查是否要更新電子郵件
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email.Trim().ToLowerInvariant() != user.Email)
            {
                var newEmail = request.Email.Trim().ToLowerInvariant();
                
                // 檢查新電子郵件是否已被其他用戶使用
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == newEmail && u.Id != userId);
                
                if (emailExists)
                {
                    return new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "該電子郵件地址已被其他用戶使用"
                    };
                }

                user.Email = newEmail;
                hasChanges = true;
                _logger.LogInformation("用戶電子郵件已更新：{UserId} -> {NewEmail}", userId, newEmail);
            }

            // 檢查是否要更新顯示名稱
            if (request.DisplayName != null && request.DisplayName.Trim() != user.DisplayName)
            {
                user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim();
                hasChanges = true;
                _logger.LogInformation("用戶顯示名稱已更新：{UserId}", userId);
            }

            // 如果有變更，儲存到資料庫
            if (hasChanges)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("用戶資料更新成功：{UserId}", userId);
            }

            // 返回更新後的用戶資訊
            var userResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                IsActive = user.IsActive
            };

            return new ApiResponse<UserResponse>
            {
                Success = true,
                Message = hasChanges ? "用戶資料更新成功" : "沒有需要更新的資料",
                Data = userResponse
            };
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "資料庫更新錯誤，更新用戶資料：{UserId}", userId);
            
            // 檢查是否為唯一性約束違反 (電子郵件重複)
            if (dbEx.InnerException?.Message?.Contains("duplicate key") == true || 
                dbEx.InnerException?.Message?.Contains("UNIQUE constraint") == true)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "該電子郵件地址已被其他用戶使用"
                };
            }
            
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Message = "資料庫錯誤，請稍後再試"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用戶資料時發生未預期錯誤：{UserId}", userId);
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Message = "更新用戶資料時發生系統錯誤，請稍後再試"
            };
        }
    }

    /// <summary>
    /// 更換用戶密碼
    /// 驗證目前密碼後更新為新密碼
    /// </summary>
    /// <param name="userId">用戶 UUID</param>
    /// <param name="request">更換密碼請求資料</param>
    /// <returns>處理結果，包含成功或失敗訊息</returns>
    public async Task<ApiResponse<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        try
        {
            // 查找用戶
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            
            if (user == null)
            {
                _logger.LogWarning("更換密碼失敗：用戶不存在或已停用 {UserId}", userId);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "用戶不存在或已停用"
                };
            }

            // 驗證目前密碼是否正確
            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("更換密碼失敗：目前密碼錯誤 {UserId}", userId);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "目前密碼不正確"
                };
            }

            // 檢查新密碼是否與目前密碼相同
            if (VerifyPassword(request.NewPassword, user.PasswordHash))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "新密碼不能與目前密碼相同"
                };
            }

            // 更新密碼
            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用戶密碼更換成功：{UserId}", userId);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "密碼更換成功"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更換密碼時發生未預期錯誤：{UserId}", userId);
            return new ApiResponse<object>
            {
                Success = false,
                Message = "更換密碼時發生系統錯誤，請稍後再試"
            };
        }
    }
} 