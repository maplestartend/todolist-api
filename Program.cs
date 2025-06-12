// =============================================================================
// TodoListApi - 多用戶待辦事項管理 API 應用程式
// 使用 ASP.NET Core 8.0 + Entity Framework Core + PostgreSQL + JWT 身份驗證
// =============================================================================

// 引入必要的命名空間
using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT Bearer 身份驗證
using Microsoft.EntityFrameworkCore;                 // Entity Framework Core 的核心功能
using Microsoft.IdentityModel.Tokens;               // JWT 權杖處理
using System.Security.Claims;                       // 身份聲明處理
using System.Text;                                  // 文字編碼
using TodoListApi.Data;                             // 資料層相關類別
using TodoListApi.Models;                           // 模型類別
using TodoListApi.Services;                         // 服務層類別
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// ========== 服務容器設定 ==========
// 將服務註冊到 DI (Dependency Injection) 容器中

// 註冊 API 探索服務，用於自動產生 API 文件
builder.Services.AddEndpointsApiExplorer();

// 註冊 Swagger 服務，用於產生互動式 API 文件
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TodoListApi", Version = "v1" });
    
    // 添加 JWT Bearer 驗證到 Swagger UI
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// 設定 PostgreSQL 資料庫連線
// 從 appsettings.json 讀取連線字串並配置 Entity Framework Core
builder.Services.AddDbContext<TodoDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(120); // 增加到 120 秒
    });
    
    // 在生產環境也啟用部分記錄以便診斷
    if (builder.Environment.IsDevelopment() || builder.Environment.IsProduction())
    {
        options.LogTo(Console.WriteLine, LogLevel.Warning);
    }
});

// ========== JWT 身份驗證設定 ==========
// 設定 JWT 權杖驗證
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? 
                "TodoListApi_Default_Secret_Key_For_Development_Only_Please_Change_In_Production")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TodoListApi",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TodoListApi",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // 設定從 Cookie 中讀取 token
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // 優先從 Authorization header 讀取（保持向下相容）
                var token = context.Request.Headers.Authorization
                    .FirstOrDefault()?.Split(" ").Last();

                // 如果 header 中沒有 token，則從 cookie 中讀取
                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Cookies["authToken"];
                }

                context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

// 註冊授權服務
builder.Services.AddAuthorization();

// 註冊業務服務到 DI 容器
// 使用 Scoped 生命週期，確保每個 HTTP 請求都有獨立的服務實例
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// 設定 CORS (跨來源資源共用) 政策
// 支援 Cookie 跨域傳遞，需要指定具體來源而非 AllowAnyOrigin
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // 開發環境允許本地端口，生產環境需要指定實際域名
        var allowedOrigins = new List<string>
        {
            "http://localhost:3000",     // Vite 開發伺服器
            "http://localhost:5173",     // Vite 備用端口
            "https://localhost:3000",    // HTTPS 本地開發
            "https://localhost:5173"     // HTTPS Vite 備用
        };

        // 生產環境 - Vercel 域名
        allowedOrigins.Add("https://todolist-frontend-lac.vercel.app");
        
        // 如果有自定義域名，也要添加
        // allowedOrigins.Add("https://your-custom-domain.com");

        policy.WithOrigins(allowedOrigins.ToArray())  // 指定允許的來源
              .AllowAnyMethod()                       // 允許任何 HTTP 方法
              .AllowAnyHeader()                       // 允許任何請求標頭
              .AllowCredentials();                    // 允許傳遞 Cookie 和認證資訊
    });
});

var app = builder.Build();



// ========== HTTP 請求管道設定 ==========
// 設定中間件的執行順序

// 在開發環境和生產環境中啟用 Swagger UI（方便測試 API）
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();    // 啟用 Swagger JSON 端點
    app.UseSwaggerUI();  // 啟用 Swagger UI 介面
}

// 只在非開發環境使用 HTTPS 重定向
// 開發環境通常使用 HTTP，生產環境建議使用 HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 啟用 CORS 中間件
app.UseCors();

// 啟用身份驗證中間件
app.UseAuthentication();

// 啟用授權中間件
app.UseAuthorization();

// ========== 輔助方法 ==========
// 從 JWT Claims 中取得當前用戶 ID
static Guid GetCurrentUserId(ClaimsPrincipal user)
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
    {
        return userId;
    }
    throw new UnauthorizedAccessException("無效的用戶身份");
}

// ========== 身份驗證 API 端點 ==========

// POST /auth/register - 用戶註冊
app.MapPost("/auth/register", async (RegisterRequest request, IUserService userService) =>
{
    // 驗證請求資料
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { 
            message = "電子郵件和密碼為必填欄位",
            errors = new[] { "電子郵件和密碼為必填欄位" }
        });
    }

    try
    {
        var result = await userService.RegisterAsync(request);
        
        if (!result.IsSuccess)
        {
            // 根據錯誤詳情返回適當的狀態碼和訊息
            return Results.Conflict(new { 
                message = result.ErrorMessage,
                errors = result.ErrorDetails,
                details = "請檢查並修正以下問題後重新嘗試註冊"
            });
        }

        return Results.Created($"/users/{result.User!.Id}", new {
            message = "註冊成功",
            user = result.User
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { 
            message = "註冊時發生系統錯誤，請稍後再試",
            errors = new[] { 
                $"錯誤類型：{ex.GetType().Name}",
                $"錯誤訊息：{ex.Message}",
                ex.InnerException != null ? $"內部錯誤：{ex.InnerException.Message}" : null
            }.Where(e => e != null).ToArray(),
            details = "請檢查並修正以下問題後重新嘗試註冊"
        }, statusCode: 500);
    }
})
.WithName("Register")
.WithOpenApi()
.WithSummary("用戶註冊")
.WithDescription("建立新的用戶帳號，只使用電子郵件作為登入帳號");

// POST /auth/login - 用戶登入（安全版本）
app.MapPost("/auth/login", async (LoginRequest request, IUserService userService, HttpContext context) =>
{
    try
    {
        var response = await userService.LoginAsync(request);
        if (response == null)
        {
            return Results.Unauthorized();
        }

        // 設定安全的 HttpOnly Cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,                    // 防止 JavaScript 存取
            Secure = !context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(), // 生產環境強制 HTTPS
            SameSite = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() 
                ? SameSiteMode.Lax              // 開發環境使用 Lax（允許同域和安全跨域）
                : SameSiteMode.None,            // 生產環境使用 None（允許跨域，需要 Secure=true）
            Expires = DateTimeOffset.UtcNow.AddHours(24), // 24 小時過期
            Path = "/"                          // 整個網站可用
        };

        context.Response.Cookies.Append("authToken", response.AccessToken, cookieOptions);

        // 回傳不包含 token 的用戶資訊
        var safeResponse = new
        {
            UserId = response.UserId,
            Email = response.Email,
            DisplayName = response.DisplayName,
            TokenType = response.TokenType,
            ExpiresAt = response.ExpiresAt
        };

        return Results.Ok(safeResponse);
    }
    catch (Exception ex)
    {
        return Results.Problem($"登入時發生錯誤: {ex.Message}");
    }
})
.WithName("Login")
.WithOpenApi()
.WithSummary("用戶登入")
.WithDescription("驗證用戶身份並設定安全的認證 Cookie");

// POST /auth/logout - 用戶登出（安全版本）
app.MapPost("/auth/logout", (HttpContext context) =>
{
    // 清除認證 Cookie
    context.Response.Cookies.Delete("authToken", new CookieOptions
    {
        HttpOnly = true,
        Secure = !context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
        SameSite = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() 
            ? SameSiteMode.Lax 
            : SameSiteMode.None,
        Path = "/"
    });

    return Results.Ok(new { message = "登出成功" });
})
.WithName("Logout")
.WithOpenApi()
.WithSummary("用戶登出")
.WithDescription("清除認證 Cookie 並登出用戶");

// GET /auth/me - 取得當前用戶資訊
app.MapGet("/auth/me", async (ClaimsPrincipal user, IUserService userService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var userInfo = await userService.GetUserByIdAsync(userId);
        
        if (userInfo == null)
        {
            return Results.NotFound("用戶不存在");
        }

        return Results.Ok(userInfo);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得用戶資訊時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("GetCurrentUser")
.WithOpenApi()
.WithSummary("取得當前用戶資訊")
.WithDescription("取得目前登入用戶的基本資訊");

// POST /auth/forgot-password - 忘記密碼
app.MapPost("/auth/forgot-password", async (ForgotPasswordRequest request, IUserService userService, HttpContext context) =>
{
    try
    {
        // 驗證請求資料
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.BadRequest(new { 
                message = "電子郵件為必填欄位",
                errors = new[] { "請提供有效的電子郵件地址" }
            });
        }

        var result = await userService.ForgotPasswordAsync(request);
        
        if (result.Success)
        {
            return Results.Ok(new { 
                message = result.Message,
                data = result.Data
            });
        }
        else
        {
            return Results.BadRequest(new { 
                message = result.Message,
                errors = result.Errors
            });
        }
    }
    catch (Exception ex)
    {
        // 記錄詳細錯誤資訊
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "忘記密碼功能發生錯誤");
        
        return Results.Json(new { 
            message = "處理請求時發生錯誤，請稍後再試",
            errors = new[] { 
                $"錯誤類型：{ex.GetType().Name}",
                $"錯誤訊息：{ex.Message}",
                ex.InnerException != null ? $"內部錯誤：{ex.InnerException.Message}" : null
            }.Where(e => e != null).ToArray()
        }, statusCode: 500);
    }
})
.WithName("ForgotPassword")
.WithOpenApi()
.WithSummary("忘記密碼")
.WithDescription("發送密碼重設連結到指定的電子郵件地址");

// POST /auth/reset-password - 重設密碼
app.MapPost("/auth/reset-password", async (ResetPasswordRequest request, IUserService userService) =>
{
    try
    {
        // 驗證請求資料
        if (string.IsNullOrWhiteSpace(request.Token) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Results.BadRequest(new { 
                message = "所有欄位都是必填的",
                errors = new[] { "請提供有效的重設 Token、電子郵件和新密碼" }
            });
        }

        var result = await userService.ResetPasswordAsync(request);
        
        if (result.Success)
        {
            return Results.Ok(new { 
                message = result.Message,
                data = result.Data
            });
        }
        else
        {
            return Results.BadRequest(new { 
                message = result.Message,
                errors = result.Errors
            });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"重設密碼時發生錯誤: {ex.Message}");
    }
})
.WithName("ResetPassword")
.WithOpenApi()
.WithSummary("重設密碼")
.WithDescription("使用重設 Token 更新用戶密碼");

// PUT /auth/profile - 更新用戶基本資料（電子郵件和暱稱）
app.MapPut("/auth/profile", async (UpdateUserProfileRequest request, ClaimsPrincipal user, IUserService userService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var result = await userService.UpdateUserProfileAsync(userId, request);
        
        if (result.Success)
        {
            return Results.Ok(new { 
                message = result.Message,
                data = result.Data
            });
        }
        else
        {
            return Results.BadRequest(new { 
                message = result.Message,
                errors = result.Errors
            });
        }
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"更新用戶資料時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("UpdateUserProfile")
.WithOpenApi()
.WithSummary("更新用戶基本資料")
.WithDescription("更新當前用戶的電子郵件和顯示名稱");

// PUT /auth/change-password - 更換密碼
app.MapPut("/auth/change-password", async (ChangePasswordRequest request, ClaimsPrincipal user, IUserService userService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var result = await userService.ChangePasswordAsync(userId, request);
        
        if (result.Success)
        {
            return Results.Ok(new { 
                message = result.Message
            });
        }
        else
        {
            return Results.BadRequest(new { 
                message = result.Message,
                errors = result.Errors
            });
        }
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"更換密碼時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("ChangePassword")
.WithOpenApi()
.WithSummary("更換用戶密碼")
.WithDescription("更換當前用戶的密碼，需要驗證目前密碼");

// ========== 待辦事項 API 端點 ==========

// GET /todos/search - 取得當前用戶的待辦事項 (帶篩選和分頁)
app.MapGet("/todos/search", async (
    ClaimsPrincipal user, 
    ITodoService todoService,
    bool? isCompleted = null,
    bool includeDeleted = false,
    string? category = null,
            int? priority = null,
    string sortBy = "CreatedDate",
    bool sortAscending = false,
    int page = 1,
    int pageSize = 20) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var query = new TodoQueryRequest
        {
            IsCompleted = isCompleted,
            IncludeDeleted = includeDeleted,
            Category = category,
            Priority = priority,
            SortBy = sortBy,
            SortAscending = sortAscending,
            Page = page,
            PageSize = pageSize
        };
        
        var result = await todoService.GetTodosAsync(userId, query);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("SearchTodos")
.WithOpenApi()
.WithSummary("搜尋待辦事項")
.WithDescription("根據條件搜尋當前用戶的待辦事項，支援篩選、排序和分頁");

// GET /todos - 取得當前用戶的所有待辦事項 (簡化版)
app.MapGet("/todos", async (ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var todos = await todoService.GetAllTodosAsync(userId);
    return Results.Ok(todos);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("GetTodos")
.WithOpenApi()
.WithSummary("取得所有待辦事項")
.WithDescription("返回當前用戶的所有待辦事項，按建立時間倒序排列，不包含已刪除項目");

// GET /todos/completed - 取得當前用戶的已完成待辦事項
app.MapGet("/todos/completed", async (ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var todos = await todoService.GetCompletedTodosAsync(userId);
        return Results.Ok(todos);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得已完成待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("GetCompletedTodos")
.WithOpenApi()
.WithSummary("取得已完成待辦事項")
.WithDescription("返回當前用戶的所有已完成待辦事項，不包含已刪除項目");

// GET /todos/pending - 取得當前用戶的未完成待辦事項
app.MapGet("/todos/pending", async (ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var todos = await todoService.GetPendingTodosAsync(userId);
        return Results.Ok(todos);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得未完成待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("GetPendingTodos")
.WithOpenApi()
.WithSummary("取得未完成待辦事項")
.WithDescription("返回當前用戶的所有未完成待辦事項，按優先級倒序排列，不包含已刪除項目");

// GET /todos/deleted - 取得當前用戶的已刪除待辦事項 (回收站)
app.MapGet("/todos/deleted", async (ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var todos = await todoService.GetDeletedTodosAsync(userId);
        return Results.Ok(todos);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得已刪除待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("GetDeletedTodos")
.WithOpenApi()
.WithSummary("取得已刪除待辦事項")
.WithDescription("返回當前用戶的所有已刪除待辦事項 (回收站功能)");

// GET /todos/categories - 取得當前用戶的所有分類
app.MapGet("/todos/categories", async (ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var categories = await todoService.GetCategoriesAsync(userId);
        return Results.Ok(categories);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得分類時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("GetCategories")
.WithOpenApi()
.WithSummary("取得所有分類")
.WithDescription("返回當前用戶使用過的所有分類名稱");

// GET /todos/statistics - 取得當前用戶的待辦事項統計
app.MapGet("/todos/statistics", async (ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var statistics = await todoService.GetTodoStatisticsAsync(userId);
        return Results.Ok(statistics);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得統計資訊時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("GetTodoStatistics")
.WithOpenApi()
.WithSummary("取得待辦事項統計")
.WithDescription("返回當前用戶的待辦事項統計資訊，包含總數、完成率等");

// GET /todos/{id} - 根據 UUID 取得當前用戶的特定待辦事項
app.MapGet("/todos/{id:guid}", async (Guid id, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var todo = await todoService.GetTodoByIdAsync(userId, id);
    return todo != null ? Results.Ok(todo) : Results.NotFound($"找不到 ID 為 {id} 的待辦事項");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"取得待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("GetTodoById")
.WithOpenApi()
.WithSummary("取得特定待辦事項")
.WithDescription("根據UUID取得當前用戶的單一待辦事項，不包含已刪除項目");

// POST /todos - 為當前用戶建立新的待辦事項
app.MapPost("/todos", async (CreateTodoRequest request, ClaimsPrincipal user, ITodoService todoService) =>
{
    // 驗證請求資料
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("標題不能為空");
    }

    try
    {
        var userId = GetCurrentUserId(user);
        var todo = await todoService.CreateTodoAsync(userId, request);
        return Results.Created($"/todos/{todo.Id}", todo);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"建立待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("CreateTodo")
.WithOpenApi()
.WithSummary("建立新待辦事項")
.WithDescription("為當前用戶建立一個新的待辦事項，支援標題、描述、優先級、分類等欄位");

// PUT /todos/{id} - 更新當前用戶的現有待辦事項
app.MapPut("/todos/{id:guid}", async (Guid id, UpdateTodoRequest request, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var todo = await todoService.UpdateTodoAsync(userId, id, request);
        return todo != null ? Results.Ok(todo) : Results.NotFound($"找不到 ID 為 {id} 的待辦事項");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"更新待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("UpdateTodo")
.WithOpenApi()
.WithSummary("更新待辦事項")
.WithDescription("更新當前用戶的現有待辦事項，支援部分更新，自動更新 UpdatedDate");

// DELETE /todos/{id}/soft - 軟刪除當前用戶的指定待辦事項
app.MapDelete("/todos/{id:guid}/soft", async (Guid id, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var success = await todoService.SoftDeleteTodoAsync(userId, id);
        return success ? Results.NoContent() : Results.NotFound($"找不到 ID 為 {id} 的待辦事項");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"軟刪除待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("SoftDeleteTodo")
.WithOpenApi()
.WithSummary("軟刪除待辦事項")
.WithDescription("軟刪除當前用戶的指定待辦事項，項目會移至回收站，可以恢復");

// DELETE /todos/{id} - 永久刪除當前用戶的指定待辦事項
app.MapDelete("/todos/{id:guid}", async (Guid id, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var success = await todoService.DeleteTodoAsync(userId, id);
        return success ? Results.NoContent() : Results.NotFound($"找不到 ID 為 {id} 的待辦事項");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"永久刪除待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("DeleteTodo")
.WithOpenApi()
.WithSummary("永久刪除待辦事項")
.WithDescription("永久刪除當前用戶的指定待辦事項，無法恢復");

// PATCH /todos/{id}/restore - 恢復已軟刪除的待辦事項
app.MapPatch("/todos/{id:guid}/restore", async (Guid id, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var success = await todoService.RestoreTodoAsync(userId, id);
        if (!success)
        {
            return Results.NotFound($"找不到 ID 為 {id} 的已刪除待辦事項");
        }

        // 取得恢復後的待辦事項並返回
        var todo = await todoService.GetTodoByIdAsync(userId, id);
        return Results.Ok(todo);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"恢復待辦事項時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("RestoreTodo")
.WithOpenApi()
.WithSummary("恢復待辦事項")
.WithDescription("從回收站恢復已軟刪除的待辦事項");

// PATCH /todos/{id}/toggle - 切換當前用戶的待辦事項完成狀態
app.MapPatch("/todos/{id:guid}/toggle", async (Guid id, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var success = await todoService.ToggleCompletionAsync(userId, id);
        if (!success)
        {
            return Results.NotFound($"找不到 ID 為 {id} 的待辦事項");
        }

        // 取得更新後的待辦事項並返回
        var todo = await todoService.GetTodoByIdAsync(userId, id);
        return Results.Ok(todo);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"切換完成狀態時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("ToggleTodoCompletion")
.WithOpenApi()
.WithSummary("切換完成狀態")
.WithDescription("切換當前用戶的待辦事項完成/未完成狀態，自動更新 CompletedDate 和 UpdatedDate");

// POST /todos/batch/complete - 批量標記多個待辦事項為已完成
app.MapPost("/todos/batch/complete", async (BatchOperationRequest request, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var updatedCount = await todoService.BatchUpdateCompletionAsync(userId, request.TodoIds, true);
        return Results.Ok(new { UpdatedCount = updatedCount, Message = $"成功標記 {updatedCount} 個項目為已完成" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"批量完成操作時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("BatchCompleteTodos")
.WithOpenApi()
.WithSummary("批量標記為已完成")
.WithDescription("批量將多個待辦事項標記為已完成。請求體格式: {\"todoIds\": [\"uuid1\", \"uuid2\"]}");

// POST /todos/batch/uncomplete - 批量標記多個待辦事項為未完成
app.MapPost("/todos/batch/uncomplete", async (BatchOperationRequest request, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var updatedCount = await todoService.BatchUpdateCompletionAsync(userId, request.TodoIds, false);
        return Results.Ok(new { UpdatedCount = updatedCount, Message = $"成功標記 {updatedCount} 個項目為未完成" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"批量取消完成操作時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("BatchUncompleteTodos")
.WithOpenApi()
.WithSummary("批量標記為未完成")
.WithDescription("批量將多個待辦事項標記為未完成。請求體格式: {\"todoIds\": [\"uuid1\", \"uuid2\"]}");

// POST /todos/batch/soft-delete - 批量軟刪除多個待辦事項 (移至回收站)
app.MapPost("/todos/batch/soft-delete", async (BatchOperationRequest request, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var deletedCount = await todoService.BatchSoftDeleteAsync(userId, request.TodoIds);
        return Results.Ok(new { DeletedCount = deletedCount, Message = $"成功將 {deletedCount} 個項目移至回收站" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"批量軟刪除操作時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("BatchSoftDeleteTodos")
.WithOpenApi()
.WithSummary("批量軟刪除 (移至回收站)")
.WithDescription("批量軟刪除多個待辦事項，項目會移至回收站，可以恢復。請求體格式: {\"todoIds\": [\"uuid1\", \"uuid2\"]}");

// POST /todos/batch/permanent-delete - 批量永久刪除多個待辦事項
app.MapPost("/todos/batch/permanent-delete", async (BatchOperationRequest request, ClaimsPrincipal user, ITodoService todoService) =>
{
    try
    {
        var userId = GetCurrentUserId(user);
        var deletedCount = await todoService.BatchPermanentDeleteAsync(userId, request.TodoIds);
        return Results.Ok(new { DeletedCount = deletedCount, Message = $"成功永久刪除 {deletedCount} 個項目" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"批量永久刪除操作時發生錯誤: {ex.Message}");
    }
})
.RequireAuthorization()
.WithName("BatchPermanentDeleteTodos")
.WithOpenApi()
.WithSummary("批量永久刪除")
.WithDescription("批量永久刪除多個待辦事項，從資料庫中實際刪除，無法恢復，主要用於清空回收站。請求體格式: {\"todoIds\": [\"uuid1\", \"uuid2\"]}");


// ========== 根路徑處理 ==========
// 根路徑重定向到 Swagger 文件（不在 Swagger 文件中顯示）
app.MapGet("/", () => Results.Redirect("/swagger"))
    .WithName("Root")
    .ExcludeFromDescription(); // 從 Swagger 文件中排除

// 健康檢查端點（不在 Swagger 文件中顯示）
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    service = "TodoListApi",
    version = "1.0.0"
}))
.WithName("HealthCheck")
.ExcludeFromDescription(); // 從 Swagger 文件中排除



// 啟動應用程式
// 開始監聽 HTTP 請求
app.Run();
