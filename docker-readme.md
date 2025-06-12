# Docker 部署指南

本文件說明如何使用 Docker 和 Docker Compose 來部署 TodoList API 應用程式。

## 📋 前置需求

- Docker Desktop 或 Docker Engine
- Docker Compose V2

### 安裝 Docker（Windows）

1. 下載並安裝 [Docker Desktop for Windows](https://desktop.docker.com/win/stable/Docker%20Desktop%20Installer.exe)
2. 啟動 Docker Desktop
3. 確認安裝：
   ```bash
   docker --version
   docker-compose --version
   ```

## 🚀 快速開始

### 生產環境部署

```bash
# 1. 建構並啟動所有服務
docker-compose up -d

# 2. 查看服務狀態
docker-compose ps

# 3. 查看日誌
docker-compose logs -f api
```

### 開發環境設定

```bash
# 1. 啟動開發用資料庫
docker-compose -f docker-compose.dev.yml up -d

# 2. 在本地運行 API
dotnet run

# 3. 訪問 pgAdmin（資料庫管理工具）
# http://localhost:8080
# 用戶名: admin@todolist.com
# 密碼: admin123
```

## 🔧 詳細設定

### 環境變數

#### 生產環境

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=Host=database;Database=todolist;Username=postgres;Password=TodoList@2024`

#### 開發環境

- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__DefaultConnection=Host=localhost;Port=5433;Database=todolist_dev;Username=postgres;Password=dev123`

### 端口配置

| 服務       | 生產環境端口 | 開發環境端口 | 說明           |
| ---------- | ------------ | ------------ | -------------- |
| API        | 5000         | 5001/5000    | TodoList API   |
| PostgreSQL | 5432         | 5433         | 資料庫         |
| pgAdmin    | -            | 8080         | 資料庫管理工具 |

### 資料持久化

- **生產環境**: `todolist-postgres-data` volume
- **開發環境**: `todolist-postgres-dev-data` volume

## 📝 常用命令

### Docker Compose 命令

```bash
# 建構映像檔
docker-compose build

# 啟動服務（背景模式）
docker-compose up -d

# 停止服務
docker-compose down

# 停止服務並移除 volumes
docker-compose down -v

# 查看服務狀態
docker-compose ps

# 查看即時日誌
docker-compose logs -f [service_name]

# 進入容器
docker-compose exec api bash
docker-compose exec database psql -U postgres -d todolist
```

### Docker 映像檔管理

```bash
# 列出本地映像檔
docker images

# 移除未使用的映像檔
docker image prune

# 建構 API 映像檔
docker build -t todolist-api .

# 運行單個容器
docker run -p 5000:80 todolist-api
```

## 🔍 疑難排解

### 常見問題

1. **端口已被佔用**

   ```bash
   # 檢查端口使用情況
   netstat -ano | findstr :5000

   # 修改 docker-compose.yml 中的端口映射
   ports:
     - "5001:80"  # 改為其他端口
   ```

2. **資料庫連線失敗**

   ```bash
   # 檢查資料庫容器狀態
   docker-compose logs database

   # 手動測試資料庫連線
   docker-compose exec database psql -U postgres -d todolist
   ```

3. **API 容器無法啟動**

   ```bash
   # 查看詳細錯誤日誌
   docker-compose logs api

   # 重新建構映像檔
   docker-compose build --no-cache api
   ```

### 日誌檢查

```bash
# 查看所有服務日誌
docker-compose logs

# 查看特定服務日誌
docker-compose logs api
docker-compose logs database

# 即時監控日誌
docker-compose logs -f --tail=100 api
```

### 健康檢查

```bash
# 檢查 API 健康狀態
curl http://localhost:5000/health

# 檢查資料庫健康狀態
docker-compose exec database pg_isready -U postgres
```

## 🧹 清理資源

```bash
# 停止並移除所有容器、網路
docker-compose down

# 移除所有相關資源（包含 volumes）
docker-compose down -v

# 清理 Docker 系統
docker system prune -a
```

## 🔒 安全性考量

1. **密碼管理**: 使用環境變數或 Docker secrets
2. **網路隔離**: 使用自定義網路
3. **最小權限**: 容器以非 root 用戶運行
4. **映像檔安全**: 定期更新基礎映像檔

## 📚 進階配置

### 多環境部署

```bash
# 開發環境
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up

# 測試環境
docker-compose -f docker-compose.yml -f docker-compose.test.yml up

# 生產環境
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up
```

### 擴展服務

```bash
# 擴展 API 服務實例
docker-compose up -d --scale api=3
```

### 監控和日誌

考慮整合：

- Prometheus + Grafana（監控）
- ELK Stack（日誌管理）
- Jaeger（分散式追蹤）
