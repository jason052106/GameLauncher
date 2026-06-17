using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameLauncher
{
    public partial class EditGameForm : Form
    {
        public Game CurrentGame { get; private set; } // 存放被編輯的遊戲資料
        private string newImagePath = ""; // 暫存使用者新選的圖片路徑

        private TextBox txtName, txtCategory, txtPublisher;
        private PictureBox picPreview;
        public EditGameForm(Game game)
        {
            CurrentGame = game;
            newImagePath = game.CoverImagePath; // 預設為原本的路徑
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "編輯遊戲中介資料";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent; // 顯示在主視窗正中央
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 1. 遊戲名稱
            Label lblName = new Label() { Text = "遊戲名稱:", Location = new Point(20, 20), AutoSize = true };
            txtName = new TextBox() { Text = CurrentGame.Name, Location = new Point(120, 18), Width = 230 };

            // 2. 分類標籤
            Label lblCat = new Label() { Text = "分類 (例:RPG):", Location = new Point(20, 60), AutoSize = true };
            txtCategory = new TextBox() { Text = CurrentGame.Category, Location = new Point(120, 58), Width = 230 };

            // 3. 發行商
            Label lblPub = new Label() { Text = "發行商:", Location = new Point(20, 100), AutoSize = true };
            txtPublisher = new TextBox() { Text = CurrentGame.Publisher, Location = new Point(120, 98), Width = 230 };

            // 4. 封面圖片預覽與上傳按鈕
            Label lblCover = new Label() { Text = "封面圖片:", Location = new Point(20, 140), AutoSize = true };
            picPreview = new PictureBox()
            {
                Location = new Point(120, 140),
                Size = new Size(120, 160),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 嘗試載入現有圖片
            if (!string.IsNullOrEmpty(newImagePath) && File.Exists(newImagePath))
            {
                using (FileStream fs = new FileStream(newImagePath, FileMode.Open, FileAccess.Read))
                {
                    picPreview.Image = Image.FromStream(fs);
                }
            }

            Button btnBrowse = new Button() { Text = "選擇新圖片", Location = new Point(250, 140), Width = 100 };
            btnBrowse.Click += BtnBrowse_Click;

            // 5. 儲存與取消按鈕
            Button btnSave = new Button() { Text = "儲存變更", Location = new Point(120, 350), Width = 100, BackColor = Color.Teal, ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button() { Text = "取消", Location = new Point(230, 350), Width = 100 };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // 將控制項加入視窗
            this.Controls.AddRange(new Control[] { lblName, txtName, lblCat, txtCategory, lblPub, txtPublisher, lblCover, picPreview, btnBrowse, btnSave, btnCancel });
        }

        // 選擇新圖片檔案
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "圖片檔案 (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    newImagePath = ofd.FileName;
                    using (FileStream fs = new FileStream(newImagePath, FileMode.Open, FileAccess.Read))
                    {
                        picPreview.Image = Image.FromStream(fs);
                    }
                }
            }
        }

        // 儲存邏輯
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("遊戲名稱不能為空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 更新物件資料
            CurrentGame.Name = txtName.Text.Trim();
            CurrentGame.Category = txtCategory.Text.Trim();
            CurrentGame.Publisher = txtPublisher.Text.Trim();
            CurrentGame.CoverImagePath = newImagePath;

            // 告訴系統這次操作是「確定」的
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
