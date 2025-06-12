# TodoListApi 架構設計文件

## 概述

本文件詳細說明 TodoListApi 專案的技術架構、設計模式、以及各層級之間的關係。

## 整體架構

### 分層架構 (Layered Architecture)

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer (API 層)                   │
│                     Program.cs                         │
│              • HTTP 端點定義                            │
│              • 中間件配置                               │
│              • 服務註冊                                 │
└─────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────┐
│                 Service Layer (服務層)                  │
│              ITodoService / TodoService                │
│              • 業務邏輯實作                             │
│              • 資料驗證                                 │
│              • 錯誤處理                                 │
└─────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────┐
│                  Data Layer (資料層)                    │
│                   TodoDbContext                        │
│              • Entity Framework 配置                   │
│              • 資料庫對應設定                           │
│              • 查詢最佳化                               │
└─────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────┐
│                 Model Layer (模型層)                    │
│                    TodoItem.cs                         │
│              • 實體定義                                 │
│              • 資料驗證規則                             │
│              • DTO 物件                                 │
└─────────────────────────────────────────────────────────┘
```

## 詳細設計

### 1. API 層 (Program.cs)

**職責**：

- HTTP 請求路由
- 中間件管道設定
- 依賴注入配置
- 應用程式啟動和生命週期管理

**技術選擇**：

- **Minimal API**：相較於傳統 Controller 模式更輕量，減少樣板程式碼
- **內建 DI 容器**：使用 ASP.NET Core 內建的依賴注入
- **Swagger/OpenAPI**：自動產生 API 文件

**關鍵設計決策**：

```csharp
// 使用 Scoped 生命週期確保每個請求都有獨立的服務實例
builder.Services.AddScoped<ITodoService, TodoService>();

// CORS 設定允許跨域請求（適合開發環境）
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder => {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// UUID 路由約束確保 ID 參數格式正確
app.MapGet("/todos/{id:guid}", async (Guid id, ITodoService todoService) => { ... });
```

### 2. 服務層 (Services)

**職責**：

- 實作業務邏輯規則
- 協調資料存取操作
- 處理業務相關的驗證和錯誤

**設計模式**：

- **介面隔離原則**：透過 `ITodoService` 介面定義合約
- **單一職責原則**：每個方法只負責一個特定功能
- **依賴反轉原則**：依賴抽象而非具體實作

**關鍵實作**：

```csharp
// UUID 生成 - 確保全域唯一性
var todo = new TodoItem
{
    Id = Guid.NewGuid(),  // 在應用層生成 UUID
    Title = request.Title.Trim(),
    // ... 其他屬性
};

// 部分更新邏輯 - 只更新有提供值的欄位
if (!string.IsNullOrWhiteSpace(request.Title))
    todo.Title = request.Title.Trim();

// 狀態變更時的自動處理
if (wasCompleted != request.IsCompleted.Value)
{
    todo.CompletedDate = request.IsCompleted.Value ? DateTime.UtcNow : null;
}
```

### 3. 資料層 (Data)

**職責**：

- Entity Framework Core 配置
- 資料庫對應關係定義
- 查詢最佳化

**設計模式**：

- **Active Record** (透過 Entity Framework)
- **Unit of Work** (DbContext)
- **Repository Pattern** (DbSet)

**關鍵配置**：

```csharp
// UUID 主鍵配置
entity.Property(e => e.Id)
    .HasColumnType("uuid")                    // PostgreSQL UUID 類型
    .HasDefaultValueSql("gen_random_uuid()"); // 資料庫層級 UUID 生成

// 明確的資料庫約束定義
entity.Property(e => e.Title)
    .IsRequired()        // 必填
    .HasMaxLength(200);  // 長度限制

// 預設值設定
entity.Property(e => e.CreatedDate)
    .HasDefaultValueSql("CURRENT_TIMESTAMP");
```

### 4. 模型層 (Models)

**職責**：

- 定義資料結構
- 實作驗證規則
- 提供資料傳輸物件

**設計模式**：

- **Data Transfer Object (DTO)**：`CreateTodoRequest`, `UpdateTodoRequest`
- **Domain Model**：`TodoItem`
- **Validation Attributes**：使用 Data Annotations

**UUID 實體設計**：

```csharp
public class TodoItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }  // UUID 主鍵
    // ... 其他屬性
}
```

## 技術選擇說明

### Entity Framework Core 9.0

**選擇原因**：

- 強型別的資料存取
- 自動化的資料庫遷移
- LINQ 查詢支援
- 良好的效能和快取機制
- 原生支援 UUID 類型

### PostgreSQL

**選擇原因**：

- 開源且功能完整
- 支援進階資料型別（包括原生 UUID）
- 優秀的併發處理
- 符合 ACID 特性
- `gen_random_uuid()` 函式支援

### Minimal API

**選擇原因**：

- 減少樣板程式碼
- 更好的效能
- 簡化的專案結構
- 適合微服務架構

### UUID 作為主鍵

**選擇原因**：

- **全域唯一性**：避免分散式系統中的 ID 衝突
- **安全性**：不容易被猜測，減少資料洩露風險
- **併發友好**：多個應用實例可同時生成唯一 ID
- **資料遷移**：便於跨資料庫的資料合併
- **API 設計**：避免序列 ID 洩露業務資訊（如總記錄數）

## 安全性考量

### 1. 輸入驗證

```csharp
// 模型層級的驗證
[Required]
[StringLength(200, MinimumLength = 1)]
public string Title { get; set; }

// 業務邏輯層級的驗證
if (string.IsNullOrWhiteSpace(request.Title))
{
    return Results.BadRequest("標題不能為空");
}

// UUID 格式驗證 (透過路由約束)
app.MapGet("/todos/{id:guid}", ...);  // 確保 ID 是有效的 GUID
```

### 2. SQL 注入防護

- 使用 Entity Framework Core 的參數化查詢
- 避免動態 SQL 字串組合
- UUID 參數型別安全

### 3. CORS 設定

```csharp
// 開發環境設定 - 生產環境應限制來源
builder.AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader();
```

### 4. UUID 安全優勢

- 不可預測性：防止順序掃描攻擊
- 資料保護：不洩露記錄數量和建立順序
- 分散式安全：避免多節點 ID 衝突

## 效能最佳化

### 1. 資料庫查詢

```csharp
// 使用非同步方法避免阻塞
await _context.Todos.ToListAsync();

// UUID 索引最佳化
entity.HasKey(e => e.Id);  // 主鍵自動建立索引
```

### 2. UUID 效能考量

**優勢**：

- PostgreSQL 原生支援，查詢效能良好
- 索引效率與整數接近
- 不需要額外的序列表

**注意事項**：

- UUID 占用 16 bytes vs int 4 bytes
- 在大量資料場景下需考慮儲存空間
- 建議使用 UUIDv4 (隨機) 或 UUIDv7 (時間排序)

### 3. 記憶體管理

- 使用 `using` 語句確保資源釋放
- DbContext 的 Scoped 生命週期管理
- UUID 字串轉換最佳化

### 4. HTTP 回應最佳化

```csharp
// 適當的 HTTP 狀態碼
return Results.Created($"/todos/{todo.Id}", todo);  // 201
return Results.NoContent();                         // 204
return Results.NotFound();                         // 404
```

## 測試策略

### 1. 單元測試

- 服務層業務邏輯測試
- 模型驗證測試
- UUID 生成和驗證測試

### 2. 整合測試

- API 端點測試
- 資料庫整合測試
- UUID 約束驗證

### 3. HTTP 測試

- 使用 `TodoListApi.http` 檔案進行手動測試
- 涵蓋所有 CRUD 操作
- UUID 格式驗證測試

## 部署考量

### 1. 環境設定

```json
// 不同環境使用不同的 appsettings
appsettings.json          // 基本設定
appsettings.Development.json  // 開發環境
appsettings.Production.json   // 生產環境
```

### 2. 資料庫遷移

```csharp
// 自動資料庫初始化
context.Database.EnsureCreated();
// 或使用完整遷移
context.Database.Migrate();
```

### 3. UUID 函式支援

確保目標 PostgreSQL 版本支援 `gen_random_uuid()` 函式：

```sql
-- PostgreSQL 13+ 預設啟用
-- 較舊版本可能需要啟用擴充
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
```

### 4. 容器化支援

專案結構支援 Docker 容器化部署，UUID 配置與環境無關。

## 擴充性設計

### 1. 新增功能

- 透過介面擴充服務層
- 新增實體時更新 DbContext
- 保持分層架構原則
- UUID 設計支援分散式擴充

### 2. 效能擴充

- 新增快取層 (Redis)
- 實作 CQRS 模式
- 資料庫讀寫分離
- UUID 分片策略

### 3. 微服務轉換

- 目前的分層架構便於拆分為微服務
- 服務介面可作為服務邊界定義
- 資料層可獨立為資料服務
- UUID 確保跨服務的唯一性

### 4. 分散式系統支援

UUID 的特性使專案天然適合分散式架構：

- **服務分割**：每個服務可獨立生成 UUID
- **資料同步**：UUID 避免跨服務 ID 衝突
- **事件溯源**：UUID 作為事件和聚合根的識別碼
- **API Gateway**：UUID 便於請求追蹤和路由
