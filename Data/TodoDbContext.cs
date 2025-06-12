using Microsoft.EntityFrameworkCore;
using TodoListApi.Models;

namespace TodoListApi.Data;

/// <summary>
/// Entity Framework Core 資料庫上下文類別
/// 負責管理 TodoList API 應用程式的資料庫連線和實體對應
/// 使用 Code First 方式進行資料庫模型管理
/// </summary>
public class TodoDbContext : DbContext
{
    /// <summary>
    /// TodoDbContext 建構函式
    /// 透過依賴注入接收資料庫配置選項
    /// </summary>
    /// <param name="options">Entity Framework Core 的配置選項</param>
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    /// <summary>
    /// 待辦事項資料集
    /// 對應資料庫中的 Todos 表格
    /// 包含用戶的所有待辦事項資料
    /// </summary>
    public DbSet<TodoItem> Todos { get; set; }
    
    /// <summary>
    /// 用戶資料集
    /// 對應資料庫中的 Users 表格
    /// 包含系統中的所有用戶帳號資料
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 設定實體模型的詳細配置
    /// 定義資料表結構、關聯關係、索引、約束等
    /// 此方法在資料庫初始化時自動執行
    /// </summary>
    /// <param name="modelBuilder">Entity Framework Core 的模型建構器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === 用戶實體配置 ===
        modelBuilder.Entity<User>(entity =>
        {
            // 設定表格名稱
            entity.ToTable("Users");
            
            // 設定主鍵使用 UUID，由 PostgreSQL 自動產生
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // 註：移除 Username 欄位，只使用電子郵件作為登入帳號

            // 設定電子郵件為必填且唯一
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            // 設定密碼雜湊為必填
            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            // 設定顯示名稱為選填
            entity.Property(u => u.DisplayName)
                .HasMaxLength(100);

            // 設定註冊時間預設值
            entity.Property(u => u.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 設定帳號啟用狀態預設為 true
            entity.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        });

        // === 待辦事項實體配置 ===
        modelBuilder.Entity<TodoItem>(entity =>
        {
            // 設定表格名稱
            entity.ToTable("Todos");
            
            // 設定主鍵使用 UUID，由 PostgreSQL 自動產生
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // 設定用戶 ID 外鍵為必填
            entity.Property(t => t.UserId)
                .IsRequired();

            // 設定標題為必填，最大長度 200 字元
            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            // 設定描述為選填，最大長度 1000 字元
            entity.Property(t => t.Description)
                .HasMaxLength(1000);

            // 設定完成狀態預設為 false
            entity.Property(t => t.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            // 設定建立時間預設為當前時間
            entity.Property(t => t.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 設定更新時間預設為當前時間
            entity.Property(t => t.UpdatedDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 設定完成時間為選填
            entity.Property(t => t.CompletedDate);

            // 設定軟刪除狀態預設為 false
            entity.Property(t => t.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // 設定優先級預設為 0
            entity.Property(t => t.Priority)
                .IsRequired()
                .HasDefaultValue(0);

            // 設定分類為選填，最大長度 50 字元
            entity.Property(t => t.Category)
                .HasMaxLength(50);

            // === 建立索引以提升查詢效能 ===
            
            // 用戶 ID 索引 (最重要的查詢條件)
            entity.HasIndex(t => t.UserId)
                .HasDatabaseName("IX_Todos_UserId");

            // 用戶 ID + 軟刪除狀態組合索引 (常用查詢組合)
            entity.HasIndex(t => new { t.UserId, t.IsDeleted })
                .HasDatabaseName("IX_Todos_UserId_IsDeleted");

            // 用戶 ID + 完成狀態組合索引 (篩選查詢)
            entity.HasIndex(t => new { t.UserId, t.IsCompleted })
                .HasDatabaseName("IX_Todos_UserId_IsCompleted");

            // 用戶 ID + 分類組合索引 (分類篩選)
            entity.HasIndex(t => new { t.UserId, t.Category })
                .HasDatabaseName("IX_Todos_UserId_Category");

            // 用戶 ID + 優先級組合索引 (優先級排序)
            entity.HasIndex(t => new { t.UserId, t.Priority })
                .HasDatabaseName("IX_Todos_UserId_Priority");

            // 建立時間索引 (時間排序)
            entity.HasIndex(t => t.CreatedDate)
                .HasDatabaseName("IX_Todos_CreatedDate");

            // 更新時間索引 (最近更新排序)
            entity.HasIndex(t => t.UpdatedDate)
                .HasDatabaseName("IX_Todos_UpdatedDate");
        });

        // === 設定實體關聯關係 ===
        
        // 用戶與待辦事項的一對多關聯
        modelBuilder.Entity<TodoItem>()
            .HasOne(t => t.User)                    // 每個待辦事項屬於一個用戶
            .WithMany(u => u.TodoItems)             // 每個用戶可以有多個待辦事項
            .HasForeignKey(t => t.UserId)           // 外鍵是 UserId
            .HasConstraintName("FK_Todos_Users_UserId")  // 自訂外鍵約束名稱
            .OnDelete(DeleteBehavior.Cascade);      // 級聯刪除：刪除用戶時同時刪除其所有待辦事項
    }
} 