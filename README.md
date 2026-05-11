# duotify-membership-v6

這個儲存庫是課堂上建立的 **GitHub Spec Kit** 示範專案，主要用途是讓學生練習以 **規格驅動開發（Spec-Driven Development, SDD）** 的方式，從需求、規劃、任務拆解一路走到實作。

本專案目前重點不在商業功能本身，而是在了解 **Spec Kit 的工作流程、檔案結構與協作方式**，因此很適合作為上課、練習與回顧的參考教材。

## 這個專案的用途

- 示範如何在 GitHub Copilot / Spec Kit 環境中建立需求規格
- 示範如何從規格產生實作計畫與任務清單
- 示範如何透過一致的模板與流程管理功能開發
- 作為學生課後複習與練習 Spec Kit 指令的範例專案

## 這個專案在示範什麼

此專案展示的是一套標準流程：

1. **specify**：先定義需求與使用情境
2. **plan**：再整理技術規劃與實作策略
3. **tasks**：把工作拆成可執行的任務
4. **implement**：依照規格與任務進行實作

在這個流程中，重點是先把「要做什麼」說清楚，再進入「怎麼做」。

## 儲存庫結構說明

目前這個儲存庫主要由 Spec Kit 所需的設定、模板與輔助檔案組成：

```plaintext
.
├── .github/
│   ├── copilot-instructions.md    # Copilot 額外指示
│   └── prompts/                   # Spec Kit 提示詞
├── .specify/
│   ├── memory/                    # 專案規範與憲章
│   ├── templates/                 # spec / plan / tasks 模板
│   ├── workflows/                 # Spec Kit 工作流程設定
│   ├── scripts/                   # 自動化腳本
│   └── extensions/                # 延伸功能（例如 Git 工作流程）
├── .vscode/
│   └── settings.json              # VS Code 設定
├── AGENTS.md                      # 本專案工作代理的補充說明
└── README.md
```

## 重要檔案與目錄

### `.specify/templates/`

這裡放的是 Spec Kit 的核心模板，包含：

- `spec-template.md`：功能規格模板
- `plan-template.md`：實作計畫模板
- `tasks-template.md`：任務清單模板
- `constitution-template.md`：專案憲章模板

學生可以透過這些模板理解 Spec Kit 在每個階段期望產出的內容。

### `.specify/workflows/`

這裡定義了 Spec Kit 的工作流程。以本專案為例，可看到一個完整流程：

`specify → review-spec → plan → review-plan → tasks → implement`

這代表功能開發不是直接寫程式，而是要先經過需求與規劃的確認。

### `.specify/memory/constitution.md`

這份文件可以視為專案的「開發憲章」，說明此專案對於：

- 程式碼品質
- 測試要求
- 使用者體驗一致性
- 效能預期
- 維護性與簡潔性

有哪些基本原則。

### `.specify/extensions/git/`

這是 Git 工作流程的延伸模組，提供像是：

- 建立 feature branch
- 驗證分支命名
- 自動提交

這些功能可協助把 Spec Kit 流程與 Git 協作整合在一起。

## 建議學生如何閱讀這個專案

如果你是第一次接觸 Spec Kit，建議依照下面順序閱讀：

1. 先看 `README.md` 了解整體用途
2. 再看 `.specify/memory/constitution.md`，理解專案的規範
3. 接著閱讀 `.specify/templates/` 下的各種模板
4. 最後看 `.specify/workflows/speckit/workflow.yml`，了解完整工作流程

這樣會比較容易掌握：

- Spec Kit 為什麼要先寫規格
- 每個階段產生什麼文件
- 文件彼此之間如何銜接

## 課堂練習建議

你可以把這個專案當成練習場，嘗試以下活動：

1. 想一個小功能需求
2. 使用 Spec Kit 建立 spec
3. 根據 spec 產生 plan
4. 再把 plan 拆成 tasks
5. 最後再進入實作

練習重點不是一次把功能做大，而是學會：

- 把需求寫清楚
- 把實作範圍界定清楚
- 把工作拆成可以執行的小步驟

## 參考指令流程

若你是在支援 Spec Kit 的環境中操作，常見流程會像這樣：

```plaintext
/speckit.specify
/speckit.plan
/speckit.tasks
/speckit.implement
```

實際指令名稱與使用方式會依你的工具環境而略有不同，但概念都是相同的：  
**先規格、再規劃、再拆任務、最後實作。**

## 這個專案目前的狀態

- 這是一個教學示範用儲存庫
- 目前主要內容是 Spec Kit 設定與模板
- 尚未包含完整的產品程式碼結構
- 適合用來理解流程，而不是作為正式產品範例

## 學生應該學到什麼

看完這個專案後，建議你至少能回答以下問題：

- Spec Kit 的核心流程是什麼？
- `spec`、`plan`、`tasks` 三種文件各自負責什麼？
- 為什麼功能開發前要先做需求與規劃？
- 專案規範（constitution）在團隊協作中扮演什麼角色？
- 為什麼 Git 分支與提交流程也可以納入規格化管理？

## 總結

這個儲存庫不是用來展示複雜功能，而是用來示範一種更有紀律的開發流程。  
對學生來說，最重要的不是背指令，而是理解：

> **先把需求說清楚，再把實作做正確。**

如果你能透過這個專案理解 Spec Kit 的流程與思維方式，就已經達成這份示範專案的學習目的。
