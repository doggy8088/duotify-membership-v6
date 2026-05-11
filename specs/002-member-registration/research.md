# Phase 0 Research: 會員註冊流程

## 1. MVC 分層與最小依賴

**Decision**: 採用薄 Controller + Application Service + SQL 存取層的三層結構；Controller 僅處理 HTTP、ModelState、Anti-forgery 與轉導，註冊、驗證、重寄與受限規則集中在 Application Service。

**Rationale**: 這個功能的核心是狀態轉移與交易一致性，不是畫面渲染。將流程規則集中在 service 層，可以讓註冊、驗證、重寄與未驗證限制共用同一套規則，且不必為自訂驗證流程硬套 ASP.NET Core Identity UI。

**Alternatives considered**:
- 使用完整 ASP.NET Core Identity UI：行為與資料表結構過重，且難以自然表達「驗證碼 3 次錯誤後鎖定到重寄為止」與「未驗證仍可登入但功能受限」。
- 將商業邏輯寫進 Controller：難以測試且容易在不同 action 間產生規則分歧。

## 2. SQL Server 資料模型與交易邊界

**Decision**: 以 `Members`、`EmailVerificationChallenges`、`SecurityAuditLogs` 為核心資料實體；由 SQL Server unique index 保證身分證字號與 E-Mail 唯一，並以 filtered unique index 保證每位會員同一時間只有一筆 active challenge。

**Rationale**: 驗證挑戰獨立成表後，才能完整追蹤重寄、失效、驗證成功、鎖定與過期等歷史。註冊會在同一交易內建立 member 與首筆 challenge；重寄會在同一交易內失效舊 challenge 並建立新 challenge；驗證則在同一交易內更新失敗次數、鎖定或驗證成功狀態。

**Alternatives considered**:
- 只用一張會員表承載所有驗證欄位：難以表達重寄、失效與稽核歷史。
- 只靠應用程式先查重再插入：併發下無法保證唯一性，必須由資料庫作最終防線。

## 3. 高安全標準

**Decision**: 密碼使用 ASP.NET Core 內建 `PasswordHasher`；驗證碼用密碼學安全亂數產生 6 位數，資料庫只存 keyed hash/HMAC，不存明碼；重寄必定建立新 challenge 並使舊 challenge 失效；驗證與重寄端點都加上 rate limiting 與審計記錄。

**Rationale**: 內建 `PasswordHasher` 已是官方建議做法。對於只有 100 萬種組合的 6 位數驗證碼，若只存一般雜湊，資料庫外洩時容易被離線枚舉；採用 keyed hash/HMAC 能降低此風險。審計資料可支援安全檢查與日後調查，但不得紀錄明碼密碼或明碼驗證碼。

**Alternatives considered**:
- 自行設計 PBKDF2 密碼儲存：增加安全風險與維護成本。
- 明碼或可逆加密儲存驗證碼：不符合高安全標準。
- 只靠失敗次數，不做 rate limiting：無法涵蓋重寄轟炸或高頻猜碼行為。

## 4. 無 Cache 的一致性策略

**Decision**: 以 SQL Server 作為唯一真實來源，不在 Session、快取或記憶體保存驗證流程狀態。關鍵命令以資料庫交易維護一致性，並透過 rowversion/鎖定讀取避免併發覆寫。

**Rationale**: 沒有 cache 時，只要會員驗證狀態、active challenge 與錯誤次數都由資料庫維護，應用程式重啟或多節點部署都不會造成狀態分裂。這也讓測試與審計更可驗證。

**Alternatives considered**:
- 用 IMemoryCache 或 Session 暫存驗證狀態：重啟即遺失，且不利於稽核。
- 全域使用最嚴格隔離等級：會製造不必要的鎖競爭，應只在命令交易中精準使用。

## 5. Tailwind CSS v4 + Vanilla JS 整合

**Decision**: 使用 Tailwind CSS v4 CLI 直接編譯到 MVC 的靜態資產目錄，前端只使用 Vanilla JS 處理驗證碼六格輸入、自動跳位、重寄倒數、按鈕 disable 與錯誤聚焦；整體 UI 嚴格採用 DESIGN.md 的 transactional track。

**Rationale**: 這是 MVC 專案下最低複雜度的前端整合方式，不需要引入 Vite、SPA 框架或額外狀態管理。視覺風格應使用 DESIGN.md 定義的 transactional track：`canvas-light` / `canvas-cream`、黑色主 CTA pill、必要時以 `aloe`/`pistachio` 做 light-track 點綴、警示用 amber、錯誤用 red，並維持 pill 按鈕系統。字體方面因禁止商業套件，實作時以開源的 Inter Variable / Inter Display light weights 近似 display/body 層級，而不引入商業字型套件。

**Alternatives considered**:
- 導入 Vite、React 或 Vue：超出需求，增加建置與維護複雜度。
- 使用 jQuery：功能上足夠，但不必要地增加依賴。
- 自行發明另一套圓角矩形按鈕或 teal/cyan 主題：違反 DESIGN.md 的 pill button 與交易頁色彩系統。

## 6. 自動化測試策略

**Decision**: 採三層測試：單元測試、SQL Server-backed 整合測試、少量 Web integration tests。

**Rationale**: 這個功能最大的風險是狀態機、資料一致性與安全規則，而非複雜 UI。單元測試驗證狀態轉移與安全規則；整合測試驗證 unique index、交易與 active challenge 限制；Web tests 驗證 MVC 表單、Anti-forgery 與轉導行為。

**Alternatives considered**:
- 只做 Controller 測試：無法覆蓋資料庫與交易風險。
- 只做大量 UI 自動化：成本高，且抓不到 SQL 邊界與唯一性問題。