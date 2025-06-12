using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApi.Models;

/// <summary>
/// 待辦事項實體類別
/// 對應資料庫中的 Todos 表格
/// </summary>
[Table("Todos")]
public class TodoItem
{
    /// <summary>
    /// 待辦事項的唯一識別碼 (主鍵)
    /// 使用 UUID (Guid) 格式確保全域唯一性
    /// 由資料庫層自動產生 (gen_random_uuid())
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// 待辦事項所屬的用戶 ID (外鍵)
    /// 用於建立用戶與待辦事項的關聯關係
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 導航屬性：待辦事項所屬的用戶
    /// Entity Framework 用於建立關聯關係
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// 待辦事項的標題
    /// 必填欄位，最大長度 200 字元
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 待辦事項的詳細描述
    /// 選填欄位，最大長度 1000 字元
    /// 可以為 null
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 待辦事項是否已完成
    /// 預設值為 false (未完成)
    /// </summary>
    public bool IsCompleted { get; set; } = false;
    
    /// <summary>
    /// 待辦事項的建立時間
    /// 必填欄位，使用 UTC 時間
    /// 預設值為當前 UTC 時間
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 待辦事項的最後更新時間
    /// 必填欄位，使用 UTC 時間
    /// 每次更新時自動設定為當前時間
    /// </summary>
    [Required]
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 待辦事項的完成時間
    /// 選填欄位，只有當 IsCompleted 為 true 時才有值
    /// 可以為 null
    /// </summary>
    public DateTime? CompletedDate { get; set; }
    
    /// <summary>
    /// 待辦事項是否已刪除 (軟刪除)
    /// 預設值為 false (未刪除)
    /// 實現軟刪除功能，可以恢復已刪除的項目
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// 待辦事項的優先級
    /// 數值越大優先級越高
    /// 0: 普通 (預設)
    /// 1: 低優先級
    /// 2: 中優先級  
    /// 3: 高優先級
    /// 4: 緊急
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// 待辦事項的分類
    /// 選填欄位，最大長度 50 字元
    /// 例如：工作、個人、學習、購物等
    /// </summary>
    [StringLength(50)]
    public string? Category { get; set; }
}

/// <summary>
/// 建立待辦事項請求模型
/// 用於 POST /todos API 端點
/// </summary>
public class CreateTodoRequest
{
    /// <summary>
    /// 待辦事項的標題
    /// 必填欄位，最大長度 200 字元
    /// </summary>
    [Required(ErrorMessage = "標題為必填")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題長度必須在 1-200 字元之間")]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 待辦事項的詳細描述
    /// 選填欄位，最大長度 1000 字元
    /// </summary>
    [StringLength(1000, ErrorMessage = "描述長度不能超過 1000 字元")]
    public string? Description { get; set; }
    
    /// <summary>
    /// 待辦事項的優先級
    /// 可選欄位，預設為 0 (普通優先級)
    /// 有效值：0-4
    /// </summary>
    [Range(0, 4, ErrorMessage = "優先級必須在 0-4 之間")]
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// 待辦事項的分類
    /// 選填欄位，最大長度 50 字元
    /// </summary>
    [StringLength(50, ErrorMessage = "分類長度不能超過 50 字元")]
    public string? Category { get; set; }
}

/// <summary>
/// 更新待辦事項請求模型
/// 用於 PUT /todos/{id} API 端點
/// 所有欄位都是選填的，只更新提供的欄位
/// </summary>
public class UpdateTodoRequest
{
    /// <summary>
    /// 待辦事項的標題
    /// 選填欄位，如果提供則必須在 1-200 字元之間
    /// </summary>
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題長度必須在 1-200 字元之間")]
    public string? Title { get; set; }
    
    /// <summary>
    /// 待辦事項的詳細描述
    /// 選填欄位，如果提供則最大長度 1000 字元
    /// </summary>
    [StringLength(1000, ErrorMessage = "描述長度不能超過 1000 字元")]
    public string? Description { get; set; }
    
    /// <summary>
    /// 待辦事項的完成狀態
    /// 選填欄位，如果提供則更新完成狀態
    /// </summary>
    public bool? IsCompleted { get; set; }
    
    /// <summary>
    /// 待辦事項的優先級
    /// 選填欄位，如果提供則必須在 0-4 之間
    /// </summary>
    [Range(0, 4, ErrorMessage = "優先級必須在 0-4 之間")]
    public int? Priority { get; set; }
    
    /// <summary>
    /// 待辦事項的分類
    /// 選填欄位，如果提供則最大長度 50 字元
    /// </summary>
    [StringLength(50, ErrorMessage = "分類長度不能超過 50 字元")]
    public string? Category { get; set; }
}

/// <summary>
/// 待辦事項查詢請求模型
/// 用於 GET /todos API 端點的查詢參數
/// </summary>
public class TodoQueryRequest
{
    /// <summary>
    /// 過濾完成狀態
    /// null: 不過濾, true: 只顯示已完成, false: 只顯示未完成
    /// </summary>
    public bool? IsCompleted { get; set; }
    
    /// <summary>
    /// 是否包含已刪除的項目
    /// 預設為 false (不包含)
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;
    
    /// <summary>
    /// 過濾分類
    /// 可選欄位，只顯示特定分類的待辦事項
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 過濾優先級
    /// 可選欄位，只顯示特定優先級的待辦事項
    /// </summary>
    [Range(0, 4, ErrorMessage = "優先級必須在 0-4 之間")]
    public int? Priority { get; set; }
    
    /// <summary>
    /// 排序欄位
    /// 預設按建立時間排序，可選：CreatedDate, UpdatedDate, Title, Priority
    /// </summary>
    public string SortBy { get; set; } = "CreatedDate";
    
    /// <summary>
    /// 是否升序排列
    /// 預設為 false (降序)
    /// </summary>
    public bool SortAscending { get; set; } = false;
    
    /// <summary>
    /// 頁碼
    /// 從 1 開始，預設為第 1 頁
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於 0")]
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每頁項目數
    /// 預設 10 項，最大 100 項
    /// </summary>
    [Range(1, 100, ErrorMessage = "每頁項目數必須在 1-100 之間")]
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// 分頁回應模型
/// 包含分頁資料和分頁資訊
/// </summary>
public class TodoPagedResponse
{
    /// <summary>
    /// 當前頁的待辦事項清單
    /// </summary>
    public List<TodoItem> Items { get; set; } = new();
    
    /// <summary>
    /// 總項目數
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 總頁數
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// 當前頁碼 (從 1 開始)
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// 每頁項目數
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 是否有上一頁
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;
    
    /// <summary>
    /// 是否有下一頁
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}

/// <summary>
/// 待辦事項統計資料模型
/// 用於 GET /todos/statistics API 端點
/// </summary>
public class TodoStatistics
{
    /// <summary>
    /// 總項目數
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 已完成項目數
    /// </summary>
    public int CompletedCount { get; set; }
    
    /// <summary>
    /// 未完成項目數
    /// </summary>
    public int PendingCount { get; set; }
    
    /// <summary>
    /// 已刪除項目數
    /// </summary>
    public int DeletedCount { get; set; }
    
    /// <summary>
    /// 完成率 (百分比)
    /// </summary>
    public double CompletionRate { get; set; }
    
    /// <summary>
    /// 本週新增項目數
    /// </summary>
    public int ThisWeekCount { get; set; }
    
    /// <summary>
    /// 本週完成項目數
    /// </summary>
    public int ThisWeekCompletedCount { get; set; }
    
    /// <summary>
    /// 本月新增項目數
    /// </summary>
    public int ThisMonthCount { get; set; }
    
    /// <summary>
    /// 本月完成項目數
    /// </summary>
    public int ThisMonthCompletedCount { get; set; }
    
    /// <summary>
    /// 優先級分佈統計
    /// </summary>
    public PriorityDistribution PriorityStats { get; set; } = new();
    
    /// <summary>
    /// 分類統計
    /// </summary>
    public IEnumerable<CategoryStats> CategoryStats { get; set; } = new List<CategoryStats>();
    
    /// <summary>
    /// 平均每日完成數
    /// </summary>
    public double AverageCompletionPerDay { get; set; }
    
    /// <summary>
    /// 最長連續完成天數
    /// </summary>
    public int LongestCompletionStreak { get; set; }
    
    /// <summary>
    /// 當前連續完成天數
    /// </summary>
    public int CurrentCompletionStreak { get; set; }
}

/// <summary>
/// 優先級分佈統計
/// </summary>
public class PriorityDistribution
{
    /// <summary>
    /// 普通優先級項目數
    /// </summary>
    public int Normal { get; set; }
    
    /// <summary>
    /// 低優先級項目數
    /// </summary>
    public int Low { get; set; }
    
    /// <summary>
    /// 中優先級項目數
    /// </summary>
    public int Medium { get; set; }
    
    /// <summary>
    /// 高優先級項目數
    /// </summary>
    public int High { get; set; }
    
    /// <summary>
    /// 緊急優先級項目數
    /// </summary>
    public int Urgent { get; set; }
}

/// <summary>
/// 分類統計
/// </summary>
public class CategoryStats
{
    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 該分類總項目數
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 該分類已完成項目數
    /// </summary>
    public int CompletedCount { get; set; }
    
    /// <summary>
    /// 該分類未完成項目數
    /// </summary>
    public int PendingCount { get; set; }
    
    /// <summary>
    /// 該分類完成率 (百分比)
    /// </summary>
    public double CompletionRate { get; set; }
}

/// <summary>
/// 批量操作請求模型
/// 用於批量操作 API 端點
/// </summary>
public class BatchOperationRequest
{
    /// <summary>
    /// 要操作的待辦事項 UUID 清單
    /// </summary>
    [Required(ErrorMessage = "待辦事項 ID 清單為必填")]
    public List<Guid> TodoIds { get; set; } = new();
} 