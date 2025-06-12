# TodoList - 全端待辦事項管理系統

## 🚀 線上演示

**🌐 線上應用程式**: [TodoList API 在 Azure](https://todolistapi-d3gzb9dkcvfshncn.eastasia-01.azurewebsites.net/)  
**📊 專案狀態**: ✅ 已部署到 Azure App Service  
**💾 資料庫**: ✅ Supabase PostgreSQL  
**🔄 CI/CD**: ✅ GitHub Actions 自動化部署

> 💡 本專案已成功部署到生產環境，包含完整的 DevOps 流程實作！

---

## 📖 專案簡介

這是一個現代化的全端待辦事項管理系統，展示了完整的軟體開發流程，從程式設計到雲端部署。

### ✨ 主要特色

- 🔐 **完整用戶認證系統** - 註冊、登入、密碼重設
- 📝 **待辦事項管理** - CRUD 操作、狀態切換
- 👤 **用戶資料管理** - 個人資料更新、密碼變更
- 🔒 **安全性** - JWT 認證、密碼雜湊、資料隔離
- 📧 **郵件服務** - 密碼重設郵件通知

## 🛠️ 技術架構

### 後端技術棧

- **ASP.NET Core 8.0** - Web API 框架
- **Entity Framework Core** - ORM 資料存取
- **PostgreSQL** - 關聯式資料庫
- **JWT** - 身份驗證
- **BCrypt** - 密碼加密
- **MailKit** - 郵件服務

### DevOps 工具鏈

- **Docker** - 容器化部署
- **GitHub Actions** - CI/CD 自動化
- **Azure App Service** - 雲端託管
- **Supabase** - 雲端資料庫

## 🏗️ 專案結構

```
TodoListApi/
├── Models/              # 資料模型
├── Services/            # 業務邏輯服務
├── Data/                # 資料存取層
├── Migrations/          # 資料庫遷移
├── .github/workflows/   # GitHub Actions CI/CD
├── Dockerfile           # Docker 容器配置
├── docker-compose.yml   # 服務編排
└── README.md           # 專案說明
```

## 🚀 快速開始

### 前置需求

- .NET 8.0 SDK
- PostgreSQL 或 Docker
- Git

### 本地開發設定

1. **複製專案**

   ```bash
   git clone https://github.com/maplestartend/todolist-api.git
   cd todolist-api
   ```

2. **啟動資料庫（使用 Docker）**

   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

3. **設定環境變數**

   ```bash
   # 更新 appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5433;Database=todolist_dev;Username=postgres;Password=dev123"
     }
   }
   ```

4. **執行應用程式**

   ```bash
   dotnet restore
   dotnet run
   ```

5. **測試 API**
   - API 文件: `https://localhost:5001/swagger`
   - 健康檢查: `https://localhost:5001/health`

### Docker 部署

```bash
# 建構並啟動所有服務
docker-compose up -d

# 查看服務狀態
docker-compose ps

# 查看日誌
docker-compose logs -f api
```

## 📋 API 端點

### 認證

- `POST /auth/register` - 用戶註冊
- `POST /auth/login` - 用戶登入
- `POST /auth/forgot-password` - 忘記密碼
- `POST /auth/reset-password` - 重設密碼
- `PUT /auth/profile` - 更新個人資料
- `PUT /auth/change-password` - 變更密碼

### 待辦事項

- `GET /todos` - 取得所有待辦事項
- `POST /todos` - 建立新待辦事項
- `PUT /todos/{id}` - 更新待辦事項
- `PATCH /todos/{id}/toggle` - 切換完成狀態
- `DELETE /todos/{id}` - 刪除待辦事項

_詳細的 API 文件請參考 [Swagger UI](https://todolistapi-d3gzb9dkcvfshncn.eastasia-01.azurewebsites.net/swagger)_

## 🔧 部署架構

### 生產環境

- **雲端平台**: Azure App Service
- **資料庫**: Supabase PostgreSQL
- **部署方式**: Visual Studio 發布 + GitHub Actions
- **HTTPS**: 已啟用 SSL 憑證

### 環境分離

- **開發環境**: 本地 Docker + PostgreSQL
- **生產環境**: Azure App Service + Supabase

## 🤝 開發流程

### Git 工作流程

```bash
# 功能開發
git checkout -b feature/new-feature
git commit -m "feat: add new feature"
git push origin feature/new-feature

# 建立 Pull Request
# 合併後自動觸發 CI/CD
```

### CI/CD 自動化

- ✅ 程式碼品質檢查
- ✅ 自動化測試
- ✅ Docker 映像檔建構
- ✅ 自動部署到 Azure

## 📚 學習重點

這個專案展示了以下技能：

1. **後端開發**: ASP.NET Core Web API 開發
2. **資料庫設計**: PostgreSQL 資料庫設計與 Entity Framework
3. **認證授權**: JWT Token 實作
4. **容器化**: Docker 和 Docker Compose
5. **CI/CD**: GitHub Actions 自動化部署
6. **雲端部署**: Azure 和 Supabase 整合
7. **版本控制**: Git 和 GitHub 協作流程

## 🛡️ 安全性

- JWT Token 認證機制
- 密碼 BCrypt 雜湊加密
- 用戶資料隔離
- HTTPS 強制加密傳輸
- 環境變數安全管理

## 📄 授權

此專案僅供學習和開發參考使用。

---

⭐ 如果這個專案對您有幫助，請考慮給個星星！

📧 有問題或建議？歡迎開 [Issue](https://github.com/maplestartend/todolist-api/issues)
