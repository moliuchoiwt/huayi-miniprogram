# 圖片篩查「防自動扣費」防護模組

本模組解決一個具體風險：**騰訊雲按量付費 API（圖片鑒黃 / 圖像標籤 / OCR）在用戶上傳圖片時自動調用，免費額用盡後會從賬戶餘額「自動扣費」**，開發者可能一個月後查單才發現。

新增 4 個檔案（放進原有 huayiWebApi 項目對應目錄即可）：

| 檔案 | 位置 | 作用 |
|------|------|------|
| `EmailClient.cs` | `YW.Common/` | 郵件發送（Gmail SMTP，可換 SendGrid/企業郵） |
| `RedisQuotaHelper.cs` | `YW.Common/` | 基於 Redis 的月度配額原子計數器 |
| `QuotaGuard.cs` | `YW.Service/ImageAudit/` | 防護核心：開關 + 配額鎖 + 額度用完發郵件 |
| `ImageAuditService.cs` | `YW.Service/ImageAudit/` | 業務包裝範本（含降級邏輯、紅字提示文案） |

另需把 `appsettings.addon.json` 的節點合併進 `YW.WebApi/appsettings.json`。

---

## 防護機制（你要求的 4 點）

1. **免費額度配額鎖（Redis 計數器）**
   - 每種雲 API 獨立計數，key 形如 `quota:ImageTag:2026-08`，月底自動過期重置。
   - 用 Lua 腳本做原子自增+判斷，高併發下不會超額。
   - 用盡 → 自動降級「人工審核」，不再 call 騰訊雲，**不再扣費**。

2. **鉴黄「試用包 + 失敗降級」**
   - 雲 API 報錯 / 超額 → 圖片標記「待人工審核」，不阻塞用戶上傳，不重試燒錢。

3. **配置開關 `ImageAudit:Enabled`**
   - 設 `false` 一鍵關掉所有騰訊雲調用，圖片全部走人工審核，**0 成本**。

4. **絕不重複調用**
   - `QuotaGuard.ShouldCallCloud()` 內部只做一次原子消耗；業務層每張圖只調一次對應 API。

5. **★ 額外：免費額用完發郵件通知** `studioofjoyhk@gmail.com`
   - 某配額本月**首次**用盡時，自動發郵件通知管理員（含已用量/上限/時間）。
   - 用 Redis 標記 `quota:notified:...:{yyyy-MM}`，**每月只通知一次**，不會轟炸郵箱。

---

## 部署步驟

1. 將 4 個 `.cs` 檔 copy 進對應目錄。
2. 合併 `appsettings.addon.json` 的 `ImageAudit` 與 `EmailSetting` 節點到 `YW.WebApi/appsettings.json`。
3. `YW.Common.csproj` 確認已引用 `Microsoft.Extensions.Caching.StackExchangeRedis`（現有已有），`StackExchange.Redis` 會隨依賴自動可用。
4. 填寫 `EmailSetting:Password` = Gmail **應用程式專用密碼**（不是登入密碼，見下）。
5. 重新編譯 `huayiWebApi`。

---

## Gmail SMTP 設置（重要）

Gmail **不能用普通登入密碼**發 SMTP，必須：
1. 登入 Google 帳號 → 安全性 → 開啟「兩步驗證」。
2. 搜尋「應用程式密碼」→ 生成一組 16 位密碼。
3. 把該密碼填進 `EmailSetting:Password`。

（若改用 SendGrid / 騰訊企業郵，只改 `EmailSetting` 節點，代碼無需改。）

---

## 接騰訊雲（後續步驟）

`ImageAuditService.cs` 中 `CallTencentXxxAsync` 目前是 `NotImplementedException` 佔位。接駁時：
1. NuGet 引入騰訊雲 SDK（`TencentCloud.CMS` / `TencentCloud.Tiia` / `TencentCloud.Ocr`）。
2. 替換 3 個 TODO 方法為真實調用。
3. 在原有 `Upload` / `SubmitShop` / `CreateTask` / 評價上傳處，呼叫 `ImageAuditService` 對應方法並處理 `AuditResult`。

未接 SDK 前，配額鎖與郵件通知邏輯已可獨立運作（模擬降級路徑）。

---

## 上線前建議（費用安全）

- 去騰訊雲費用中心設 **月度預算告警**（到錢會短信/郵件提醒你）。
- 確認三項服務各自的免費額度（尤其鉴黄需手動領試用包，非每月自動續）。
- 初期把 `ImageAudit:Enabled` 設 `false` 跑通流程，確認無誤再設 `true`。
