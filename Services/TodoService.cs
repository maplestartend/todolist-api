using Microsoft.EntityFrameworkCore;
using TodoListApi.Data;
using TodoListApi.Models;

namespace TodoListApi.Services;

/// <summary>
/// 待辦事項服務的實作類別
/// 實作 ITodoService 介面，提供所有待辦事項相關的業務邏輯
/// 使用 Entity Framework Core 進行資料庫操作
/// 支援多用戶功能，確保用戶只能操作自己的資料
/// </summary>
public class TodoService : ITodoService
{
    /// <summary>
    /// 資料庫上下文實例
    /// 用於執行所有資料庫相關操作
    /// </summary>
    private readonly TodoDbContext _context;
    
    /// <summary>
    /// 日誌記錄器
    /// 用於記錄服務操作和錯誤
    /// </summary>
    private readonly ILogger<TodoService> _logger;

    /// <summary>
    /// 建構函式
    /// 透過依賴注入接收 TodoDbContext 實例和日誌記錄器
    /// </summary>
    /// <param name="context">資料庫上下文實例</param>
    /// <param name="logger">日誌記錄器</param>
    public TodoService(TodoDbContext context, ILogger<TodoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 取得指定用戶的待辦事項 (帶篩選和分頁)
    /// 支援完成狀態、分類、優先級篩選，以及多種排序方式
    /// </summary>
    public async Task<TodoPagedResponse> GetTodosAsync(Guid userId, TodoQueryRequest query)
    {
        try
        {
            var todoQuery = _context.Todos.Where(t => t.UserId == userId);

            // 軟刪除篩選
            if (!query.IncludeDeleted)
            {
                todoQuery = todoQuery.Where(t => !t.IsDeleted);
            }

            // 完成狀態篩選
            if (query.IsCompleted.HasValue)
            {
                todoQuery = todoQuery.Where(t => t.IsCompleted == query.IsCompleted.Value);
            }

            // 分類篩選
            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                todoQuery = todoQuery.Where(t => t.Category == query.Category);
            }

            // 優先級篩選
            if (query.Priority.HasValue)
            {
                todoQuery = todoQuery.Where(t => t.Priority == query.Priority.Value);
            }

            // 排序
            todoQuery = query.SortBy.ToLower() switch
            {
                "updatedate" => query.SortAscending 
                    ? todoQuery.OrderBy(t => t.UpdatedDate) 
                    : todoQuery.OrderByDescending(t => t.UpdatedDate),
                "priority" => query.SortAscending 
                    ? todoQuery.OrderBy(t => t.Priority) 
                    : todoQuery.OrderByDescending(t => t.Priority),
                "title" => query.SortAscending 
                    ? todoQuery.OrderBy(t => t.Title) 
                    : todoQuery.OrderByDescending(t => t.Title),
                _ => query.SortAscending 
                    ? todoQuery.OrderBy(t => t.CreatedDate) 
                    : todoQuery.OrderByDescending(t => t.CreatedDate)
            };

            // 取得總數
            var totalCount = await todoQuery.CountAsync();

            // 分頁
            var items = await todoQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            var response = new TodoPagedResponse
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = query.Page,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };

            _logger.LogInformation("取得用戶 {UserId} 的 {Count}/{TotalCount} 個待辦事項 (第 {Page} 頁)", 
                userId, items.Count, totalCount, query.Page);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的待辦事項時發生錯誤", userId);
            return new TodoPagedResponse();
        }
    }

    /// <summary>
    /// 取得指定用戶的所有待辦事項 (簡化版)
    /// 按建立時間倒序排列，最新建立的會顯示在最前面，不包含已刪除項目
    /// </summary>
    public async Task<IEnumerable<TodoItem>> GetAllTodosAsync(Guid userId)
    {
        try
        {
            var todos = await _context.Todos
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            _logger.LogInformation("取得用戶 {UserId} 的 {Count} 個待辦事項", userId, todos.Count);
            return todos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的待辦事項時發生錯誤", userId);
            return new List<TodoItem>();
        }
    }

    /// <summary>
    /// 根據 UUID 取得指定用戶的特定待辦事項
    /// 確保用戶只能存取自己的待辦事項，不包含已刪除項目
    /// </summary>
    public async Task<TodoItem?> GetTodoByIdAsync(Guid userId, Guid todoId)
    {
        try
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId && !t.IsDeleted);

            if (todo == null)
            {
                _logger.LogWarning("用戶 {UserId} 嘗試存取不存在或不屬於自己的待辦事項 {TodoId}", userId, todoId);
            }

            return todo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的待辦事項 {TodoId} 時發生錯誤", userId, todoId);
            return null;
        }
    }

    /// <summary>
    /// 為指定用戶建立新的待辦事項
    /// 自動設定 UserId 為當前用戶，CreatedDate 和 UpdatedDate 為當前時間
    /// </summary>
    public async Task<TodoItem> CreateTodoAsync(Guid userId, CreateTodoRequest request)
    {
        try
        {
            var now = DateTime.UtcNow;
            var todo = new TodoItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Priority = request.Priority,
                Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
                IsCompleted = false,
                IsDeleted = false,
                CreatedDate = now,
                UpdatedDate = now
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("用戶 {UserId} 建立了新的待辦事項 {TodoId}: {Title}", userId, todo.Id, todo.Title);
            return todo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 建立待辦事項時發生錯誤", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新指定用戶的現有待辦事項
    /// 只更新請求中有提供值的欄位，採用部分更新的方式，自動更新 UpdatedDate
    /// 確保用戶只能更新自己的待辦事項
    /// </summary>
    public async Task<TodoItem?> UpdateTodoAsync(Guid userId, Guid todoId, UpdateTodoRequest request)
    {
        try
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId && !t.IsDeleted);
            
            if (todo == null)
            {
                _logger.LogWarning("用戶 {UserId} 嘗試更新不存在或不屬於自己的待辦事項 {TodoId}", userId, todoId);
                return null;
            }

            // 只更新有提供值的欄位
            if (!string.IsNullOrWhiteSpace(request.Title))
                todo.Title = request.Title.Trim();

            if (request.Description != null)
                todo.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            if (request.Priority.HasValue)
                todo.Priority = request.Priority.Value;

            if (request.Category != null)
                todo.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();

            if (request.IsCompleted.HasValue)
            {
                var wasCompleted = todo.IsCompleted;
                todo.IsCompleted = request.IsCompleted.Value;
                
                // 如果完成狀態有變更，更新完成時間
                if (wasCompleted != request.IsCompleted.Value)
                {
                    todo.CompletedDate = request.IsCompleted.Value ? DateTime.UtcNow : null;
                }
            }

            // 自動更新 UpdatedDate
            todo.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("用戶 {UserId} 更新了待辦事項 {TodoId}", userId, todoId);
            return todo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 更新待辦事項 {TodoId} 時發生錯誤", userId, todoId);
            return null;
        }
    }

    /// <summary>
    /// 軟刪除指定用戶的待辦事項
    /// 設定 IsDeleted 為 true，不從資料庫中實際刪除
    /// 確保用戶只能刪除自己的待辦事項
    /// </summary>
    public async Task<bool> SoftDeleteTodoAsync(Guid userId, Guid todoId)
    {
        try
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId && !t.IsDeleted);
            
            if (todo == null)
            {
                _logger.LogWarning("用戶 {UserId} 嘗試刪除不存在或不屬於自己的待辦事項 {TodoId}", userId, todoId);
                return false;
            }

            todo.IsDeleted = true;
            todo.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("用戶 {UserId} 軟刪除了待辦事項 {TodoId}", userId, todoId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 軟刪除待辦事項 {TodoId} 時發生錯誤", userId, todoId);
            return false;
        }
    }

    /// <summary>
    /// 永久刪除指定用戶的待辦事項
    /// 從資料庫中實際刪除資料，無法恢復
    /// 確保用戶只能刪除自己的待辦事項
    /// </summary>
    public async Task<bool> DeleteTodoAsync(Guid userId, Guid todoId)
    {
        try
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);
            
            if (todo == null)
            {
                _logger.LogWarning("用戶 {UserId} 嘗試永久刪除不存在或不屬於自己的待辦事項 {TodoId}", userId, todoId);
                return false;
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("用戶 {UserId} 永久刪除了待辦事項 {TodoId}", userId, todoId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 永久刪除待辦事項 {TodoId} 時發生錯誤", userId, todoId);
            return false;
        }
    }

    /// <summary>
    /// 恢復已軟刪除的待辦事項
    /// 設定 IsDeleted 為 false，恢復項目的可見性
    /// </summary>
    public async Task<bool> RestoreTodoAsync(Guid userId, Guid todoId)
    {
        try
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId && t.IsDeleted);
            
            if (todo == null)
            {
                _logger.LogWarning("用戶 {UserId} 嘗試恢復不存在或未刪除的待辦事項 {TodoId}", userId, todoId);
                return false;
            }

            todo.IsDeleted = false;
            todo.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("用戶 {UserId} 恢復了待辦事項 {TodoId}", userId, todoId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 恢復待辦事項 {TodoId} 時發生錯誤", userId, todoId);
            return false;
        }
    }

    /// <summary>
    /// 切換指定用戶的待辦事項完成狀態
    /// 如果目前是已完成則改為未完成，如果是未完成則改為已完成
    /// 同時會自動更新 CompletedDate 和 UpdatedDate 欄位
    /// 確保用戶只能操作自己的待辦事項
    /// </summary>
    public async Task<bool> ToggleCompletionAsync(Guid userId, Guid todoId)
    {
        try
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId && !t.IsDeleted);
            
            if (todo == null)
            {
                _logger.LogWarning("用戶 {UserId} 嘗試切換不存在或不屬於自己的待辦事項 {TodoId} 完成狀態", userId, todoId);
                return false;
            }

            var now = DateTime.UtcNow;
            todo.IsCompleted = !todo.IsCompleted;
            todo.CompletedDate = todo.IsCompleted ? now : null;
            todo.UpdatedDate = now;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("用戶 {UserId} 將待辦事項 {TodoId} 標記為 {Status}", 
                userId, todoId, todo.IsCompleted ? "已完成" : "未完成");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 切換待辦事項 {TodoId} 完成狀態時發生錯誤", userId, todoId);
            return false;
        }
    }

    /// <summary>
    /// 取得指定用戶的待辦事項統計資訊
    /// 包含總數、已完成數、各優先級分布等資訊
    /// </summary>
    public async Task<TodoStatistics> GetTodoStatisticsAsync(Guid userId)
    {
        try
        {
            // 取得所有待辦事項 (包含已刪除)
            var allTodos = await _context.Todos
                .Where(t => t.UserId == userId)
                .ToListAsync();

            // 取得活躍的待辦事項 (不包含已刪除)
            var activeTodos = allTodos.Where(t => !t.IsDeleted).ToList();

            var now = DateTime.UtcNow;
            var thisWeekStart = now.AddDays(-(int)now.DayOfWeek);
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var thirtyDaysAgo = now.AddDays(-30);

            // 基本統計
            var totalCount = activeTodos.Count;
            var completedCount = activeTodos.Count(t => t.IsCompleted);
            var pendingCount = totalCount - completedCount;
            var deletedCount = allTodos.Count(t => t.IsDeleted);

            // 時間範圍統計
            var thisWeekTodos = activeTodos.Where(t => t.CreatedDate >= thisWeekStart).ToList();
            var thisMonthTodos = activeTodos.Where(t => t.CreatedDate >= thisMonthStart).ToList();
            
            // 過去30天完成的項目
            var last30DaysCompleted = activeTodos
                .Where(t => t.CompletedDate.HasValue && t.CompletedDate >= thirtyDaysAgo)
                .ToList();

            // 優先級分布統計
            var priorityStats = new PriorityDistribution
            {
                Normal = activeTodos.Count(t => t.Priority == 0),
                Low = activeTodos.Count(t => t.Priority == 1),
                Medium = activeTodos.Count(t => t.Priority == 2),
                High = activeTodos.Count(t => t.Priority == 3),
                Urgent = activeTodos.Count(t => t.Priority == 4)
            };

            // 分類統計
            var categoryStats = activeTodos
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .GroupBy(t => t.Category!)
                .Select(g => new CategoryStats
                {
                    Category = g.Key,
                    TotalCount = g.Count(),
                    CompletedCount = g.Count(t => t.IsCompleted),
                    PendingCount = g.Count(t => !t.IsCompleted),
                    CompletionRate = g.Count() > 0 ? (double)g.Count(t => t.IsCompleted) / g.Count() * 100 : 0
                })
                .OrderByDescending(c => c.TotalCount)
                .ToList();

            // 計算連續完成天數
            var completionStreak = CalculateCompletionStreak(activeTodos);

            var statistics = new TodoStatistics
            {
                TotalCount = totalCount,
                CompletedCount = completedCount,
                PendingCount = pendingCount,
                DeletedCount = deletedCount,
                CompletionRate = totalCount > 0 ? (double)completedCount / totalCount * 100 : 0,
                ThisWeekCount = thisWeekTodos.Count,
                ThisWeekCompletedCount = thisWeekTodos.Count(t => t.IsCompleted),
                ThisMonthCount = thisMonthTodos.Count,
                ThisMonthCompletedCount = thisMonthTodos.Count(t => t.IsCompleted),
                PriorityStats = priorityStats,
                CategoryStats = categoryStats,
                AverageCompletionPerDay = last30DaysCompleted.Count / 30.0,
                LongestCompletionStreak = completionStreak.Longest,
                CurrentCompletionStreak = completionStreak.Current
            };

            _logger.LogInformation("取得用戶 {UserId} 的統計資訊：總計 {Total}，已完成 {Completed}，未完成 {Pending}，已刪除 {Deleted}", 
                userId, statistics.TotalCount, statistics.CompletedCount, statistics.PendingCount, statistics.DeletedCount);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的統計資訊時發生錯誤", userId);
            return new TodoStatistics();
        }
    }

    /// <summary>
    /// 計算用戶的連續完成天數
    /// </summary>
    /// <param name="todos">用戶的待辦事項清單</param>
    /// <returns>包含當前連續天數和最長連續天數的元組</returns>
    private (int Current, int Longest) CalculateCompletionStreak(List<TodoItem> todos)
    {
        try
        {
            // 取得所有有完成日期的項目，按完成日期分組
            var completedByDate = todos
                .Where(t => t.CompletedDate.HasValue)
                .GroupBy(t => t.CompletedDate!.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => g.Key)
                .ToList();

            if (!completedByDate.Any())
                return (0, 0);

            var currentStreak = 0;
            var longestStreak = 0;
            var today = DateTime.UtcNow.Date;

            // 檢查是否昨天或今天有完成項目（當前連續）
            if (completedByDate.Contains(today) || completedByDate.Contains(today.AddDays(-1)))
            {
                var checkDate = completedByDate.Contains(today) ? today : today.AddDays(-1);
                
                while (completedByDate.Contains(checkDate))
                {
                    currentStreak++;
                    checkDate = checkDate.AddDays(-1);
                }
            }

            // 計算最長連續天數
            var tempStreak = 1;
            for (int i = 1; i < completedByDate.Count; i++)
            {
                if (completedByDate[i] == completedByDate[i - 1].AddDays(1))
                {
                    tempStreak++;
                }
                else
                {
                    longestStreak = Math.Max(longestStreak, tempStreak);
                    tempStreak = 1;
                }
            }
            longestStreak = Math.Max(longestStreak, tempStreak);

            return (currentStreak, longestStreak);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "計算連續完成天數時發生錯誤");
            return (0, 0);
        }
    }

    /// <summary>
    /// 取得指定用戶的已完成待辦事項
    /// 不包含已刪除項目
    /// </summary>
    public async Task<IEnumerable<TodoItem>> GetCompletedTodosAsync(Guid userId)
    {
        try
        {
            var todos = await _context.Todos
                .Where(t => t.UserId == userId && t.IsCompleted && !t.IsDeleted)
                .OrderByDescending(t => t.CompletedDate)
                .ToListAsync();

            _logger.LogInformation("取得用戶 {UserId} 的 {Count} 個已完成待辦事項", userId, todos.Count);
            return todos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的已完成待辦事項時發生錯誤", userId);
            return new List<TodoItem>();
        }
    }

    /// <summary>
    /// 取得指定用戶的未完成待辦事項
    /// 不包含已刪除項目
    /// </summary>
    public async Task<IEnumerable<TodoItem>> GetPendingTodosAsync(Guid userId)
    {
        try
        {
            var todos = await _context.Todos
                .Where(t => t.UserId == userId && !t.IsCompleted && !t.IsDeleted)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedDate)
                .ToListAsync();

            _logger.LogInformation("取得用戶 {UserId} 的 {Count} 個未完成待辦事項", userId, todos.Count);
            return todos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的未完成待辦事項時發生錯誤", userId);
            return new List<TodoItem>();
        }
    }

    /// <summary>
    /// 取得指定用戶的所有分類清單
    /// 返回該用戶使用過的所有分類名稱
    /// </summary>
    public async Task<IEnumerable<string>> GetCategoriesAsync(Guid userId)
    {
        try
        {
            var categories = await _context.Todos
                .Where(t => t.UserId == userId && !t.IsDeleted && !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            _logger.LogInformation("取得用戶 {UserId} 的 {Count} 個分類", userId, categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的分類時發生錯誤", userId);
            return new List<string>();
        }
    }

    /// <summary>
    /// 取得指定用戶的已刪除待辦事項
    /// 用於回收站功能
    /// </summary>
    public async Task<IEnumerable<TodoItem>> GetDeletedTodosAsync(Guid userId)
    {
        try
        {
            var todos = await _context.Todos
                .Where(t => t.UserId == userId && t.IsDeleted)
                .OrderByDescending(t => t.UpdatedDate)
                .ToListAsync();

            _logger.LogInformation("取得用戶 {UserId} 的 {Count} 個已刪除待辦事項", userId, todos.Count);
            return todos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的已刪除待辦事項時發生錯誤", userId);
            return new List<TodoItem>();
        }
    }

    /// <summary>
    /// 批量更新多個待辦事項的完成狀態
    /// 可以同時標記多個項目為已完成或未完成
    /// </summary>
    public async Task<int> BatchUpdateCompletionAsync(Guid userId, IEnumerable<Guid> todoIds, bool isCompleted)
    {
        try
        {
            var todos = await _context.Todos
                .Where(t => t.UserId == userId && !t.IsDeleted && todoIds.Contains(t.Id))
                .ToListAsync();

            var now = DateTime.UtcNow;
            var updatedCount = 0;

            foreach (var todo in todos)
            {
                if (todo.IsCompleted != isCompleted)
                {
                    todo.IsCompleted = isCompleted;
                    todo.CompletedDate = isCompleted ? now : null;
                    todo.UpdatedDate = now;
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("用戶 {UserId} 批量更新了 {Count} 個待辦事項的完成狀態為 {Status}", 
                userId, updatedCount, isCompleted ? "已完成" : "未完成");
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 批量更新完成狀態時發生錯誤", userId);
            return 0;
        }
    }

    /// <summary>
    /// 批量軟刪除多個待辦事項 (移至回收站)
    /// 可以同時軟刪除多個項目，設定 IsDeleted 為 true
    /// 項目可以透過 RestoreTodoAsync 恢復
    /// </summary>
    public async Task<int> BatchSoftDeleteAsync(Guid userId, IEnumerable<Guid> todoIds)
    {
        try
        {
            var todos = await _context.Todos
                .Where(t => t.UserId == userId && !t.IsDeleted && todoIds.Contains(t.Id))
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var todo in todos)
            {
                todo.IsDeleted = true;
                todo.UpdatedDate = now;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("用戶 {UserId} 批量軟刪除了 {Count} 個待辦事項", userId, todos.Count);
            return todos.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 批量軟刪除時發生錯誤", userId);
            return 0;
        }
    }

    /// <summary>
    /// 批量永久刪除多個待辦事項
    /// 從資料庫中實際刪除資料，無法恢復
    /// 主要用於清空回收站功能
    /// </summary>
    public async Task<int> BatchPermanentDeleteAsync(Guid userId, IEnumerable<Guid> todoIds)
    {
        try
        {
            var todos = await _context.Todos
                .Where(t => t.UserId == userId && todoIds.Contains(t.Id))
                .ToListAsync();

            if (todos.Count == 0)
            {
                _logger.LogWarning("用戶 {UserId} 嘗試永久刪除不存在或不屬於自己的待辦事項", userId);
                return 0;
            }

            _context.Todos.RemoveRange(todos);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("用戶 {UserId} 批量永久刪除了 {Count} 個待辦事項", userId, todos.Count);
            return todos.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用戶 {UserId} 批量永久刪除時發生錯誤", userId);
            return 0;
        }
    }
} 