# GitHub Actions CI/CD 說明

本目錄包含 TodoList API 專案的 GitHub Actions 工作流程設定。

## 📁 工作流程概覽

### 🔄 ci-cd.yml - 主要 CI/CD 管線

**觸發時機**：推送到 main/develop 分支或 feature 分支
**功能**：

- 程式碼品質檢查
- 單元測試執行
- 安全性掃描
- Docker 映像檔建構
- 自動部署到開發/生產環境

### 🔍 pr-check.yml - Pull Request 檢查

**觸發時機**：建立或更新 Pull Request
**功能**：

- PR 標題和描述格式驗證
- 程式碼格式檢查
- 建構和測試驗證
- 變更檔案分析
- 自動生成檢查報告

### 🚀 release.yml - 自動化發佈

**觸發時機**：推送版本標籤 (v*.*.\*)
**功能**：

- 版本標籤驗證
- 發佈版本建構
- Docker 映像檔發佈
- 自動生成變更日誌
- 建立 GitHub Release
- 生產環境部署

## 🎯 使用指南

### 開發流程

1. **功能開發**

   ```bash
   # 從 develop 分支建立功能分支
   git checkout develop
   git pull origin develop
   git checkout -b feature/your-feature-name

   # 開發完成後推送
   git add .
   git commit -m "feat: add your feature description"
   git push origin feature/your-feature-name
   ```

2. **建立 Pull Request**

   - 標題格式：`feat(scope): description`
   - 提供詳細的變更描述
   - 等待自動檢查完成
   - 請求程式碼審查

3. **合併到開發分支**

   ```bash
   # PR 通過後合併到 develop
   # 將觸發開發環境部署
   ```

4. **發佈到生產環境**

   ```bash
   # 從 develop 合併到 main
   git checkout main
   git pull origin main
   git merge develop
   git push origin main

   # 建立版本標籤
   git tag v1.0.0
   git push origin v1.0.0
   ```

### Commit 訊息格式

使用 [Conventional Commits](https://www.conventionalcommits.org/) 格式：

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**類型 (type)**：

- `feat`: 新功能
- `fix`: 錯誤修復
- `docs`: 文件更新
- `style`: 程式碼格式調整
- `refactor`: 程式碼重構
- `perf`: 效能優化
- `test`: 測試相關
- `chore`: 建構流程或輔助工具變更
- `ci`: CI/CD 配置變更

**範例**：

```bash
git commit -m "feat(auth): add JWT token refresh functionality"
git commit -m "fix(api): resolve database connection timeout issue"
git commit -m "docs: update API documentation for user endpoints"
```

### 版本號規則

使用 [Semantic Versioning](https://semver.org/) 格式：

- **MAJOR.MINOR.PATCH** (例如：1.0.0)
- **MAJOR.MINOR.PATCH-PRERELEASE** (例如：1.0.0-beta)

**版本類型**：

- `MAJOR`: 不相容的 API 變更
- `MINOR`: 向後相容的新功能
- `PATCH`: 向後相容的錯誤修復
- `PRERELEASE`: 預發布版本 (alpha, beta, rc)

## 🔧 環境設定

### 必要的 Secrets

在 GitHub 倉庫設定中加入以下 secrets：

```yaml
# 容器註冊表
DOCKER_USERNAME: your-docker-username
DOCKER_PASSWORD: your-docker-password

# 部署相關
DEPLOY_SSH_KEY: your-deployment-ssh-key
DEPLOY_HOST: your-deployment-host
DEPLOY_USER: your-deployment-user

# 通知相關 (可選)
SLACK_WEBHOOK: your-slack-webhook-url
DISCORD_WEBHOOK: your-discord-webhook-url
```

### 環境變數

```yaml
# .github/workflows 中使用的環境變數
DOTNET_VERSION: "8.0.x"
NODE_VERSION: "18"
DOCKER_REGISTRY: "ghcr.io"
IMAGE_NAME: "todolist-api"
```

## 📊 工作流程狀態

### 分支保護規則建議

為 `main` 和 `develop` 分支設定保護規則：

```yaml
# 分支保護設定
required_status_checks:
  - "程式碼品質檢查"
  - "單元測試"
  - "安全性掃描"

require_pull_request_reviews: true
dismiss_stale_reviews: true
require_code_owner_reviews: false
required_approving_review_count: 1

enforce_admins: false
allow_force_pushes: false
allow_deletions: false
```

### 環境設定

建立以下環境並設定適當的保護規則：

- **development**: 自動部署，無需審批
- **production**: 需要手動審批才能部署

## 🔍 監控和日誌

### 工作流程監控

- 在 GitHub Actions 頁面查看執行狀態
- 每個工作流程都有詳細的執行日誌
- 失敗時會收到電子郵件通知

### 部署監控

- 檢查部署後的應用程式健康狀態
- 監控 Docker 容器運行狀況
- 追蹤應用程式效能指標

## 🛠️ 疑難排解

### 常見問題

1. **建構失敗**

   - 檢查程式碼格式是否正確
   - 確認所有測試都通過
   - 驗證依賴套件版本

2. **部署失敗**

   - 檢查環境變數設定
   - 驗證 secrets 配置
   - 確認目標環境可用性

3. **Docker 建構失敗**
   - 檢查 Dockerfile 語法
   - 確認基礎映像檔可用
   - 驗證建構上下文

### 偵錯技巧

```bash
# 本地測試 Docker 建構
docker build -t todolist-api .

# 本地執行測試
dotnet test --configuration Release

# 檢查程式碼格式
dotnet format --verify-no-changes
```

## 📚 延伸學習

- [GitHub Actions 官方文件](https://docs.github.com/en/actions)
- [Docker 最佳實踐](https://docs.docker.com/develop/dev-best-practices/)
- [.NET 應用程式部署](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
