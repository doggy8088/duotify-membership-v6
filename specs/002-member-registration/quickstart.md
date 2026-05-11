# Quickstart: 會員註冊流程

## Prerequisites

- .NET SDK 8
- SQL Server 2022 或相容版本
- Node.js 20+（用於 Tailwind CSS v4 CLI）
- 一個可用的 SMTP 測試或正式寄信環境

## 1. 建立專案骨架

1. 建立 ASP.NET Core MVC 專案作為主網站。
2. 於 `src/Duotify.Membership.Web` 下建立 `Application`、`Infrastructure`、`ViewModels`、`Views/Registration` 與 `wwwroot` 子結構。
3. 僅加入必要 NuGet 套件：SQL Server 存取與測試所需套件，其餘優先使用 .NET 8 內建功能。

## 2. 設定基礎組態

1. 設定 SQL Server 連線字串。
2. 設定 SMTP 寄信服務參數。
3. 設定驗證碼 keyed hash/HMAC 所需 secret 與 key version。
4. 啟用 ASP.NET Core Anti-forgery 與 Rate Limiting。

## 3. 建立資料庫結構

1. 建立 `Members`、`EmailVerificationChallenges`、`SecurityAuditLogs`。
2. 為 `Members.NationalId` 與 `Members.Email` 建立唯一索引。
3. 建立限制每位會員同時只有一筆 active challenge 的索引策略。
4. 為驗證與重寄流程建立必要查詢索引。

## 4. 實作應用服務

1. `RegisterMemberService`: 驗證資料、建立會員、建立首筆 challenge、寫入審計事件。
2. `VerifyEmailCodeService`: 驗證最新 challenge、累積錯誤次數、鎖定或啟用會員。
3. `ResendEmailVerificationService`: 失效舊 challenge、建立新 challenge、解除鎖定。
4. `RestrictedAccessPolicy`: 控制未驗證會員登入後只可查看基本資料。

## 5. 實作 MVC 與前端

1. 建立 `RegistrationController` 與對應 Razor Views。
2. 使用 Tailwind CSS v4 CLI 編譯樣式到 `wwwroot/css/app.css`。
3. 使用 Vanilla JS 建立驗證碼六格輸入、倒數顯示、重寄按鈕與錯誤聚焦。
4. 視覺風格依 DESIGN.md 的 transactional track：`canvas-light` / `canvas-cream`、黑色主 CTA pill、`aloe`/`pistachio` 點綴、amber 警示、red 錯誤，不使用紫色主題。

## 6. 驗證功能

1. 執行單元測試：密碼規則、驗證碼狀態轉移、鎖定與重寄規則。
2. 執行 SQL Server 整合測試：唯一索引、單一 active challenge、交易一致性。
3. 執行 Web integration tests：註冊頁、驗證頁、Anti-forgery、轉導與錯誤提示。
4. 手動檢查 UI 狀態：載入、欄位錯誤、重複註冊、逾期碼、鎖定、重寄、驗證成功、未驗證受限提示。

## 7. 驗收清單

- 註冊成功後不會自動登入。
- 身分證字號與 E-Mail 均不可重複。
- 驗證碼只有最新一組有效，且 5 分鐘過期。
- 同一帳號驗證碼錯誤 3 次後鎖定，直到重新寄送新碼。
- 未驗證會員登入後僅可查看基本資料。
- 所有關鍵動作都有安全審計紀錄。