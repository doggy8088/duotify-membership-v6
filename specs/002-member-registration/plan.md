# Implementation Plan: 會員註冊流程

**Branch**: `[002-member-registration]` | **Date**: 2026-05-11 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-member-registration/spec.md`

## Summary

以 .NET 10 的 ASP.NET Core MVC 建立單向會員註冊與 E-Mail 驗證流程，採用薄 Controller、Application Service 集中流程規則、SQL Server 作為唯一真實來源，並以最少的 NuGet 套件完成高安全標準。此功能會實作身分證字號與 E-Mail 唯一性、6 位數驗證碼挑戰、重新寄送即失效舊碼、錯誤 3 次鎖定到重寄為止、未驗證帳號僅可查看基本資料，以及使用 Tailwind CSS v4 + Vanilla JS 建立符合 transactional track 的 MVC 介面：light/cream canvas、pill button、非紫色、以 aloe/pistachio 作為限定點綴。

## Technical Context

**Language/Version**: C# 14 / .NET 10
**Primary Dependencies**: ASP.NET Core MVC shared framework、Microsoft.Data.SqlClient、內建 Antiforgery、內建 Rate Limiting、Tailwind CSS v4 CLI、Vanilla JS、開源字型 Inter Variable/Inter Display（取代任何商業字型套件）
**Storage**: SQL Server 2022 或相容版本，無快取層
**Testing**: xUnit、ASP.NET Core integration tests、SQL Server-backed integration suite、少量端對端流程驗證
**UX Surface**: 註冊頁、驗證碼輸入頁、重新寄送流程、未驗證帳號受限提示、共用表單錯誤與成功狀態
**Target Platform**: ASP.NET Core Web App on Linux/Windows server + desktop/mobile browsers
**Project Type**: Server-rendered web application (MVC, controller-based)
**Performance Goals**: 註冊送出與驗證請求在應用程式層 p95 < 500ms（不含外部寄信供應商延遲）；95% 驗證碼寄送結果於 30 秒內回饋；主要表單畫面在一般 4G/桌機環境可於 1.5 秒內完成首屏可互動
**Constraints**: 不使用 cache、不得使用商業套件、NuGet 依賴維持最少、不可自動登入、註冊流程必須使用 DESIGN.md 定義的 transactional track、不得使用紫色作為主要視覺色、所有按鈕維持 pill shape、需符合高安全標準
**Scale/Scope**: 單一 MVC 網站中的註冊/驗證子流程；2 個主要頁面、3 個主要 POST 動作、4 個核心資料實體、支援每日數千筆註冊量級

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Code Quality**: 變更面集中在註冊/驗證專屬 Controller、ViewModel、Application Service、SQL 存取層與 Razor Views。流程規則統一放在 service 層，避免 Controller 與 View 散落狀態判斷；資料存取採最小可行抽象，不引入完整會員框架或額外前端框架。
- **Testing**: 必要自動化測試包含單元測試（密碼規則、驗證碼狀態轉移、鎖定/重寄規則）、整合測試（SQL Server unique index、active challenge 單一性、交易一致性）、Web integration tests（MVC 表單與 Anti-forgery 行為）。合併前需提供 `dotnet test` 綠燈證據與至少一組 SQL Server-backed 測試結果。
- **UX Consistency**: 註冊/驗證頁走 DESIGN.md 的 transactional track：`canvas-light` / `canvas-cream` 底、黑色主 CTA pill、必要時以 `aloe`/`pistachio` 作為 light track 點綴，禁止紫色；沿用 MVC 表單、驗證摘要、欄位錯誤提示、成功/失敗 alert 樣式；明確覆蓋 loading、欄位錯誤、重複註冊、驗證碼錯誤、驗證碼過期、驗證鎖定、重寄成功、未驗證受限提示等狀態；保持鍵盤操作、焦點順序、語意標記與手機版可讀性。
- **Performance**: 以伺服器端 request timing、SQL query profiling、寄信流程監控驗證效能預算；若寄信流程造成 30 秒以上回饋延遲，採用明確失敗回饋與後續重送指引作為緩解；若 SQL 查詢超出 p95 預算，優先以索引與查詢調整修正，而非引入 cache。
- **Exceptions**: 無憲章例外。Phase 1 設計後再次檢查仍為 PASS。

## Project Structure

### Documentation (this feature)

```text
specs/002-member-registration/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── member-registration.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
└── Duotify.Membership.Web/
    ├── Application/
    │   ├── Interfaces/
    │   └── Services/
    ├── Controllers/
    ├── Domain/
    │   ├── Entities/
    │   └── ValueObjects/
    ├── Infrastructure/
    │   ├── Data/
    │   ├── Email/
    │   ├── Repositories/
    │   └── Security/
    ├── ViewModels/
    ├── Views/
    │   └── Registration/
    ├── wwwroot/
    │   ├── css/
    │   └── js/
    └── Program.cs

tests/
├── Duotify.Membership.Web.UnitTests/
├── Duotify.Membership.Web.IntegrationTests/
└── Duotify.Membership.Web.WebTests/
```

**Structure Decision**: 採用單一 MVC Web 專案搭配分層資料夾與獨立測試專案。這是符合現況空 repo、最少套件、Controller-based MVC 與高安全需求的最低複雜度結構；不拆前後端，也不引入獨立 API 專案。

## Phase 0 Research Summary

- 採用薄 Controller + Application Service + SQL 存取層，不導入 ASP.NET Core Identity UI。
- SQL Server 以 Members、EmailVerificationChallenges、SecurityAuditLogs 為核心；用 unique index 與 filtered unique index 保證唯一性與單一 active challenge。
- 密碼使用內建 PasswordHasher；驗證碼以 CSPRNG 產生，資料庫只存 keyed hash/HMAC 結果，不存明碼。
- 重新寄送在同一交易中失效舊 challenge 並建立新 challenge，鎖定也僅能透過重寄解除。
- Tailwind CSS v4 以 CLI 編譯靜態資產，Vanilla JS 只處理驗證碼輸入、倒數與送出輔助。
- 測試策略以單元測試 + SQL Server 整合測試 + 少量 Web integration tests 為主。

## Phase 1 Design Output

- Data model: [data-model.md](./data-model.md)
- Contracts: [contracts/member-registration.md](./contracts/member-registration.md)
- Quickstart: [quickstart.md](./quickstart.md)
- Research: [research.md](./research.md)

## Post-Design Constitution Check

- **Code Quality**: PASS。分層與變更邊界已固定，不需額外框架或商業套件。
- **Testing**: PASS。已定義單元、整合與 Web 測試層，並明確要求 SQL Server-backed 驗證。
- **UX Consistency**: PASS。已涵蓋所有必要狀態與可及性要求，且維持 MVC/Razor 互動模式。
- **Performance**: PASS。已定義 request latency、寄信回饋與 SQL 驗證方式，且不依賴 cache。
- **Exceptions**: 無。

## Complexity Tracking

無。
