-- PostgreSQL 資料庫設定腳本
-- 請在 PostgreSQL 中執行這些指令

-- 建立開發環境資料庫
CREATE DATABASE "TodoListDb_Dev";

-- 建立生產環境資料庫
CREATE DATABASE "TodoListDb";

-- 建立專用使用者 (選擇性)
CREATE USER todoapp WITH PASSWORD 'SecurePassword123!';

-- 授予權限
GRANT ALL PRIVILEGES ON DATABASE "TodoListDb_Dev" TO todoapp;
GRANT ALL PRIVILEGES ON DATABASE "TodoListDb" TO todoapp;

-- 連線到開發資料庫並授予 schema 權限
\c "TodoListDb_Dev"
GRANT ALL ON SCHEMA public TO todoapp;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO todoapp;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO todoapp;

-- 連線到生產資料庫並授予 schema 權限
\c "TodoListDb"
GRANT ALL ON SCHEMA public TO todoapp;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO todoapp;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO todoapp;

-- 顯示建立的資料庫
\l 