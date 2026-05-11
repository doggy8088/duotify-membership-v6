# Contract: 會員註冊流程

此文件定義 MVC 註冊與驗證流程對外可見的 HTTP/頁面契約。格式以 server-rendered MVC flow 為主，而非 JSON API。

## Route Surface

### 1. GET /register

- **Purpose**: 顯示註冊表單。
- **Fields displayed**: 身分證字號、姓名、E-Mail、密碼。
- **States**:
  - 初始空白表單
  - 欄位驗證錯誤
  - 重複身分證字號錯誤
  - 重複 E-Mail 錯誤

### 2. POST /register

- **Purpose**: 建立會員與首筆驗證挑戰。
- **Request fields**:

| Field | Type | Required | Validation |
| --- | --- | --- | --- |
| nationalId | form string | Yes | 依會員制度格式驗證，且不可重複 |
| fullName | form string | Yes | 非空 |
| email | form string | Yes | E-Mail 格式，且不可重複 |
| password | form string | Yes | 8-20 碼，含大寫、小寫、數字 |

- **Security requirements**:
  - 必須驗證 anti-forgery token
  - 必須套用註冊端點 rate limiting
- **Success outcome**:
  - 建立會員與首筆 challenge
  - 寄送驗證碼 E-Mail
  - Redirect 到 `/register/verify`，並帶入不暴露敏感資訊的流程參考值
- **Failure outcome**:
  - 留在註冊頁並顯示欄位或業務錯誤

### 3. GET /register/verify

- **Purpose**: 顯示驗證碼輸入畫面。
- **Fields displayed**: 6 位數驗證碼輸入、重寄按鈕、狀態提示。
- **States**:
  - 等待輸入
  - 驗證碼錯誤
  - 驗證碼已逾期
  - 驗證流程已鎖定
  - 重寄成功

### 4. POST /register/verify

- **Purpose**: 驗證最新的 6 位數驗證碼。
- **Request fields**:

| Field | Type | Required | Validation |
| --- | --- | --- | --- |
| registrationRef | form string | Yes | 對應當前註冊流程參考值 |
| verificationCode | form string | Yes | 必須為 6 位數 |

- **Security requirements**:
  - 必須驗證 anti-forgery token
  - 必須套用 verify 端點 rate limiting
- **Success outcome**:
  - 將會員標記為已驗證
  - 啟用完整功能
  - 顯示驗證成功訊息並引導使用者自行登入
- **Failure outcome**:
  - 錯誤碼：顯示錯誤訊息並累積失敗次數
  - 第 3 次錯誤：鎖定直到重寄
  - 過期碼：顯示已失效訊息
  - 舊碼：視為無效碼

### 5. POST /register/verify/resend

- **Purpose**: 重新寄送新的驗證碼並使舊碼失效。
- **Request fields**:

| Field | Type | Required | Validation |
| --- | --- | --- | --- |
| registrationRef | form string | Yes | 對應當前註冊流程參考值 |

- **Security requirements**:
  - 必須驗證 anti-forgery token
  - 必須套用 resend 端點 rate limiting
- **Success outcome**:
  - 舊 challenge 失效
  - 新 challenge 建立並寄信
  - 若原驗證流程被鎖定，於此時解除鎖定
- **Failure outcome**:
  - 若流程參考值失效或會員已驗證，顯示對應訊息並拒絕重寄

## Cross-Cutting Contract Rules

- 註冊成功後不得自動登入。
- 未驗證會員登入後僅能查看基本資料；任何互動功能應被拒絕並顯示需先完成 E-Mail 驗證的提示。
- 所有使用者可見錯誤都需避免洩漏密碼、驗證碼雜湊、內部資料表名稱或安全實作細節。
- 所有 POST 行為都必須保留可追蹤的安全審計事件。