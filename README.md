# RentalManagementPlatform

## 專案簡介

RentalManagementPlatform 是一個仿 Airbnb 風格的租賃管理平台，由 iSpan FUCN43班 第一組開發。本專案採用現代化的全端技術棧，提供完整的房源管理、預訂系統和用戶管理功能。

## 🚀 技術棧

### 後端技術
- **框架**: ASP.NET Core 8 (C# 12)
- **ORM**: Entity Framework Core 8
- **資料庫**: SQL Server 2022
- **架構**: 分層架構 (Controller → Service → Repository)
- **搜尋引擎**: Meilisearch
- **快取系統**: Redis Streams
- **檔案儲存**: MinIO

### 前端技術
- **框架**: Vue 3 + TypeScript
- **建置工具**: Vite
- **狀態管理**: Pinia + TanStack Query
- **路由**: Vue Router 4
- **UI 框架**: Bootstrap 5
- **表單驗證**: Vee-Validate
- **圖表**: Chart.js
- **地圖**: Leaflet
- **圖片輪播**: Swiper
- **日期選擇器**: Vue Datepicker
- **圖示**: Font Awesome
- **檔案上傳**: Uppy

### 開發與部署
- **容器化**: Docker + Docker Compose
- **反向代理**: Nginx
- **開發工具**: Vite DevTools

## 📁 專案結構

```
RentalManagementPlatform/
├── RentalManagementPlatformMVC/          # 主要後端專案
│   ├── RentalManagementPlatformMVC/      # MVC 管理後台
│   ├── RentalManagementPlatformWebAPI/   # Web API
│   ├── RentalManagementPlatform.Common/  # 共用類別庫
│   └── ChaseCheng.Global.Utilities/      # 工具類別庫
├── RentalManagementPlatformVue/          # Vue 3 前端專案
├── Document/                             # 專案文件
├── nginx/                                # Nginx 配置
├── docker-compose.yaml                   # Docker 編排配置
└── nginx.conf                           # Nginx 主配置
```

## 🛠️ 安裝與執行

### 前置需求
- .NET 8 SDK
- Node.js 20.19.0+ 或 22.12.0+
- SQL Server 2022
- Docker & Docker Compose (可選)

### 快速開始

1. **克隆專案**
   ```bash
   git clone https://github.com/iSpanChase/RentalManagementPlatform.git
   cd RentalManagementPlatform
   ```

2. **後端設定**
   ```bash
   cd RentalManagementPlatformMVC/RentalManagementPlatformMVC
   dotnet restore
   dotnet run
   ```

3. **前端設定**
   ```bash
   cd RentalManagementPlatformMVC/RentalManagementPlatformVue
   npm install
   npm run dev
   ```

4. **使用 Docker 執行**
   ```bash
   docker-compose up -d
   ```

### 環境變數設定

複製 `.env` 檔案並設定必要的環境變數：

```bash
# 資料庫連接字串
ConnectionStrings__DefaultConnection=Server=localhost;Database=RentalManagementPlatform;Trusted_Connection=true;TrustServerCertificate=true;

# Redis 設定
Redis__ConnectionString=localhost:6379

# Meilisearch 設定
Meilisearch__Host=http://localhost:7700
Meilisearch__ApiKey=your-api-key

# MinIO 設定
MinIO__Endpoint=localhost:9000
MinIO__AccessKey=your-access-key
MinIO__SecretKey=your-secret-key
```

## 📋 功能特色

### 房源管理
- 📍 房源列表與搜尋
- 🏠 房源詳細資訊
- 📸 多圖上傳與管理
- 🗺️ 地圖定位
- ⭐ 評價與評分系統

### 預訂系統
- 📅 日期選擇與可用性檢查
- 💰 價格計算
- 🔒 安全預訂流程
- 📧 預訂確認通知

### 用戶管理
- 👤 用戶註冊與登入
- 🔐 身份驗證與授權
- 💳 支付資訊管理
- 📝 個人資料編輯

### 管理後台
- 📊 數據統計與分析
- 👥 用戶管理
- 🏢 房源審核
- 💬 評價管理
- ⚙️ 系統設定

## 🧪 開發指南

### 程式碼規範
- 使用分層架構模式
- 所有資料庫操作使用非同步方法
- 遵循 RESTful API 設計原則
- 使用 Repository 模式進行資料存取

### 測試
```bash
# 執行單元測試
dotnet test

# 執行前端測試
npm run test
```

### 建置與部署
```bash
# 建置後端
dotnet publish -c Release

# 建置前端
npm run build

# 使用 Docker 部署
docker-compose -f docker-compose.prod.yml up -d
```

## 🤝 貢獻指南

我們歡迎任何形式的貢獻！請遵循以下步驟：

1. Fork 本專案
2. 建立功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交變更 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 開啟 Pull Request

## 📄 授權

本專案採用 MIT 授權 - 詳見 [LICENSE](LICENSE) 檔案

## 👥 開發團隊

- **iSpan FUCN43班 第一組**
- 指導老師: [老師姓名]
- 專案負責人: [負責人姓名]

## 📞 聯絡我們

如有任何問題或建議，請透過以下方式聯絡我們：
- 專案 Issues: [GitHub Issues](https://github.com/iSpanChase/RentalManagementPlatform/issues)
- 電子郵件: [聯絡郵箱]

---

⭐ 如果這個專案對您有幫助，請給我們一個 Star！