# Data Model: 會員註冊流程

## 1. 會員帳號 Member

**Purpose**: 代表可以登入平台的會員主體，承載登入憑證、唯一身份欄位與驗證狀態。

### Fields

| Field | Type | Required | Rules |
| --- | --- | --- | --- |
| MemberId | GUID | Yes | 主鍵 |
| RegistrationReference | string | Yes | 唯一；供驗證/重寄流程識別該次註冊流程 |
| NationalId | string | Yes | 唯一；格式依現行會員制度規則驗證 |
| FullName | string | Yes | 非空；長度依 UI/資料庫上限限制 |
| Email | string | Yes | 唯一；需符合 E-Mail 格式 |
| PasswordHash | string | Yes | 由 ASP.NET Core PasswordHasher 產生 |
| EmailVerificationStatus | enum | Yes | `Pending`, `Verified` |
| CapabilityStatus | enum | Yes | `Restricted`, `FullAccess` |
| EmailVerifiedAt | datetimeoffset? | No | 驗證成功時寫入 |
| CreatedAt | datetimeoffset | Yes | 建立時間 |
| UpdatedAt | datetimeoffset | Yes | 最後更新時間 |

### Validation Rules

- `NationalId` 不可重複。
- `Email` 不可重複。
- `RegistrationReference` 不可重複。
- 密碼需符合 8 到 20 碼，且同時包含大寫英文、小寫英文與數字。
- `CapabilityStatus` 與 `EmailVerificationStatus` 必須同步：`Pending` 對應 `Restricted`，`Verified` 對應 `FullAccess`。

### State Transitions

- `Pending/Restricted` -> `Verified/FullAccess`：輸入正確且有效的最新驗證碼。
- `Pending/Restricted` 維持不變：錯誤碼、逾期碼、舊碼、鎖定中驗證。

## 2. E-Mail 驗證挑戰 EmailVerificationChallenge

**Purpose**: 代表寄送給會員的一次 6 位數驗證碼挑戰，負責過期、失效、錯誤次數與鎖定規則。

### Fields

| Field | Type | Required | Rules |
| --- | --- | --- | --- |
| ChallengeId | GUID | Yes | 主鍵 |
| MemberId | GUID | Yes | 外鍵指向 Member |
| CodeHash | binary/string | Yes | 儲存 keyed hash/HMAC 結果，不存明碼 |
| KeyVersion | string | Yes | 用於驗證 hash/HMAC 版本 |
| SentAt | datetimeoffset | Yes | 寄送時間 |
| ExpiresAt | datetimeoffset | Yes | `SentAt + 5 minutes` |
| FailedAttemptCount | int | Yes | 預設 0；達 3 時鎖定 |
| Status | enum | Yes | `Active`, `Locked`, `Verified`, `Expired`, `Invalidated` |
| InvalidatedAt | datetimeoffset? | No | 重寄後舊 challenge 失效時間 |
| VerifiedAt | datetimeoffset? | No | 驗證成功時間 |
| LockedAt | datetimeoffset? | No | 連續 3 次錯誤後寫入 |
| ReplacedByChallengeId | GUID? | No | 若因重寄失效則記錄新 challenge |

### Validation Rules

- 同一位會員任一時間只能有一筆 `Active` 或 `Locked` 的目前挑戰。
- 只有最新 challenge 可被接受。
- 達到 3 次錯誤後，當前 challenge 進入 `Locked`。
- `Locked` 僅能透過建立新 challenge 並讓舊 challenge `Invalidated` 的方式解除。

### State Transitions

- `Active` -> `Verified`：驗證碼正確且未逾期。
- `Active` -> `Locked`：連續 3 次錯誤。
- `Active` -> `Expired`：超過 `ExpiresAt` 且未完成驗證。
- `Active` -> `Invalidated`：使用者重寄驗證碼。
- `Locked` -> `Invalidated`：使用者重寄驗證碼。

## 3. 驗證鎖定狀態 VerificationLockState

**Purpose**: 表示會員當前驗證流程是否因錯誤次數過多而被鎖定；實作上可由當前 challenge 狀態推導。

### Fields

| Field | Type | Required | Rules |
| --- | --- | --- | --- |
| MemberId | GUID | Yes | 對應會員 |
| ActiveChallengeId | GUID | Yes | 對應目前鎖定中的 challenge |
| LockReason | enum | Yes | 固定為 `TooManyIncorrectCodes` |
| LockedAt | datetimeoffset | Yes | 第 3 次錯誤時寫入 |
| UnlockCondition | enum | Yes | 固定為 `ResendNewCode` |

### Validation Rules

- 只有當 active challenge 為 `Locked` 時，此狀態才存在。
- 重寄新碼後，舊的鎖定狀態必須終止。

## 4. 安全審計紀錄 SecurityAuditLog

**Purpose**: 追蹤註冊、驗證、重寄、鎖定與未驗證受限操作等安全事件。

### Fields

| Field | Type | Required | Rules |
| --- | --- | --- | --- |
| AuditLogId | GUID | Yes | 主鍵 |
| MemberId | GUID? | No | 已識別會員時填入 |
| ChallengeId | GUID? | No | 涉及 challenge 時填入 |
| EventType | enum | Yes | `RegistrationCreated`, `DuplicateNationalIdRejected`, `DuplicateEmailRejected`, `VerificationSucceeded`, `VerificationFailed`, `VerificationLocked`, `VerificationResent`, `RestrictedFeatureDenied` |
| OccurredAt | datetimeoffset | Yes | 事件時間 |
| IpAddress | string? | No | 視隱私政策保存必要資訊 |
| UserAgent | string? | No | 可裁切保存摘要 |
| MetadataJson | string? | No | 不得包含明碼密碼或驗證碼 |

## Relationships

- `Member` 1 -> N `EmailVerificationChallenge`
- `Member` 1 -> N `SecurityAuditLog`
- `EmailVerificationChallenge` 1 -> N `SecurityAuditLog`（可選）
- `VerificationLockState` 為 `EmailVerificationChallenge.Status = Locked` 的衍生狀態

## Database Constraints

- Unique index on `Members.NationalId`
- Unique index on `Members.Email`
- Unique index on `Members.RegistrationReference`
- Filtered unique index on current challenge set，確保每位會員只有一筆有效中的 challenge
- Foreign key from `EmailVerificationChallenges.MemberId` to `Members.MemberId`
- Foreign key from `SecurityAuditLogs.MemberId` to `Members.MemberId`
