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
using System.Linq;

namespace GameLauncher
{
    public partial class GameLauncher : Form
    {
        private DatabaseManager dbManager;
        private FlowLayoutPanel flowLayoutPanel;
        private Button btnAddGame;
        private ComboBox cmbCategoryFilter;

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
            btnAddGame.Font = new Font("微軟正黑體", 10, FontStyle.Bold);
            btnAddGame.Size = new Size(150, 40);
            btnAddGame.Location = new Point(20, 20);
            btnAddGame.BackColor = Color.Teal;
            btnAddGame.ForeColor = Color.White;
            btnAddGame.FlatStyle = FlatStyle.Flat;
            btnAddGame.Click += BtnAddGame_Click;
            this.Controls.Add(btnAddGame);

            Label lblFilter = new Label();
            lblFilter.Text = "分類篩選:";
            lblFilter.ForeColor = Color.White;
            lblFilter.Location = new Point(190, 30);
            lblFilter.AutoSize = true;
            lblFilter.Font = new Font("微軟正黑體", 10, FontStyle.Bold);

            cmbCategoryFilter = new ComboBox();
            cmbCategoryFilter.Location = new Point(270, 27);
            cmbCategoryFilter.Size = new Size(150, 25);
            cmbCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList; // 限制只能用選的，不能手打
            cmbCategoryFilter.SelectedIndexChanged += CmbCategoryFilter_SelectedIndexChanged; // 綁定切換事件

            Button btnDashboard = new Button();
            btnDashboard.Text = "📊 數據儀表板";
            btnDashboard.Font = new Font("微軟正黑體", 10, FontStyle.Bold);
            btnDashboard.Size = new Size(130, 40);
            btnDashboard.Location = new Point(450, 20); // 放在下拉選單右邊
            btnDashboard.BackColor = Color.Indigo;
            btnDashboard.ForeColor = Color.White;
            btnDashboard.FlatStyle = FlatStyle.Flat;

            // 綁定點擊事件，開啟 Dashboard 視窗
            btnDashboard.Click += (s, e) =>
            {
                using (DashboardForm dashboard = new DashboardForm())
                {
                    dashboard.ShowDialog();
                }
            };

            this.Controls.Add(btnDashboard);

            this.Controls.Add(lblFilter);
            this.Controls.Add(cmbCategoryFilter);

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
                    LoadGamesToUI(); 
                }
            }
        }

        // 讀取資料庫並動態生成遊戲卡片
        private void LoadGamesToUI()
        {
            flowLayoutPanel.Controls.Clear();
            RefreshCategoryFilter();
            List<Game> games = dbManager.GetAllGames();

            string selectedFilter = cmbCategoryFilter.SelectedItem?.ToString() ?? "全部";
            List<Game> displayGames = (selectedFilter == "全部")
                              ? games
                              : games.Where(g => g.Category == selectedFilter).ToList();

            foreach (var game in games)
            {
                Panel card = new Panel();
                card.Size = new Size(200, 410); 
                card.BackColor = Color.FromArgb(50, 50, 50);
                card.Margin = new Padding(10);

                // 1. 遊戲封面
                PictureBox picCover = new PictureBox();
                picCover.Size = new Size(180, 210); // 縮小一點圖片，留空間給文字
                picCover.Location = new Point(10, 10);
                picCover.SizeMode = PictureBoxSizeMode.Zoom;
                picCover.Image = GetGameCover(game);
                picCover.Cursor = Cursors.Hand;
                picCover.Tag = game;
                picCover.Click += BtnPlay_Click;

                // 2. 遊戲名稱
                Label lblName = new Label();
                lblName.Text = game.Name;
                lblName.ForeColor = Color.White;
                lblName.AutoSize = false;
                lblName.Size = new Size(180, 25);
                lblName.Location = new Point(10, 230);
                lblName.Font = new Font("微軟正黑體", 11, FontStyle.Bold);
                lblName.TextAlign = ContentAlignment.MiddleCenter;

                // 3. 總遊玩時間標籤 (新功能)
                int totalSeconds = dbManager.GetTotalPlayTimeSeconds(game.Id);
                Label lblTime = new Label();
                lblTime.Text = $"總時數: {FormatPlayTime(totalSeconds)}";
                lblTime.ForeColor = Color.YellowGreen; // 使用明顯的顏色提示
                lblTime.AutoSize = false;
                lblTime.Size = new Size(180, 20);
                lblTime.Location = new Point(10, 260);
                lblTime.Font = new Font("微軟正黑體", 9, FontStyle.Regular);
                lblTime.TextAlign = ContentAlignment.MiddleCenter;

                // 4. 啟動按鈕
                Button btnPlay = new Button();
                btnPlay.Text = "啟動遊戲";
                btnPlay.Font = new Font("微軟正黑體", 9, FontStyle.Regular);
                btnPlay.Size = new Size(180, 30);
                btnPlay.Location = new Point(10, 290);
                btnPlay.BackColor = Color.DarkOliveGreen;
                btnPlay.ForeColor = Color.White;
                btnPlay.FlatStyle = FlatStyle.Flat;
                btnPlay.Tag = game;
                btnPlay.Click += BtnPlay_Click;

                // 5. 詳細統計按鈕 (新功能)
                Button btnDetails = new Button();
                btnDetails.Text = "詳細紀錄";
                btnDetails.Font = new Font("微軟正黑體", 9, FontStyle.Regular);
                btnDetails.Size = new Size(180, 25);
                btnDetails.Location = new Point(10, 330);
                btnDetails.BackColor = Color.FromArgb(70, 70, 70);
                btnDetails.ForeColor = Color.LightGray;
                btnDetails.FlatStyle = FlatStyle.Flat;
                btnDetails.Tag = game;
                btnDetails.Click += BtnDetails_Click; // 綁定新事件

                Button btnEdit = new Button();
                btnEdit.Text = "編輯設定";
                btnEdit.Font = new Font("微軟正黑體", 9, FontStyle.Regular);
                btnEdit.Size = new Size(180, 25);
                btnEdit.Location = new Point(10, 370); // 調整一下卡片高度與位置
                btnEdit.BackColor = Color.FromArgb(60, 60, 60);
                btnEdit.ForeColor = Color.White;
                btnEdit.FlatStyle = FlatStyle.Flat;
                btnEdit.Tag = game;

                btnEdit.Click += (s, ev) =>
                {
                    Button btn = s as Button;
                    Game selectedGame = btn.Tag as Game;

                    // 開啟編輯視窗
                    using (EditGameForm editForm = new EditGameForm(selectedGame))
                    {
                        // 如果使用者在編輯視窗按下了「儲存變更」(DialogResult.OK)
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            // 將更新後的資料寫回資料庫
                            dbManager.UpdateGameInfo(editForm.CurrentGame);

                            // 重新載入主畫面，讓新的名字和圖片顯示出來！
                            LoadGamesToUI();
                        }
                    }
                };



                // 依序加入元件
                card.Controls.Add(picCover);
                card.Controls.Add(lblName);
                card.Controls.Add(lblTime);
                card.Controls.Add(btnPlay);
                card.Controls.Add(btnDetails);
                card.Controls.Add(btnEdit);

                flowLayoutPanel.Controls.Add(card);
            }
        }

        // 啟動遊戲與監聽邏輯
        private void BtnPlay_Click(object sender, EventArgs e)
        {
            // 1. 將 sender 轉型為通用的 Control (無論是 Button 還是 PictureBox 都能接住)
            Control clickedControl = sender as Control;
            if (clickedControl == null) return;

            // 2. 從 Tag 中取出 Game 物件
            Game game = clickedControl.Tag as Game;
            if (game == null) return;

            // 3. 防呆檢查
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
                process.EnableRaisingEvents = true;

                process.Exited += (s, args) => Process_Exited(game.Id);

                process.Start();
                activeGames.Add(game.Id, DateTime.Now);

                // 4. UI 狀態更新邏輯
                Button btnToUpdate = null;
                if (clickedControl is Button)
                {
                    btnToUpdate = (Button)clickedControl; // 點擊的就是按鈕
                }
                else if (clickedControl is PictureBox)
                {
                    // 如果點擊的是圖片，從圖片的父容器 (Panel) 裡面找出按鈕
                    foreach (Control c in clickedControl.Parent.Controls)
                    {
                        if (c is Button)
                        {
                            btnToUpdate = (Button)c;
                            break;
                        }
                    }
                }

                // 確實更新按鈕外觀
                if (btnToUpdate != null)
                {
                    btnToUpdate.Text = "執行中...";
                    btnToUpdate.BackColor = Color.Gray;
                    btnToUpdate.Enabled = false;
                }
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

        private string FormatPlayTime(int totalSeconds)
        {
            if (totalSeconds == 0) return "未遊玩";

            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);

            if (t.TotalHours >= 1)
            {
                return $"{(int)t.TotalHours} 小時 {t.Minutes} 分";
            }
            if (t.TotalMinutes >= 1)
            {
                return $"{t.Minutes} 分 {t.Seconds} 秒";
            }
            return $"{t.Seconds} 秒";
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Game game = btn.Tag as Game;
            if (game == null) return;

            // 從資料庫撈取歷史紀錄 DataTable
            DataTable historyData = dbManager.GetPlayHistory(game.Id);

            // 動態建立一個跳出視窗 (Form)
            Form detailsForm = new Form();
            detailsForm.Text = $"{game.Name} - 歷史遊玩明細";
            detailsForm.Size = new Size(450, 400);
            detailsForm.StartPosition = FormStartPosition.CenterParent;
            detailsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            detailsForm.MaximizeBox = false;
            detailsForm.MinimizeBox = false;
            detailsForm.BackColor = Color.FromArgb(35, 35, 35);

            // 建立表格控制項 DataGridView
            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.FromArgb(40, 40, 40);
            dgv.ForeColor = Color.Black; // 讓內文清晰
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false; // 唯讀設定
            dgv.ReadOnly = true;

            // 綁定資料來源
            dgv.DataSource = historyData;

            // 將表格加進彈出視窗並顯示
            detailsForm.Controls.Add(dgv);
            detailsForm.ShowDialog(); // 使用 ShowDialog 確保使用者關閉此視窗前無法操作主視窗
        }

        private void RefreshCategoryFilter()
        {
            // 暫時解除綁定事件，避免更新選單時觸發讀取
            cmbCategoryFilter.SelectedIndexChanged -= CmbCategoryFilter_SelectedIndexChanged;

            string currentSelection = cmbCategoryFilter.SelectedItem?.ToString(); // 記住目前選的分類

            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add("全部"); // 預設選項

            List<string> dbCategories = dbManager.GetAllCategories();
            foreach (string cat in dbCategories)
            {
                cmbCategoryFilter.Items.Add(cat);
            }

            // 試著回復原本的選擇，如果沒有就選 "全部"
            if (currentSelection != null && cmbCategoryFilter.Items.Contains(currentSelection))
                cmbCategoryFilter.SelectedItem = currentSelection;
            else
                cmbCategoryFilter.SelectedIndex = 0;

            // 重新綁定事件
            cmbCategoryFilter.SelectedIndexChanged += CmbCategoryFilter_SelectedIndexChanged;
        }

        private void CmbCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadGamesToUI();
        }
    }
}
