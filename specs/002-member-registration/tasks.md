# Tasks: 會員註冊流程

**Input**: Design documents from `/specs/002-member-registration/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/member-registration.md, quickstart.md

**Tests**: 依憲章要求，本功能必須包含單元測試、SQL Server 整合測試與 MVC/Web 路由測試。

**Organization**: 任務依使用者故事分組，確保每個故事都可獨立實作、測試與驗證。

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 建立 .NET 10 MVC 專案骨架、測試專案與前端建置基礎。

- [X] T001 建立解決方案與主 MVC 專案骨架於 Duotify.Membership.sln 和 src/Duotify.Membership.Web/Duotify.Membership.Web.csproj
- [X] T002 建立測試專案骨架於 tests/Duotify.Membership.Web.UnitTests/Duotify.Membership.Web.UnitTests.csproj、tests/Duotify.Membership.Web.IntegrationTests/Duotify.Membership.Web.IntegrationTests.csproj、tests/Duotify.Membership.Web.WebTests/Duotify.Membership.Web.WebTests.csproj
- [X] T003 [P] 初始化 Tailwind CSS v4 CLI 建置於 src/Duotify.Membership.Web/package.json 和 src/Duotify.Membership.Web/Styles/app.css
- [X] T004 [P] 設定共用建置與測試屬性於 Directory.Build.props 和 .gitignore

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 建立所有使用者故事都依賴的核心資料模型、存取層、安全基礎與 UI 外殼。

**⚠️ CRITICAL**: 完成這一階段前，不應開始任何使用者故事實作。

- [X] T005 建立 SQL Server schema 與索引腳本於 src/Duotify.Membership.Web/Infrastructure/Data/Sql/Schema/001_member_registration.sql
- [X] T006 [P] 定義核心實體於 src/Duotify.Membership.Web/Domain/Entities/Member.cs、src/Duotify.Membership.Web/Domain/Entities/EmailVerificationChallenge.cs、src/Duotify.Membership.Web/Domain/Entities/SecurityAuditLog.cs
- [X] T007 [P] 定義驗證與權限列舉/值物件於 src/Duotify.Membership.Web/Domain/ValueObjects/EmailVerificationStatus.cs、src/Duotify.Membership.Web/Domain/ValueObjects/CapabilityStatus.cs、src/Duotify.Membership.Web/Domain/ValueObjects/ChallengeStatus.cs
- [X] T008 [P] 建立資料存取介面與連線工廠於 src/Duotify.Membership.Web/Application/Interfaces/IMemberRepository.cs、src/Duotify.Membership.Web/Application/Interfaces/IChallengeRepository.cs、src/Duotify.Membership.Web/Application/Interfaces/ISecurityAuditLogRepository.cs、src/Duotify.Membership.Web/Infrastructure/Data/SqlConnectionFactory.cs
- [X] T009 實作 SQL Server repositories 與交易協調於 src/Duotify.Membership.Web/Infrastructure/Repositories/MemberRepository.cs、src/Duotify.Membership.Web/Infrastructure/Repositories/ChallengeRepository.cs、src/Duotify.Membership.Web/Infrastructure/Repositories/SecurityAuditLogRepository.cs
- [X] T010 [P] 建立安全基礎元件於 src/Duotify.Membership.Web/Infrastructure/Security/PasswordHasherAdapter.cs、src/Duotify.Membership.Web/Infrastructure/Security/VerificationCodeProtector.cs、src/Duotify.Membership.Web/Infrastructure/Security/RateLimitPolicies.cs
- [X] T011 [P] 建立組態與寄信選項於 src/Duotify.Membership.Web/Application/Interfaces/IEmailSender.cs、src/Duotify.Membership.Web/Infrastructure/Email/SmtpEmailSender.cs、src/Duotify.Membership.Web/appsettings.json、src/Duotify.Membership.Web/appsettings.Development.json
- [X] T012 建立共用 MVC 外殼、DI 與 light transactional track 樣式於 src/Duotify.Membership.Web/Program.cs、src/Duotify.Membership.Web/Views/Shared/_Layout.cshtml、src/Duotify.Membership.Web/wwwroot/css/app.css

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - 建立可登入帳號 (Priority: P1) 🎯 MVP

**Goal**: 讓新使用者提交身分證字號、姓名、E-Mail 與密碼，成功建立待驗證帳號並收到第一封驗證碼信件。

**Independent Test**: 使用未註冊資料提交註冊表單，系統應建立待驗證會員、建立第一筆 active challenge、寄送驗證信並停留在未登入狀態。

### Tests for User Story 1

- [X] T013 [P] [US1] 新增註冊驗證與重複資料規則單元測試於 tests/Duotify.Membership.Web.UnitTests/Application/RegisterMemberServiceTests.cs
- [ ] T014 [P] [US1] 新增會員唯一性與註冊交易整合測試於 tests/Duotify.Membership.Web.IntegrationTests/Data/RegistrationPersistenceTests.cs
- [X] T015 [P] [US1] 新增 GET/POST `/register` 路由契約測試於 tests/Duotify.Membership.Web.WebTests/Registration/RegisterRouteContractTests.cs
- [ ] T016 [P] [US1] 新增註冊請求延遲預算測試於 tests/Duotify.Membership.Web.IntegrationTests/Performance/RegisterPerformanceTests.cs

### Implementation for User Story 1

- [X] T017 [P] [US1] 建立註冊 ViewModel 於 src/Duotify.Membership.Web/ViewModels/Registration/RegisterViewModel.cs 和 src/Duotify.Membership.Web/ViewModels/Registration/RegisterSubmittedViewModel.cs
- [X] T018 [US1] 實作註冊流程服務於 src/Duotify.Membership.Web/Application/Services/RegisterMemberService.cs
- [X] T019 [US1] 實作註冊 Controller action 於 src/Duotify.Membership.Web/Controllers/RegistrationController.cs
- [X] T020 [P] [US1] 建立註冊頁 Razor View 於 src/Duotify.Membership.Web/Views/Registration/Register.cshtml
- [X] T021 [P] [US1] 實作註冊頁前端互動與 transactional track 樣式於 src/Duotify.Membership.Web/wwwroot/js/register.js 和 src/Duotify.Membership.Web/wwwroot/css/app.css
- [X] T022 [US1] 串接驗證碼信件內容與註冊審計事件於 src/Duotify.Membership.Web/Infrastructure/Email/SmtpEmailSender.cs 和 src/Duotify.Membership.Web/Application/Services/RegisterMemberService.cs
- [X] T023 [US1] 補齊註冊頁 loading、成功、欄位錯誤、重複身分證字號與重複 E-Mail 狀態於 src/Duotify.Membership.Web/Views/Registration/Register.cshtml

**Checkpoint**: User Story 1 可獨立完成註冊與寄送首筆驗證碼，作為 MVP 驗證。

---

## Phase 4: User Story 2 - 以驗證碼完成 E-Mail 驗證 (Priority: P2)

**Goal**: 讓待驗證會員可輸入最新 6 位數驗證碼完成驗證，並支援重寄、過期、鎖定與舊碼失效規則。

**Independent Test**: 預先建立一筆待驗證會員與 active challenge，驗證正確碼、錯誤碼、逾期碼、連錯 3 次鎖定、重寄後新碼生效與舊碼失效。

### Tests for User Story 2

- [X] T024 [P] [US2] 新增驗證碼生命周期與鎖定規則單元測試於 tests/Duotify.Membership.Web.UnitTests/Application/VerifyEmailCodeServiceTests.cs
- [ ] T025 [P] [US2] 新增 active challenge 失效與重寄整合測試於 tests/Duotify.Membership.Web.IntegrationTests/Data/VerificationChallengeRepositoryTests.cs
- [X] T026 [P] [US2] 新增 POST `/register/verify` 與 POST `/register/verify/resend` 路由契約測試於 tests/Duotify.Membership.Web.WebTests/Registration/VerifyRouteContractTests.cs
- [ ] T027 [P] [US2] 新增驗證與重寄延遲預算測試於 tests/Duotify.Membership.Web.IntegrationTests/Performance/VerificationPerformanceTests.cs

### Implementation for User Story 2

- [X] T028 [P] [US2] 建立驗證與重寄 ViewModel 於 src/Duotify.Membership.Web/ViewModels/Registration/VerifyEmailViewModel.cs 和 src/Duotify.Membership.Web/ViewModels/Registration/ResendVerificationViewModel.cs
- [X] T029 [US2] 實作驗證碼比對與狀態轉移服務於 src/Duotify.Membership.Web/Application/Services/VerifyEmailCodeService.cs
- [X] T030 [US2] 實作重新寄送與解除鎖定服務於 src/Duotify.Membership.Web/Application/Services/ResendEmailVerificationService.cs
- [X] T031 [US2] 擴充驗證/重寄 Controller action 於 src/Duotify.Membership.Web/Controllers/RegistrationController.cs
- [X] T032 [P] [US2] 建立驗證碼輸入頁於 src/Duotify.Membership.Web/Views/Registration/Verify.cshtml
- [X] T033 [P] [US2] 實作驗證頁六格輸入、倒數與重寄互動於 src/Duotify.Membership.Web/wwwroot/js/verify.js 和 src/Duotify.Membership.Web/wwwroot/css/app.css
- [X] T034 [US2] 串接驗證審計、鎖定解除與舊碼失效規則於 src/Duotify.Membership.Web/Application/Services/ResendEmailVerificationService.cs 和 src/Duotify.Membership.Web/Infrastructure/Repositories/SecurityAuditLogRepository.cs
- [X] T035 [US2] 補齊驗證頁等待輸入、錯誤碼、逾期、鎖定、重寄成功與驗證成功狀態於 src/Duotify.Membership.Web/Views/Registration/Verify.cshtml

**Checkpoint**: User Story 2 可獨立完成驗證成功、鎖定、重寄與舊碼失效規則。

---

## Phase 5: User Story 3 - 未驗證帳號受限使用 (Priority: P3)

**Goal**: 讓未驗證會員可登入但僅能查看基本資料，任何互動操作皆被阻止並提示先完成 E-Mail 驗證。

**Independent Test**: 以一筆未驗證會員登入後進入會員區，確認能看到基本資料頁，但對互動操作會被擋下；完成驗證後同一操作恢復可用。

### Tests for User Story 3

- [X] T036 [P] [US3] 新增會員能力狀態與限制規則單元測試於 tests/Duotify.Membership.Web.UnitTests/Application/RestrictedAccessServiceTests.cs
- [ ] T037 [P] [US3] 新增未驗證能力狀態整合測試於 tests/Duotify.Membership.Web.IntegrationTests/Data/MemberCapabilityStateTests.cs
- [X] T038 [P] [US3] 新增未驗證會員區與限制提示 Web 測試於 tests/Duotify.Membership.Web.WebTests/MemberPortal/RestrictedAccessTests.cs
- [ ] T039 [P] [US3] 新增已驗證與未驗證互動路徑延遲測試於 tests/Duotify.Membership.Web.IntegrationTests/Performance/RestrictedAccessPerformanceTests.cs

### Implementation for User Story 3

- [X] T040 [US3] 實作目前會員能力解析服務於 src/Duotify.Membership.Web/Application/Interfaces/ICurrentMemberAccessor.cs 和 src/Duotify.Membership.Web/Application/Services/RestrictedAccessService.cs
- [X] T041 [US3] 實作會員區控制器與基本資料頁於 src/Duotify.Membership.Web/Controllers/MemberPortalController.cs 和 src/Duotify.Membership.Web/Views/MemberPortal/Profile.cshtml
- [X] T042 [US3] 實作 verified-only 動作過濾器於 src/Duotify.Membership.Web/Infrastructure/Security/RequireVerifiedMemberFilter.cs 和 src/Duotify.Membership.Web/Program.cs
- [X] T043 [P] [US3] 建立未驗證限制提示 partial 於 src/Duotify.Membership.Web/Views/Shared/_VerificationRestrictionBanner.cshtml
- [X] T044 [US3] 套用會員區互動限制與審計事件於 src/Duotify.Membership.Web/Controllers/MemberPortalController.cs 和 src/Duotify.Membership.Web/Infrastructure/Repositories/SecurityAuditLogRepository.cs
- [X] T045 [US3] 補齊未驗證僅可查看基本資料、受限操作提示與驗證後恢復使用狀態於 src/Duotify.Membership.Web/Views/MemberPortal/Profile.cshtml 和 src/Duotify.Membership.Web/Views/Shared/_Layout.cshtml

**Checkpoint**: 所有 user stories 都可獨立驗證，且未驗證限制規則完整落地。

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: 完成跨故事文件、安全硬化與最終驗證。

- [X] T046 [P] 更新實作與操作文件於 specs/002-member-registration/quickstart.md 和 README.md
- [X] T047 強化安全標頭、Cookie 與錯誤處理細節於 src/Duotify.Membership.Web/Program.cs 和 src/Duotify.Membership.Web/Infrastructure/Security/SecurityHeadersConfiguration.cs
- [X] T048 [P] 補齊跨故事回歸測試於 tests/Duotify.Membership.Web.WebTests/Regression/MemberRegistrationRegressionTests.cs
- [X] T049 執行 quickstart 驗證與記錄驗收結果於 specs/002-member-registration/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 可立即開始。
- **Foundational (Phase 2)**: 依賴 Setup 完成，且會阻擋所有使用者故事。
- **User Stories (Phase 3+)**: 都依賴 Foundational 完成。
- **Polish (Phase 6)**: 依賴所有目標使用者故事完成。

### User Story Dependencies

- **User Story 1 (P1)**: 只依賴 Foundational，完成後即可形成 MVP。
- **User Story 2 (P2)**: 依賴 Foundational；雖然業務上建立在待驗證帳號之上，但可用預先建立的 pending member/challenge 獨立測試。
- **User Story 3 (P3)**: 依賴 Foundational；可用預先建立的未驗證/已驗證會員狀態獨立測試，不必等待 US2 UI 完成。

### Within Each User Story

- 先寫測試並讓其失敗。
- 先建立 ViewModel/核心模型，再實作 Service。
- Service 完成後再接 Controller 與 View。
- UX 狀態補齊後，故事才算完成。
- 效能驗證測試需在故事完成前通過。

### Parallel Opportunities

- Setup 中標記 `[P]` 的任務可平行進行。
- Foundational 中的實體、列舉、介面、安全元件與組態任務可平行進行。
- 各故事中的單元、整合、Web/contract、效能測試可平行撰寫。
- 各故事中的 ViewModel、View 與前端互動任務可在 Service 契約固定後平行進行。

---

## Parallel Example: User Story 1

```bash
# 同步撰寫 US1 測試
Task: T013 RegisterMemberServiceTests
Task: T014 RegistrationPersistenceTests
Task: T015 RegisterRouteContractTests
Task: T016 RegisterPerformanceTests

# Service 契約固定後，可同步處理畫面與前端
Task: T020 Register.cshtml
Task: T021 register.js and app.css
```

---

## Parallel Example: User Story 2

```bash
# 同步撰寫驗證/重寄測試
Task: T024 VerifyEmailCodeServiceTests
Task: T025 VerificationChallengeRepositoryTests
Task: T026 VerifyRouteContractTests
Task: T027 VerificationPerformanceTests

# 畫面與互動可平行
Task: T032 Verify.cshtml
Task: T033 verify.js and app.css
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. 完成 Phase 1: Setup。
2. 完成 Phase 2: Foundational。
3. 完成 Phase 3: User Story 1。
4. 驗證 T013-T023 全數通過後，先示範 MVP。

### Incremental Delivery

1. Setup + Foundational 完成後，先交付 US1。
2. US1 穩定後，加入 US2 的驗證/重寄/鎖定規則。
3. US2 穩定後，加入 US3 的未驗證限制與會員區提示。
4. 最後完成跨故事安全硬化與回歸驗證。

### Parallel Team Strategy

1. 一位成員先完成 Setup 與 Foundational。
2. Foundation 完成後：
   - 開發者 A 處理 US1
   - 開發者 B 處理 US2
   - 開發者 C 處理 US3
3. 所有人最後共同處理 Polish 與回歸驗證。

---

## Notes

- `[P]` 表示不同檔案、可平行進行的任務。
- `[US1]`、`[US2]`、`[US3]` 對應 spec 中的使用者故事。
- 所有故事都必須在自動化測試、UX 狀態與效能驗證完成後才可視為完成。