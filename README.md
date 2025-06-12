# TodoListApi - 全端待辦事項管理系統

## 專案簡介

這是一個完整的全端待辦事項管理系統，包含：

- **後端 API**：使用 **ASP.NET Core 8.0** 和 **PostgreSQL** 建構的 RESTful API
- **前端應用**：使用 **React 18** + **TypeScript** + **Vite** 建構的現代化 SPA 應用

專案採用 **分離式架構**，後端提供 API 服務，前端提供用戶介面，支援完整的用戶認證、待辦事項管理和用戶設定功能。

## 專案架構

### 整體架構

```
專案根目錄/
├── TodoListApi/                 # 後端 API 專案
│   ├── Program.cs              # API 進入點
│   ├── Models/                 # 資料模型
│   ├── Services/               # 業務邏輯服務
│   ├── Data/                   # 資料存取層
│   └── Migrations/             # 資料庫遷移
└── todolist-frontend/          # 前端 React 專案
    ├── src/
    │   ├── components/         # React 組件
    │   ├── contexts/           # React Context
    │   ├── services/           # API 服務
    │   ├── types/              # TypeScript 類型定義
    │   └── App.tsx             # 前端應用進入點
    ├── public/                 # 靜態資源
    └── package.json            # 前端依賴管理
```

## 技術架構

### 後端技術棧

- **.NET 8.0** - 最新的 .NET 框架
- **ASP.NET Core** - Web API 框架
- **Entity Framework Core 9.0** - ORM 資料存取框架
- **PostgreSQL** - 關聯式資料庫
- **Npgsql** - PostgreSQL 的 .NET 資料提供者
- **BCrypt** - 密碼雜湊加密
- **JWT** - JSON Web Token 身份驗證
- **MailKit** - 郵件服務
- **Swagger/OpenAPI** - API 文件生成

### 前端技術棧

- **React 18** - 現代化前端框架
- **TypeScript** - 型別安全的 JavaScript
- **Vite** - 快速建構工具
- **React Router** - 前端路由管理
- **Context API** - 全域狀態管理
- **CSS3** - 現代化樣式設計
- **玻璃擬態設計** - 現代化 UI 風格
- **響應式設計** - 支援各種螢幕尺寸

### 後端專案結構

```
TodoListApi/
├── Program.cs                    # 應用程式進入點，設定服務和中間件
├── TodoListApi.csproj           # 專案檔案，定義依賴套件
├── appsettings.json             # 主要設定檔（包含資料庫連線字串）
├── appsettings.Development.json # 開發環境設定檔
├── appsettings.Production.json  # 生產環境設定檔
├── appsettings.Staging.json     # 測試環境設定檔
├── Models/                      # 資料模型層
│   ├── TodoItem.cs             # 待辦事項實體和請求/回應模型
│   └── User.cs                 # 用戶實體和認證相關模型
├── Data/                        # 資料存取層
│   └── TodoDbContext.cs        # Entity Framework 資料庫上下文
├── Services/                    # 業務邏輯層
│   ├── ITodoService.cs         # 待辦事項服務介面
│   ├── TodoService.cs          # 待辦事項服務實作
│   ├── IUserService.cs         # 用戶服務介面
│   ├── UserService.cs          # 用戶服務實作
│   ├── IEmailService.cs        # 郵件服務介面
│   └── EmailService.cs         # 郵件服務實作
├── Migrations/                  # Entity Framework 資料庫遷移檔案
├── TodoListApi.http            # API 測試檔案
└── README.md                   # 專案說明文件
```

### 前端專案結構

```
todolist-frontend/
├── src/
│   ├── components/             # React 組件
│   │   ├── AppRouter.tsx      # 路由配置
│   │   ├── LoginPage.tsx      # 登入頁面
│   │   ├── ForgotPasswordPage.tsx # 忘記密碼頁面
│   │   ├── ResetPasswordPage.tsx  # 重設密碼頁面
│   │   ├── TodoApp.tsx        # 主要待辦事項應用
│   │   ├── UserHeader.tsx     # 用戶標頭組件
│   │   ├── UserSettingsPage.tsx # 用戶設定頁面
│   │   └── *.css              # 組件樣式檔案
│   ├── contexts/              # React Context
│   │   └── AuthContext.tsx    # 認證狀態管理
│   ├── services/              # API 服務
│   │   └── authService.ts     # 認證相關 API 服務
│   ├── types/                 # TypeScript 類型定義
│   │   └── auth.ts            # 認證相關類型
│   ├── App.tsx                # 主應用組件
│   ├── App.css                # 全域樣式
│   └── main.tsx               # 應用進入點
├── public/                     # 靜態資源
├── package.json               # 依賴管理
├── tsconfig.json              # TypeScript 配置
├── vite.config.ts             # Vite 建構配置
└── README_Frontend.md         # 前端專案說明
```

### 設計模式與架構原則

1. **分層架構 (Layered Architecture)**

   - **模型層 (Models)**: 定義資料結構和驗證規則
   - **資料層 (Data)**: 處理資料庫操作和 Entity Framework 設定
   - **服務層 (Services)**: 實作業務邏輯和資料操作
   - **API 層 (Program.cs)**: 定義 HTTP 端點和請求處理

2. **依賴注入 (Dependency Injection)**

   - 使用 ASP.NET Core 內建的 DI 容器
   - 服務註冊使用 `Scoped` 生命週期
   - 透過介面實現解耦和可測試性

3. **前後端分離 (Frontend-Backend Separation)**

   - 後端專注於 API 服務和業務邏輯
   - 前端專注於用戶介面和用戶體驗
   - 透過 RESTful API 進行通訊

4. **狀態管理 (State Management)**
   - 使用 React Context API 管理全域狀態
   - 認證狀態集中管理
   - 本地存儲與伺服器狀態同步

## 功能特色

### 🔐 完整的身份驗證系統

- ✅ 用戶註冊（電子郵件 + 密碼）
- ✅ 用戶登入（JWT Token 認證）
- ✅ 忘記密碼（郵件重設連結）
- ✅ 密碼重設（安全 Token 驗證）
- ✅ 自動登入狀態維護
- ✅ 安全登出功能

### 👤 用戶資料管理

- ✅ **基本資料更新**：修改電子郵件和暱稱
- ✅ **密碼更換**：獨立的密碼更新功能
- ✅ **資料驗證**：前後端雙重驗證
- ✅ **即時更新**：修改後立即同步顯示
- ✅ **安全檢查**：密碼更換需驗證舊密碼

### 📝 待辦事項管理

- ✅ 完整的 CRUD 操作（建立、讀取、更新、刪除）
- ✅ 待辦事項狀態切換（完成/未完成）
- ✅ 用戶資料隔離（每個用戶只能存取自己的資料）
- ✅ 即時狀態更新
- ✅ 分類和篩選功能

### 🎨 現代化用戶介面

- ✅ **玻璃擬態設計**：半透明效果和模糊背景
- ✅ **響應式佈局**：完美支援桌面和行動裝置
- ✅ **平滑動畫**：載入、切換和互動動畫
- ✅ **直觀導航**：清晰的頁面結構和導航
- ✅ **即時回饋**：操作成功/失敗即時提示

### 🛡️ 安全性功能

- ✅ **密碼雜湊**：使用 BCrypt 安全加密
- ✅ **JWT 認證**：安全的 Token 驗證機制
- ✅ **HttpOnly Cookie**：防止 XSS 攻擊
- ✅ **CORS 配置**：跨域請求安全控制
- ✅ **輸入驗證**：前後端完整的資料驗證

### 📧 郵件服務

- ✅ **HTML 郵件**：美觀的郵件模板
- ✅ **密碼重設**：安全的重設連結
- ✅ **操作通知**：重要操作的郵件確認
- ✅ **多環境支援**：開發/測試/生產環境配置

## API 端點說明

### 身份驗證端點

#### 用戶註冊

```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "securePassword123",
  "displayName": "使用者名稱"
}
```

#### 用戶登入

```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "securePassword123"
}
```

#### 忘記密碼

```http
POST /auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### 重設密碼

```http
POST /auth/reset-password
Content-Type: application/json

{
  "token": "reset-token-from-email",
  "email": "user@example.com",
  "newPassword": "newSecurePassword123"
}
```

#### 更新用戶資料

```http
PUT /auth/profile
Authorization: Bearer JWT_TOKEN
Content-Type: application/json

{
  "email": "newemail@example.com",
  "displayName": "新的暱稱"
}
```

#### 更換密碼

```http
PUT /auth/change-password
Authorization: Bearer JWT_TOKEN
Content-Type: application/json

{
  "currentPassword": "currentPassword123",
  "newPassword": "newSecurePassword123"
}
```

### 待辦事項端點

#### 取得所有待辦事項

```http
GET /todos
Authorization: Bearer JWT_TOKEN
```

#### 建立新待辦事項

```http
POST /todos
Authorization: Bearer JWT_TOKEN
Content-Type: application/json

{
  "title": "待辦事項標題",
  "description": "待辦事項描述（可選）"
}
```

#### 更新待辦事項

```http
PUT /todos/{uuid}
Authorization: Bearer JWT_TOKEN
Content-Type: application/json

{
  "title": "更新的標題",
  "description": "更新的描述",
  "isCompleted": true
}
```

#### 切換完成狀態

```http
PATCH /todos/{uuid}/toggle
Authorization: Bearer JWT_TOKEN
```

#### 刪除待辦事項

```http
DELETE /todos/{uuid}
Authorization: Bearer JWT_TOKEN
```

## 資料庫設計

### Users 資料表

| 欄位名稱           | 資料型別       | 約束條件         | 說明           |
| ------------------ | -------------- | ---------------- | -------------- |
| `Id`               | `uuid`         | PRIMARY KEY      | 用戶唯一識別碼 |
| `Email`            | `varchar(255)` | UNIQUE, NOT NULL | 登入電子郵件   |
| `DisplayName`      | `varchar(100)` | NULL             | 顯示名稱       |
| `PasswordHash`     | `varchar(255)` | NOT NULL         | 密碼雜湊       |
| `CreatedDate`      | `timestamp`    | NOT NULL         | 建立時間       |
| `IsActive`         | `boolean`      | DEFAULT true     | 帳號狀態       |
| `ResetToken`       | `varchar(255)` | NULL             | 密碼重設 Token |
| `ResetTokenExpiry` | `timestamp`    | NULL             | Token 過期時間 |

### Todos 資料表

| 欄位名稱        | 資料型別        | 約束條件      | 說明        |
| --------------- | --------------- | ------------- | ----------- |
| `Id`            | `uuid`          | PRIMARY KEY   | 待辦事項 ID |
| `Title`         | `varchar(200)`  | NOT NULL      | 標題        |
| `Description`   | `varchar(1000)` | NULL          | 描述        |
| `IsCompleted`   | `boolean`       | DEFAULT false | 完成狀態    |
| `CreatedDate`   | `timestamp`     | NOT NULL      | 建立時間    |
| `CompletedDate` | `timestamp`     | NULL          | 完成時間    |
| `UserId`        | `uuid`          | FOREIGN KEY   | 所屬用戶    |

## 環境設定

### 必要條件

- .NET 8.0 SDK
- Node.js 18+ 和 npm
- PostgreSQL 12 或更新版本
- Visual Studio 2022 或 VS Code

### 後端設定

1. **資料庫設定**

   ```bash
   # 確保 PostgreSQL 服務運行
   # 建立資料庫 TodoListDb_Dev
   # 更新 appsettings.json 中的連線字串
   ```

2. **執行後端**

   ```bash
   cd TodoListApi
   dotnet restore
   dotnet run
   ```

3. **API 位址**
   - HTTPS: `https://localhost:5001`
   - HTTP: `http://localhost:5000`
   - Swagger: `https://localhost:5001/swagger`

### 前端設定

1. **安裝依賴**

   ```bash
   cd todolist-frontend
   npm install
   ```

2. **執行前端**

   ```bash
   npm run dev
   ```

3. **前端位址**
   - 開發伺服器: `http://localhost:5173`

### 郵件服務設定

在 `appsettings.json` 中配置 SMTP 設定：

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "TodoList App"
  }
}
```

## 使用說明

### 1. 用戶註冊和登入

1. 開啟前端應用 `http://localhost:5173`
2. 點擊「註冊」建立新帳號
3. 使用電子郵件和密碼登入

### 2. 管理待辦事項

1. 登入後進入主頁面
2. 點擊「新增待辦事項」建立項目
3. 使用勾選框標記完成狀態
4. 點擊編輯按鈕修改內容
5. 使用刪除按鈕移除項目

### 3. 用戶設定

1. 點擊右上角的「⚙️ 設定」按鈕
2. 在「基本資料」分頁更新電子郵件和暱稱
3. 在「更換密碼」分頁修改密碼
4. 點擊「返回」回到主頁面

### 4. 忘記密碼

1. 在登入頁面點擊「忘記密碼？」
2. 輸入註冊的電子郵件地址
3. 檢查郵件中的重設連結
4. 點擊連結設定新密碼

## 測試 API

### 使用 Swagger UI

1. 執行後端專案
2. 開啟 `https://localhost:5001/swagger`
3. 測試各個 API 端點

### 使用 HTTP 檔案

1. 開啟 `TodoListApi.http` 檔案
2. 使用 VS Code REST Client 擴充功能
3. 點擊 "Send Request" 測試端點

## 部署建議

### 後端部署

- 使用 Docker 容器化部署
- 配置生產環境資料庫
- 設定 HTTPS 憑證
- 配置環境變數

### 前端部署

- 建構生產版本：`npm run build`
- 部署到 CDN 或靜態網站託管
- 配置正確的 API 端點

## 後續擴充建議

### 功能擴充

- 🏷️ 待辦事項標籤和分類
- 📅 到期日期和提醒功能
- 👥 團隊協作和分享
- 📊 統計和報表功能
- 🔄 資料同步和備份
- 📱 PWA 支援（離線功能）

### 技術改進

- 🧪 單元測試和整合測試
- 🚀 效能優化和快取
- 📈 監控和日誌記錄
- 🔒 進階安全性功能
- 🌐 國際化支援
- ♿ 無障礙功能改善

## 授權

此專案僅供學習和開發參考使用。
