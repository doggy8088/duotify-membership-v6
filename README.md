# Duotify Membership

此專案實作會員註冊、E-Mail 驗證、登入與未驗證會員受限使用流程，技術堆疊為 .NET 10 MVC、SQL Server、Tailwind CSS v4 與 Vanilla JS。

## 本機開發

1. 於 src/Duotify.Membership.Web 執行 npm install。
2. 於專案根目錄執行 dotnet build Duotify.Membership.slnx。
3. 於專案根目錄執行 DOTNET_ROLL_FORWARD=Major dotnet test Duotify.Membership.slnx。
4. 視需要設定 ConnectionStrings:DefaultConnection、Email 與 Security 組態。

## 功能摘要

- 註冊後建立 pending member 與第一筆驗證 challenge。
- 驗證碼 6 位數、5 分鐘有效、錯誤 3 次鎖定。
- 重寄會建立新 challenge 並使舊碼失效。
- 未驗證會員可登入，但僅能查看基本資料。
- 關鍵安全事件會寫入 SecurityAuditLogs。