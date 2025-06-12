# TodoListApi - 全端待辦事項管理系統

## 🚀 線上演示

**🌐 線上應用程式**: [TodoList API 在 Azure](https://todolistapi-d3gzb9dkcvfshncn.eastasia-01.azurewebsites.net/)
**📊 專案狀態**: ✅ 已部署到 Azure App Service  
**💾 資料庫**: ✅ Supabase PostgreSQL  
**🔄 CI/CD**: ✅ GitHub Actions 自動化部署

> 💡 本專案已成功部署到生產環境，包含完整的 DevOps 流程實作！

---

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
