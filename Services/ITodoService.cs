using TodoListApi.Models;

namespace TodoListApi.Services;

/// <summary>
/// 待辦事項服務的介面定義
/// 定義了所有待辦事項相關的業務邏輯方法
/// 使用介面可以實現依賴注入和單元測試的解耦
/// 所有方法都需要 userId 參數以確保用戶只能操作自己的資料
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// 取得指定用戶的待辦事項 (帶篩選和分頁)
    /// 支援完成狀態、分類、優先級篩選，以及多種排序方式
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="query">查詢篩選條件</param>
    /// <returns>分頁的待辦事項結果</returns>
    Task<TodoPagedResponse> GetTodosAsync(Guid userId, TodoQueryRequest query);
    
    /// <summary>
    /// 取得指定用戶的所有待辦事項 (簡化版)
    /// 返回按建立時間倒序排列的待辦事項清單，不包含已刪除項目
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <returns>該用戶的待辦事項集合的非同步任務</returns>
    Task<IEnumerable<TodoItem>> GetAllTodosAsync(Guid userId);
    
    /// <summary>
    /// 根據 UUID 取得指定用戶的特定待辦事項
    /// 確保用戶只能存取自己的待辦事項，不包含已刪除項目
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoId">待辦事項的 UUID 唯一識別碼</param>
    /// <returns>如果找到且屬於該用戶則返回待辦事項，否則返回 null 的非同步任務</returns>
    Task<TodoItem?> GetTodoByIdAsync(Guid userId, Guid todoId);
    
    /// <summary>
    /// 為指定用戶建立新的待辦事項
    /// 自動設定 UserId 為當前用戶，CreatedDate 和 UpdatedDate 為當前時間
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="request">建立待辦事項的請求資料</param>
    /// <returns>建立成功的待辦事項的非同步任務</returns>
    Task<TodoItem> CreateTodoAsync(Guid userId, CreateTodoRequest request);
    
    /// <summary>
    /// 更新指定用戶的現有待辦事項
    /// 只更新請求中有提供值的欄位，自動更新 UpdatedDate
    /// 確保用戶只能更新自己的待辦事項
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoId">要更新的待辦事項 UUID</param>
    /// <param name="request">更新待辦事項的請求資料</param>
    /// <returns>如果更新成功則返回更新後的待辦事項，否則返回 null 的非同步任務</returns>
    Task<TodoItem?> UpdateTodoAsync(Guid userId, Guid todoId, UpdateTodoRequest request);
    
    /// <summary>
    /// 軟刪除指定用戶的待辦事項
    /// 設定 IsDeleted 為 true，不從資料庫中實際刪除
    /// 確保用戶只能刪除自己的待辦事項
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoId">要刪除的待辦事項 UUID</param>
    /// <returns>如果刪除成功則返回 true，否則返回 false 的非同步任務</returns>
    Task<bool> SoftDeleteTodoAsync(Guid userId, Guid todoId);
    
    /// <summary>
    /// 永久刪除指定用戶的待辦事項
    /// 從資料庫中實際刪除資料，無法恢復
    /// 確保用戶只能刪除自己的待辦事項
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoId">要永久刪除的待辦事項 UUID</param>
    /// <returns>如果刪除成功則返回 true，否則返回 false 的非同步任務</returns>
    Task<bool> DeleteTodoAsync(Guid userId, Guid todoId);
    
    /// <summary>
    /// 恢復已軟刪除的待辦事項
    /// 設定 IsDeleted 為 false，恢復項目的可見性
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoId">要恢復的待辦事項 UUID</param>
    /// <returns>如果恢復成功則返回 true，否則返回 false 的非同步任務</returns>
    Task<bool> RestoreTodoAsync(Guid userId, Guid todoId);
    
    /// <summary>
    /// 切換指定用戶的待辦事項完成狀態
    /// 如果目前是已完成則改為未完成，如果是未完成則改為已完成
    /// 同時會自動更新 CompletedDate 和 UpdatedDate 欄位
    /// 確保用戶只能操作自己的待辦事項
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoId">要切換狀態的待辦事項 UUID</param>
    /// <returns>如果切換成功則返回 true，否則返回 false 的非同步任務</returns>
    Task<bool> ToggleCompletionAsync(Guid userId, Guid todoId);
    
    /// <summary>
    /// 取得指定用戶的待辦事項統計資訊
    /// 包含總數、已完成數、各優先級分布等資訊
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <returns>包含總數、已完成數、未完成數的統計資訊</returns>
    Task<TodoStatistics> GetTodoStatisticsAsync(Guid userId);
    
    /// <summary>
    /// 取得指定用戶的已完成待辦事項
    /// 不包含已刪除項目
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <returns>已完成的待辦事項集合</returns>
    Task<IEnumerable<TodoItem>> GetCompletedTodosAsync(Guid userId);
    
    /// <summary>
    /// 取得指定用戶的未完成待辦事項
    /// 不包含已刪除項目
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <returns>未完成的待辦事項集合</returns>
    Task<IEnumerable<TodoItem>> GetPendingTodosAsync(Guid userId);
    
    /// <summary>
    /// 取得指定用戶的所有分類清單
    /// 返回該用戶使用過的所有分類名稱
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <returns>分類名稱集合</returns>
    Task<IEnumerable<string>> GetCategoriesAsync(Guid userId);
    
    /// <summary>
    /// 取得指定用戶的已刪除待辦事項
    /// 用於回收站功能
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <returns>已刪除的待辦事項集合</returns>
    Task<IEnumerable<TodoItem>> GetDeletedTodosAsync(Guid userId);
    
    /// <summary>
    /// 批量更新多個待辦事項的完成狀態
    /// 可以同時標記多個項目為已完成或未完成
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoIds">要更新的待辦事項 UUID 清單</param>
    /// <param name="isCompleted">要設定的完成狀態</param>
    /// <returns>成功更新的項目數量</returns>
    Task<int> BatchUpdateCompletionAsync(Guid userId, IEnumerable<Guid> todoIds, bool isCompleted);
    
    /// <summary>
    /// 批量軟刪除多個待辦事項 (移至回收站)
    /// 可以同時軟刪除多個項目，設定 IsDeleted 為 true
    /// 項目可以透過 RestoreTodoAsync 恢復
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoIds">要軟刪除的待辦事項 UUID 清單</param>
    /// <returns>成功軟刪除的項目數量</returns>
    Task<int> BatchSoftDeleteAsync(Guid userId, IEnumerable<Guid> todoIds);
    
    /// <summary>
    /// 批量永久刪除多個待辦事項
    /// 從資料庫中實際刪除資料，無法恢復
    /// 主要用於清空回收站功能
    /// </summary>
    /// <param name="userId">用戶的 UUID 唯一識別碼</param>
    /// <param name="todoIds">要永久刪除的待辦事項 UUID 清單</param>
    /// <returns>成功永久刪除的項目數量</returns>
    Task<int> BatchPermanentDeleteAsync(Guid userId, IEnumerable<Guid> todoIds);
} 