# 🎮 個人遊戲啟動器與時數統計平台 (Game Launcher & Tracker)

這是一個基於 C# Windows Forms 開發的本地遊戲管理與數據統計平台。旨在提供一個流暢、美觀且具備防呆機制的遊戲管理工具，讓玩家能統一管理電腦中的遊戲，並自動記錄遊玩歷程與數據分析。

## ✨ 核心功能特色

📥 遊戲庫存管理與動態 UI

支援載入本地端 .exe 遊戲執行檔。

雙軌封面系統：自動透過 Windows API 提取執行檔的圖示，或允許使用者手動上傳自訂高畫質海報。

動態生成的流暢網格介面（FlowLayoutPanel），並支援依「遊戲分類」進行即時下拉篩選。

⏱️ 智慧啟動與底層程序監聽

透過 System.Diagnostics.Process 啟動並監聽遊戲運行狀態。

具備完善防呆機制：防止同一款遊戲重複啟動、自動檢查執行檔是否存在，並使用 UseShellExecute 解決權限存取被拒 (Access Denied) 的問題。

遊戲關閉時，自動精算遊玩秒數並寫入資料庫。

📊 數據視覺化儀表板 (Dashboard)

生涯總時數：自動加總所有遊戲的遊玩時間，並格式化為易讀的時/分/秒。

排行榜：利用 SQL JOIN 與 GROUP BY 結算最常遊玩的 Top 3 遊戲。

活躍度圖表：整合 WinForms DataVisualization.Charting，繪製過去 7 天的每日遊玩時數長條圖。

📝 中介資料編輯 (Metadata)

專屬的編輯視窗，可隨時修改遊戲名稱、分類標籤與發行商。

## 🛠️ 開發技術與套件

程式語言：C# (.NET Framework / .NET Core)

使用者介面：Windows Forms (WinForms)

資料庫：SQLite (System.Data.SQLite.Core) - 輕量級本地端資料庫儲存。

圖表渲染：System.Windows.Forms.DataVisualization

## 📸 系統畫面與操作說明

1. 遊戲主畫面與動態卡片

首頁展示所有已加入的遊戲。每張卡片皆包含封面、總時數與操作按鈕。上方支援分類篩選。

<img width="785" height="589" alt="image" src="https://github.com/user-attachments/assets/96a4ad88-4b9d-4105-82b7-b2de910ceafa" />

2. 新增與編輯遊戲資訊

點擊「編輯設定」可修改遊戲分類、發行商，或上傳自訂的精美封面。

<img width="384" height="434" alt="image" src="https://github.com/user-attachments/assets/2d26d424-fd37-4663-9962-5ed3170584b9" />

3. 遊玩歷史詳細紀錄

點擊「詳細紀錄」可彈出唯讀的 DataGridView，檢視該遊戲每一次啟動的詳細時間戳記與遊玩長度。

<img width="429" height="388" alt="image" src="https://github.com/user-attachments/assets/714286f7-edee-4c5d-b68e-bf6b8553fc33" />

4. 數據儀表板 (Dashboard)

以視覺化圖表呈現生涯總時數、Top 3 排行榜以及近 7 天活躍度長條圖。

<img width="781" height="584" alt="image" src="https://github.com/user-attachments/assets/573b5907-ce80-4627-9670-6f81796c0ac7" />

## 🚀 安裝與執行說明 (How to Run)

將本專案（或從 GitHub Clone）下載至本地端。

確認電腦已安裝 Visual Studio 2019 / 2022 與 .NET 桌面開發工作負載。

點擊開啟 GameLauncher.sln 方案檔。

在方案總管中對專案點擊右鍵 ➔ 「管理 NuGet 套件」，確認以下套件已還原：

System.Data.SQLite.Core

WindowsAPICodePack-Shell

按下 F5 或點擊「開始」編譯並執行專案。
(註：初次執行時，程式會在 bin/Debug 目錄下自動生成 GameLibrary.db 資料庫檔案，無需額外架設資料庫伺服器)

## 📁 專案架構說明

Game.cs：遊戲物件模型（Model），負責封裝遊戲屬性。

DatabaseManager.cs：資料庫存取層（DAL），集中處理所有 SQLite 連線與 CRUD / 統計 SQL 語法。

GameLauncher.cs (Form1)：主程式介面，處理 UI 動態生成、程序監聽 (Process_Exited) 與跨執行緒 UI 更新。

EditGameForm.cs：中介資料與封面編輯視窗。

DashboardForm.cs：數據分析儀表板，負責處理 Chart 控制項的渲染。

## 🔗 資料來源與參考文獻

SQLite 官方文件：https://www.sqlite.org/docs.html

Microsoft Docs - Process 類別：https://learn.microsoft.com/zh-tw/dotnet/api/system.diagnostics.process

SteamGridDB (遊戲高畫質封面來源)：https://www.steamgriddb.com/

Developed for Windows Programming (II) Final Project.
