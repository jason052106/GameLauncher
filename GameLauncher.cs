using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace GameLauncher
{
    public partial class GameLauncher : Form
    {
        private DatabaseManager dbManager;
        private FlowLayoutPanel flowLayoutPanel;
        private Button btnAddGame;

        // 紀錄遊戲啟動時間的字典 (防呆：防止同一款遊戲重複啟動)
        private Dictionary<int, DateTime> activeGames = new Dictionary<int, DateTime>();
       
        public GameLauncher()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            InitializeUI();
            LoadGamesToUI();
        }

        // 純程式碼動態生成 UI (避免依賴 Designer 拖拉)
        private void InitializeUI()
        {
            this.Text = "個人遊戲啟動與時數統計平台";
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(30, 30, 30); // 深色主題

            btnAddGame = new Button();
            btnAddGame.Text = "新增本地遊戲 (+)";
            btnAddGame.Size = new Size(150, 40);
            btnAddGame.Location = new Point(20, 20);
            btnAddGame.BackColor = Color.Teal;
            btnAddGame.ForeColor = Color.White;
            btnAddGame.FlatStyle = FlatStyle.Flat;
            btnAddGame.Click += BtnAddGame_Click;
            this.Controls.Add(btnAddGame);

            flowLayoutPanel = new FlowLayoutPanel();
            flowLayoutPanel.Location = new Point(20, 80);
            flowLayoutPanel.Size = new Size(740, 460);
            flowLayoutPanel.AutoScroll = true;
            this.Controls.Add(flowLayoutPanel);
        }

        // 新增遊戲按鈕事件
        private void BtnAddGame_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "執行檔 (*.exe)|*.exe";
                ofd.Title = "選擇遊戲執行檔";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string path = ofd.FileName;
                    string name = Path.GetFileNameWithoutExtension(path);

                    dbManager.AddGame(name, path);
                    LoadGamesToUI(); // 重新載入畫面
                }
            }
        }

        // 讀取資料庫並動態生成遊戲卡片
        private void LoadGamesToUI()
        {
            flowLayoutPanel.Controls.Clear();
            List<Game> games = dbManager.GetAllGames();

            foreach (var game in games)
            {
                // 調整卡片大小以容納圖片
                Panel card = new Panel();
                card.Size = new Size(200, 320);
                card.BackColor = Color.FromArgb(50, 50, 50);
                card.Margin = new Padding(10);

                // 新增 PictureBox 來顯示封面
                PictureBox picCover = new PictureBox();
                picCover.Size = new Size(180, 240);
                picCover.Location = new Point(10, 10);
                picCover.SizeMode = PictureBoxSizeMode.Zoom; // 讓圖片維持比例縮放
                picCover.Image = GetGameCover(game); // 呼叫我們剛剛寫的函數！

                // （進階巧思）點擊圖片也可以啟動遊戲
                picCover.Cursor = Cursors.Hand;
                picCover.Tag = game;
                picCover.Click += BtnPlay_Click;

                Label lblName = new Label();
                lblName.Text = game.Name;
                lblName.ForeColor = Color.White;
                lblName.AutoSize = false;
                lblName.Size = new Size(180, 25);
                lblName.Location = new Point(10, 260);
                lblName.Font = new Font("微軟正黑體", 10, FontStyle.Bold);
                lblName.TextAlign = ContentAlignment.MiddleCenter;

                Button btnPlay = new Button();
                btnPlay.Text = "啟動";
                btnPlay.Size = new Size(180, 25);
                btnPlay.Location = new Point(10, 290);
                btnPlay.BackColor = Color.DarkOliveGreen;
                btnPlay.ForeColor = Color.White;
                btnPlay.FlatStyle = FlatStyle.Flat;
                btnPlay.Tag = game;
                btnPlay.Click += BtnPlay_Click;

                // 依序把控制項加進卡片
                card.Controls.Add(picCover);
                card.Controls.Add(lblName);
                card.Controls.Add(btnPlay);

                flowLayoutPanel.Controls.Add(card);
            }
        }

        // 啟動遊戲與監聽邏輯
        private void BtnPlay_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Game game = btn.Tag as Game;

            if (activeGames.ContainsKey(game.Id))
            {
                MessageBox.Show("該遊戲已經在執行中！", "系統提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(game.ExecutablePath))
            {
                MessageBox.Show("找不到執行檔，請確認遊戲是否已移除。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = game.ExecutablePath;
                process.EnableRaisingEvents = true; // 允許觸發 Exited 事件

                // 這裡必須傳遞 game.Id 給監聽事件，所以用 Lambda 表達式
                process.Exited += (s, args) => Process_Exited(game.Id);

                process.Start();
                activeGames.Add(game.Id, DateTime.Now); // 記錄開始時間

                btn.Text = "執行中...";
                btn.BackColor = Color.Gray;
                btn.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("啟動失敗: " + ex.Message);
            }
        }

        // 遊戲關閉時的非同步處理
        private void Process_Exited(int gameId)
        {
            if (activeGames.ContainsKey(gameId))
            {
                DateTime startTime = activeGames[gameId];
                DateTime endTime = DateTime.Now;
                int durationSeconds = (int)(endTime - startTime).TotalSeconds;

                dbManager.LogPlaySession(gameId, durationSeconds);
                activeGames.Remove(gameId);

                // 因為 Exited 是在背景執行緒觸發，更新 UI 必須使用 Invoke
                this.Invoke((MethodInvoker)delegate {
                    LoadGamesToUI(); // 刷新 UI 狀態 (把按鈕改回"啟動遊玩")
                    MessageBox.Show($"遊戲結束！本次遊玩時間: {durationSeconds} 秒\n紀錄已寫入資料庫。", "統計完成");
                });
            }
        }

        private Image GetGameCover(Game game)
        {
            try
            {
                // 1. 檢查是否有使用者自訂的封面且檔案真實存在
                if (!string.IsNullOrEmpty(game.CoverImagePath) && File.Exists(game.CoverImagePath))
                {
                    // 使用 FileStream 讀取，避免檔案被系統鎖死
                    using (FileStream fs = new FileStream(game.CoverImagePath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(fs);
                    }
                }

                // 2. 如果沒有自訂封面，自動從 .exe 檔案中抽出預設 Icon
                if (File.Exists(game.ExecutablePath))
                {
                    Icon appIcon = Icon.ExtractAssociatedIcon(game.ExecutablePath);
                    if (appIcon != null)
                    {
                        return appIcon.ToBitmap(); // 將 Icon 轉為 WinForms 可用的 Bitmap
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取封面失敗: {ex.Message}");
            }

            // 3. 如果前兩者都失敗，回傳一個全黑的預設圖片 (防呆機制)
            Bitmap defaultBmp = new Bitmap(180, 240);
            using (Graphics g = Graphics.FromImage(defaultBmp))
            {
                g.Clear(Color.DarkGray);
                g.DrawString("No Image", new Font("Arial", 12), Brushes.White, new PointF(50, 100));
            }
            return defaultBmp;
        }
    }
}
