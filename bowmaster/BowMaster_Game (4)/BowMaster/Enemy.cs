using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BowMaster
{
    enum EnemyType { Blob, Runner, Tank, Boss }
    class Enemy
    {
        private static readonly Random rand = new Random();
        public float X { get; set; }
        public float Y { get; set; }
        public int Width  { get; private set; }
        public int Height { get; private set; }
        private float speed;
        private int   maxHealth;
        private int   health;
        private Color bodyColor;
        private EnemyType type;
        public int ScoreValue { get; private set; }
        public bool IsOffScreen { get; private set; }
        public bool IsDead { get { return health <= 0; } }
        private int flashTimer;
        private float bobOffset;
        private float bobTime;

        public RectangleF Bounds { get { return new RectangleF(X, Y, Width, Height); } }
        public static Enemy Create(EnemyType t, float x, float y, float speedMultiplier)
        {
            return new Enemy(t, x, y, speedMultiplier);
        }
        private Enemy(EnemyType t, float x, float y, float speedMultiplier)
        {
            type = t;
            X    = x;
            Y    = y;

            switch (t)
            {
                case EnemyType.Blob:
                    Width  = 36; Height  = 36;
                    maxHealth = 1;  speed = 1.4f * speedMultiplier;
                    bodyColor = Color.FromArgb(60, 180, 240);
                    ScoreValue = 10;
                    break;

                case EnemyType.Runner:
                    Width  = 30; Height  = 40;
                    maxHealth = 1;  speed = 2.8f * speedMultiplier;
                    bodyColor = Color.FromArgb(220, 80, 60);
                    ScoreValue = 20;
                    break;

                case EnemyType.Tank:
                    Width  = 52; Height  = 52;
                    maxHealth = 4;  speed = 0.9f * speedMultiplier;
                    bodyColor = Color.FromArgb(120, 60, 200);
                    ScoreValue = 50;
                    break;

                case EnemyType.Boss:
                    Width  = 80; Height  = 80;
                    maxHealth = 12; speed = 0.7f * speedMultiplier;
                    bodyColor = Color.FromArgb(200, 30, 30);
                    ScoreValue = 200;
                    break;
            }
            health = maxHealth;
        }
        public bool Hit()
        {
            health--;
            flashTimer = 8;
            return health <= 0;
        }
        public void Update(int screenWidth)
        {
            X -= speed;
            Y -= speed * 0.25f;   

            bobTime += 0.1f;
            bobOffset = (float)Math.Sin(bobTime) * 3f;

            if (flashTimer > 0) flashTimer--;

            if (X + Width < 0)
                IsOffScreen = true;

            if (Y < -100)
                IsOffScreen = true;
        }
        public void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float drawY = Y + bobOffset;
            Color col   = (flashTimer > 0) ? Color.White : bodyColor;

            switch (type)
            {
                case EnemyType.Blob:   DrawBlob(g, col, drawY);   break;
                case EnemyType.Runner: DrawRunner(g, col, drawY); break;
                case EnemyType.Tank:   DrawTank(g, col, drawY);   break;
                case EnemyType.Boss:   DrawBoss(g, col, drawY);   break;
            }
            if (maxHealth > 1)
                DrawHealthBar(g, drawY);
        }

        private void DrawBlob(Graphics g, Color c, float drawY)
        {
            g.FillEllipse(new SolidBrush(c), X, drawY, Width, Height);
            g.DrawEllipse(new Pen(Color.FromArgb(80, c.R / 2, c.G / 2, c.B / 2), 2),
                X, drawY, Width, Height);
            DrawFace(g, drawY, 10, 10, 5);
        }

        private void DrawRunner(Graphics g, Color c, float drawY)
        {
            float cx = X + Width / 2f;
            float legPhase = bobTime * 5f;
            g.DrawLine(new Pen(Color.FromArgb(180, 60, 40), 4),
                cx - 5, drawY + Height * 0.65f,
                cx - 8 + (float)Math.Sin(legPhase) * 6, drawY + Height);
            g.DrawLine(new Pen(Color.FromArgb(180, 60, 40), 4),
                cx + 5, drawY + Height * 0.65f,
                cx + 8 - (float)Math.Sin(legPhase) * 6, drawY + Height);
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddEllipse(X + 2, drawY, Width - 4, Height * 0.75f);
                g.FillPath(new SolidBrush(c), gp);
                g.DrawPath(new Pen(Color.FromArgb(160, 60, 30), 2), gp);
            }
            DrawFace(g, drawY, 8, 8, 4);
        }

        private void DrawTank(Graphics g, Color c, float drawY)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(80, 50, 160)),
                X + 4, drawY + 8, Width - 8, Height - 8);
            g.FillEllipse(new SolidBrush(c), X, drawY, Width, Height);
            using (Pen plate = new Pen(Color.FromArgb(100, 40, 180), 3))
            {
                g.DrawLine(plate, X + 10, drawY + 10, X + Width - 10, drawY + 10);
                g.DrawLine(plate, X + 10, drawY + Height - 10, X + Width - 10, drawY + Height - 10);
            }
            g.DrawEllipse(new Pen(Color.FromArgb(60, 30, 140), 3), X, drawY, Width, Height);
            DrawFace(g, drawY, 14, 14, 6);
        }

        private void DrawBoss(Graphics g, Color c, float drawY)
        {
            float cx = X + Width / 2f;
            float cy = drawY + Height / 2f;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * (float)Math.PI * 2 / 8 + bobTime * 0.5f;
                float sx    = cx + (float)Math.Cos(angle) * (Width / 2f + 12);
                float sy    = cy + (float)Math.Sin(angle) * (Height / 2f + 12);
                g.FillEllipse(new SolidBrush(Color.FromArgb(255, 200, 20)), sx - 6, sy - 6, 12, 12);
            }
            g.FillEllipse(new SolidBrush(c), X, drawY, Width, Height);
            using (Pen glow = new Pen(Color.FromArgb(200, 255, 80, 80), 4))
                g.DrawEllipse(glow, X - 3, drawY - 3, Width + 6, Height + 6);
            g.FillEllipse(Brushes.Yellow, cx - 20, cy - 12, 16, 16);
            g.FillEllipse(Brushes.Yellow, cx + 4,  cy - 12, 16, 16);
            g.FillEllipse(Brushes.Black,  cx - 17, cy - 9,  10, 10);
            g.FillEllipse(Brushes.Black,  cx + 7,  cy - 9,  10, 10);
            g.DrawArc(new Pen(Color.Black, 3), cx - 16, cy + 8, 32, 14, 10, 160);
        }

        private void DrawFace(Graphics g, float drawY, int eyeOffX, int eyeOffY, int eyeSize)
        {
            float cx = X + Width / 2f;
            float cy = drawY + Height * 0.35f;
            g.FillEllipse(Brushes.White,  cx - eyeOffX, cy, eyeSize, eyeSize);
            g.FillEllipse(Brushes.White,  cx + eyeOffX - eyeSize, cy, eyeSize, eyeSize);
            g.FillEllipse(Brushes.Black,  cx - eyeOffX + 1, cy + 1, eyeSize - 2, eyeSize - 2);
            g.FillEllipse(Brushes.Black,  cx + eyeOffX - eyeSize + 1, cy + 1, eyeSize - 2, eyeSize - 2);
        }

        private void DrawHealthBar(Graphics g, float drawY)
        {
            int bw = Width;
            int bh = 6;
            float by = drawY - 12;
            g.FillRectangle(Brushes.DarkRed, X, by, bw, bh);
            float fillW = bw * ((float)health / maxHealth);
            g.FillRectangle(Brushes.LimeGreen, X, by, fillW, bh);
            g.DrawRectangle(Pens.Black, X, by, bw, bh);
        }
    }
}
