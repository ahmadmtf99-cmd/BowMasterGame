using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BowMaster
{
    public class MenuForm : Form
    {
        private Button playBtn;
        private Button exitBtn;
        private Timer  animTimer;
        private int    frame;
        private static readonly Color SkyTop    = Color.FromArgb(30, 60, 120);
        private static readonly Color SkyBottom = Color.FromArgb(90, 160, 200);
        private static readonly Color GrassCol  = Color.FromArgb(60, 130, 50);

        public MenuForm()
        {
            Text            = "BowMaster – Start Menu";
            ClientSize      = new Size(900, 560);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            DoubleBuffered  = true;
            BackColor       = Color.Black;
            StartPosition   = FormStartPosition.CenterScreen;

            BuildButtons();

            animTimer          = new Timer();
            animTimer.Interval = 16;    // ~60 fps
            animTimer.Tick    += (s, e) => { frame++; Invalidate(); };
            animTimer.Start();
        }
        private void BuildButtons()
        {
            playBtn = new Button
            {
                Text      = "▶  PLAY",
                Font      = new Font("Arial", 16, FontStyle.Bold),
                Size      = new Size(200, 58),
                Location  = new Point(ClientSize.Width / 2 - 210, 468),
                BackColor = Color.FromArgb(50, 180, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            playBtn.FlatAppearance.BorderSize  = 2;
            playBtn.FlatAppearance.BorderColor = Color.LightGreen;
            playBtn.Click += (s, e) => { animTimer.Stop(); DialogResult = DialogResult.OK; Close(); };
            Controls.Add(playBtn);

            exitBtn = new Button
            {
                Text      = "✕  EXIT",
                Font      = new Font("Arial", 16, FontStyle.Bold),
                Size      = new Size(200, 58),
                Location  = new Point(ClientSize.Width / 2 + 10, 468),
                BackColor = Color.FromArgb(180, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            exitBtn.FlatAppearance.BorderSize  = 2;
            exitBtn.FlatAppearance.BorderColor = Color.LightCoral;
            exitBtn.Click += (s, e) => { animTimer.Stop(); DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(exitBtn);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int W = ClientSize.Width, H = ClientSize.Height;
            using (var sky = new LinearGradientBrush(
                    new Point(0,0), new Point(0, H),
                    SkyTop, SkyBottom))
                g.FillRectangle(sky, 0, 0, W, H);
            Random rng = new Random(42);
            for (int s = 0; s < 60; s++)
            {
                int sx  = rng.Next(W);
                int sy  = rng.Next(H / 2);
                int bri = (int)(128 + 80 * Math.Sin(frame * 0.04 + s * 0.7));
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(bri, Color.White)))
                    g.FillEllipse(sb, sx, sy, 2, 2);
            }
            g.FillEllipse(Brushes.Ivory, W - 120, 30, 70, 70);
            g.FillEllipse(new SolidBrush(SkyTop), W - 108, 22, 68, 68);
            g.FillRectangle(new SolidBrush(GrassCol), 0, H - 100, W, 100);
            g.FillRectangle(new SolidBrush(Color.FromArgb(80, 50, 20)), 0, H - 70, W, 70);
            DrawDecoBowman(g, 160, H - 100);
            DrawDecoBlob(g, 680, H - 95, Color.FromArgb(60, 180, 240), "L1");
            DrawDecoBlob(g, 760, H - 100, Color.FromArgb(220, 80, 60),  "L2");
            DrawDecoBlob(g, 840, H - 105, Color.FromArgb(180, 60, 220), "L3");
            int ax = (frame * 3) % (W + 100) - 50;
            DrawArrow(g, ax, H - 140, 1f);
            using (Font title = new Font("Impact", 52, FontStyle.Bold))
            {
                string txt = "BOW MASTER";
                SizeF  sz  = g.MeasureString(txt, title);
                float  tx  = (W - sz.Width) / 2f;
                float  ty  = 26;
                g.DrawString(txt, title, new SolidBrush(Color.FromArgb(120, Color.Black)), tx + 4, ty + 4);
                g.DrawString(txt, title, new SolidBrush(Color.FromArgb(255, 220, 60)), tx, ty);
            }

            using (Font sub = new Font("Arial", 14, FontStyle.Bold | FontStyle.Italic))
                DrawCentered(g, "Shoot, dodge, survive — 3 levels of escalating chaos!", sub,
                    Brushes.LightCyan, 102);
            DrawInstructionsPanel(g, W, H);
        }

        private void DrawInstructionsPanel(Graphics g, int W, int H)
        {
            int px = 60, py = 148, pw = W - 120, ph = 290;
            g.FillRectangle(new SolidBrush(Color.FromArgb(170, Color.Black)), px, py, pw, ph);
            using (Pen border = new Pen(Color.FromArgb(200, Color.Gold), 2))
                g.DrawRectangle(border, px, py, pw, ph);

            using (Font head = new Font("Arial", 13, FontStyle.Bold))
            using (Font body = new Font("Arial", 11))
            {
                DrawCentered(g, "── HOW TO PLAY ──", head, Brushes.Gold, py + 10);

                string[] lines =
                {
                    "⬅ ➡   Move the bowman LEFT and RIGHT",
                    "⬆ ⬇   Aim the bow UP and DOWN",
                    "SPACE   Shoot an arrow",
                    "P       Pause the game   |   R = Restart (when game over)",
                    "",
                    "★  Hit enemies to score points.  Arrows have limited range — time your shots!",
                    "★  Level 1: 3 basic blobs.  Level 2: faster + more enemies.  Level 3: boss mode!",
                    "★  Each level is cleared when all waves are defeated.  Don't let them reach you!",
                    "★  You have 3 lives.  A collision with an enemy costs 1 life.",
                };

                float ly = py + 42;
                foreach (string line in lines)
                {
                    g.DrawString(line, body, line.StartsWith("★") ? Brushes.LightYellow : Brushes.White,
                        px + 18, ly);
                    ly += 22;
                }
            }
        }
        private void DrawDecoBowman(Graphics g, int cx, int groundY)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 130, 50)), cx - 10, groundY - 50, 20, 30);
            g.FillEllipse(new SolidBrush(Color.FromArgb(220, 180, 120)), cx - 12, groundY - 82, 24, 24);
            g.FillRectangle(Brushes.DarkGreen, cx - 10, groundY - 96, 20, 10);
            g.FillRectangle(Brushes.DarkGreen, cx - 6, groundY - 106, 12, 12);
            g.DrawLine(new Pen(Color.FromArgb(200, 160, 100), 3), cx + 10, groundY - 60, cx + 36, groundY - 60);
            g.DrawArc(new Pen(Color.SaddleBrown, 3), cx + 30, groundY - 76, 20, 32, -80, 160);
            g.DrawLine(new Pen(Color.Ivory, 1), cx + 30, groundY - 76, cx + 30, groundY - 44);
            g.DrawLine(new Pen(Color.FromArgb(40, 90, 40), 5), cx - 5, groundY - 20, cx - 8, groundY);
            g.DrawLine(new Pen(Color.FromArgb(40, 90, 40), 5), cx + 5, groundY - 20, cx + 8, groundY);
        }

        private void DrawDecoBlob(Graphics g, int cx, int groundY, Color col, string label)
        {
            int size = label == "L3" ? 52 : label == "L2" ? 44 : 36;
            g.FillEllipse(new SolidBrush(col), cx - size / 2, groundY - size, size, size);
            g.FillEllipse(Brushes.White, cx - size / 4, groundY - size + 8, 8, 8);
            g.FillEllipse(Brushes.White, cx + size / 4 - 8, groundY - size + 8, 8, 8);
            g.FillEllipse(Brushes.Black, cx - size / 4 + 2, groundY - size + 10, 4, 4);
            g.FillEllipse(Brushes.Black, cx + size / 4 - 6, groundY - size + 10, 4, 4);
            using (Font f = new Font("Arial", 8, FontStyle.Bold))
                g.DrawString(label, f, Brushes.White, cx - 7, groundY - size / 2 - 5);
        }

        private void DrawArrow(Graphics g, int x, int y, float scale)
        {
            using (Pen p = new Pen(Color.Ivory, 2))
            {
                g.DrawLine(p, x, y, x + (int)(30 * scale), y);
                Point[] head =
                {
                    new Point(x + (int)(30 * scale), y),
                    new Point(x + (int)(22 * scale), y - 4),
                    new Point(x + (int)(22 * scale), y + 4)
                };
                g.FillPolygon(Brushes.Ivory, head);
                g.DrawLine(p, x, y, x - 4, y - 5);
                g.DrawLine(p, x, y, x - 4, y + 5);
            }
        }

        private void DrawCentered(Graphics g, string text, Font font, Brush brush, float y)
        {
            SizeF sz = g.MeasureString(text, font);
            g.DrawString(text, font, brush, (ClientSize.Width - sz.Width) / 2f, y);
        }
    }
}
