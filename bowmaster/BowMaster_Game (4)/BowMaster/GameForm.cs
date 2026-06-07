using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BowMaster
{
    public partial class GameForm : Form
    {
        private GameEngine engine;
        private Timer      gameTimer;
        private Scoreboard scoreboard;

        private Button restartBtn;
        private Button exitBtn;
        public GameForm()
        {
            SetupForm();
            SetupGame();
        }
        private void SetupForm()
        {
            Text            = "BowMaster  |  ← → Move   ↑ ↓ Aim   SPACE Shoot   P Pause";
            ClientSize      = new Size(900, 560);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            DoubleBuffered  = true;
            BackColor       = Color.Black;
            StartPosition   = FormStartPosition.CenterScreen;
        }
        private void SetupGame()
        {
            string scoreFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "bowmaster_scores.txt");

            scoreboard = new Scoreboard(scoreFile);
            engine     = new GameEngine(ClientSize.Width, ClientSize.Height, scoreboard);

            gameTimer          = new Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick    += OnTick;
            gameTimer.Start();

            KeyPreview = true;
            KeyDown   += OnKeyDown;
            KeyUp     += OnKeyUp;

            BuildOverlayButtons();
        }
        private void BuildOverlayButtons()
        {
            restartBtn = new Button
            {
                Text      = "▶  Restart",
                Font      = new Font("Arial", 13, FontStyle.Bold),
                Size      = new Size(160, 50),
                BackColor = Color.FromArgb(40, 175, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible   = false
            };
            restartBtn.FlatAppearance.BorderSize  = 2;
            restartBtn.FlatAppearance.BorderColor = Color.LightGreen;
            restartBtn.Click += (s, e) => { HideButtons(); engine.Restart(); };
            Controls.Add(restartBtn);

            exitBtn = new Button
            {
                Text      = "✕  Exit",
                Font      = new Font("Arial", 13, FontStyle.Bold),
                Size      = new Size(160, 50),
                BackColor = Color.FromArgb(180, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible   = false
            };
            exitBtn.FlatAppearance.BorderSize  = 2;
            exitBtn.FlatAppearance.BorderColor = Color.LightCoral;
            exitBtn.Click += (s, e) => Close();
            Controls.Add(exitBtn);
        }

        private void ShowButtons()
        {
            int cx  = ClientSize.Width / 2;
            int by  = (int)(ClientSize.Height * 0.80f);
            int gap = 20;
            restartBtn.Location = new Point(cx - 160 - gap / 2, by);
            exitBtn.Location    = new Point(cx + gap / 2,        by);
            restartBtn.Visible  = true;
            exitBtn.Visible     = true;
            restartBtn.BringToFront();
            exitBtn.BringToFront();
        }

        private void HideButtons()
        {
            restartBtn.Visible = false;
            exitBtn.Visible    = false;
        }
        private void OnTick(object sender, EventArgs e)
        {
            engine.Update();

            if (engine.IsGameOver && !restartBtn.Visible)
                ShowButtons();
            else if (!engine.IsGameOver && restartBtn.Visible)
                HideButtons();

            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            engine.Draw(e.Graphics);
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            engine.Player.KeyDown(e.KeyCode);

            if (e.KeyCode == Keys.P)
                engine.TogglePause();

            if (e.KeyCode == Keys.R && engine.IsGameOver)
            {
                HideButtons();
                engine.Restart();
            }

            e.Handled = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            engine.Player.KeyUp(e.KeyCode);
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            gameTimer.Stop();
            base.OnFormClosed(e);
        }
    }
}
