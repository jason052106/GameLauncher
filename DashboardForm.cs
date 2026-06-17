using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace GameLauncher
{
    public partial class DashboardForm : Form
    {
        private DatabaseManager dbManager;
        public DashboardForm()
        {
            dbManager = new DatabaseManager();
            InitializeDashboardUI();
        }

        private void InitializeDashboardUI()
        {
            this.Text = "個人遊戲數據總覽";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(35, 35, 35);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // --- 區塊 1：總遊玩時數 ---
            int totalSeconds = dbManager.GetAllGamesTotalPlayTime();
            Label lblTotalTitle = new Label() { Text = "生涯總遊玩時數", ForeColor = Color.LightGray, Font = new Font("微軟正黑體", 14, FontStyle.Bold), Location = new Point(30, 30), AutoSize = true };
            Label lblTotalTime = new Label() { Text = FormatPlayTime(totalSeconds), ForeColor = Color.Cyan, Font = new Font("微軟正黑體", 24, FontStyle.Bold), Location = new Point(30, 60), AutoSize = true };

            // --- 區塊 2：Top 3 最常玩遊戲 ---
            Label lblTopTitle = new Label() { Text = "最常玩遊戲 Top 3", ForeColor = Color.LightGray, Font = new Font("微軟正黑體", 14, FontStyle.Bold), Location = new Point(30, 130), AutoSize = true };

            Panel pnlTopGames = new Panel() { Location = new Point(30, 160), Size = new Size(300, 150), BackColor = Color.FromArgb(45, 45, 45) };
            var topGames = dbManager.GetTop3Games();
            int yPos = 10;
            int rank = 1;
            foreach (var game in topGames)
            {
                Label lblRank = new Label() { Text = $"#{rank} {game.Key}", ForeColor = Color.White, Font = new Font("微軟正黑體", 11), Location = new Point(10, yPos), AutoSize = true };
                Label lblTime = new Label() { Text = FormatPlayTime(game.Value), ForeColor = Color.YellowGreen, Font = new Font("微軟正黑體", 10), Location = new Point(180, yPos), AutoSize = true };
                pnlTopGames.Controls.Add(lblRank);
                pnlTopGames.Controls.Add(lblTime);
                yPos += 40;
                rank++;
            }

            // --- 區塊 3：繪製過去 7 天長條圖 ---
            Label lblChartTitle = new Label() { Text = "過去 7 天活躍度 (分鐘)", ForeColor = Color.LightGray, Font = new Font("微軟正黑體", 14, FontStyle.Bold), Location = new Point(380, 30), AutoSize = true };

            Chart playChart = new Chart();
            playChart.Location = new Point(380, 70);
            playChart.Size = new Size(380, 450);
            playChart.BackColor = Color.FromArgb(45, 45, 45);

            ChartArea chartArea = new ChartArea();
            chartArea.BackColor = Color.FromArgb(45, 45, 45);
            chartArea.AxisX.LabelStyle.ForeColor = Color.White;
            chartArea.AxisX.MajorGrid.LineColor = Color.DimGray;
            chartArea.AxisY.LabelStyle.ForeColor = Color.White;
            chartArea.AxisY.MajorGrid.LineColor = Color.DimGray;
            playChart.ChartAreas.Add(chartArea);

            Series series = new Series("遊玩分鐘數");
            series.ChartType = SeriesChartType.Column;
            series.Color = Color.Teal; // 長條圖顏色
            series.IsValueShownAsLabel = true; // 在柱子上方顯示數字
            series.LabelForeColor = Color.White;
            playChart.Series.Add(series);

            // 將資料綁定到圖表上
            var dailyData = dbManager.GetLast7DaysPlayTime();
            foreach (var data in dailyData)
            {
                // 將秒數轉換為分鐘數顯示在圖表上，比較直覺
                series.Points.AddXY(data.Key, data.Value / 60);
            }

            // 加入所有控制項
            this.Controls.Add(lblTotalTitle);
            this.Controls.Add(lblTotalTime);
            this.Controls.Add(lblTopTitle);
            this.Controls.Add(pnlTopGames);
            this.Controls.Add(lblChartTitle);
            this.Controls.Add(playChart);
        }

        // 時間格式化工具 (從 Form1 借用過來的邏輯)
        private string FormatPlayTime(int totalSeconds)
        {
            if (totalSeconds == 0) return "未遊玩";
            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
            if (t.TotalHours >= 1) return $"{(int)t.TotalHours} 小時 {t.Minutes} 分";
            if (t.TotalMinutes >= 1) return $"{t.Minutes} 分 {t.Seconds} 秒";
            return $"{t.Seconds} 秒";
        }
    }
}
