//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Windows.Forms;

//namespace BowMaster
//{

//    class Bowman
//    {
//        public float X { get; set; }
//        public float Y { get; set; }

//        private float minX;
//        private float maxX;

//        public int Lives { get; private set; }

//        public bool IsInvincible { get { return invincibleTimer > 0; } }
//        private int invincibleTimer;
//        private float aimAngle;
//        private const float AimMin = -75f;
//        private const float AimMax = 0f;
//        private const float AimStep = 3f;
//        private const float MoveSpeed = 4f;
//        private int shootCooldown;
//        private int maxCooldown;
//        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
//        private bool shootRequested;
//        public const int W = 44;
//        public const int H = 72;

//        public RectangleF Bounds { get { return new RectangleF(X - W / 2, Y - H, W, H); } }
//        public Bowman(float startX, float groundY, float minX, float maxX, int shootCooldown)
//        {
//            X = startX;
//            Y = groundY;
//            this.minX = minX;
//            this.maxX = maxX;
//            this.maxCooldown = shootCooldown;
//            Lives = 3;
//            aimAngle = -30f;
//        }
//        public void KeyDown(Keys k)
//        {
//            pressedKeys.Add(k);
//            if (k == Keys.Space) shootRequested = true;
//        }
//        public void KeyUp(Keys k) { pressedKeys.Remove(k); }

//        public Arrow Update()
//        {
//            if (pressedKeys.Contains(Keys.Left) || pressedKeys.Contains(Keys.A))
//                X -= MoveSpeed;
//            if (pressedKeys.Contains(Keys.Right) || pressedKeys.Contains(Keys.D))
//                X += MoveSpeed;

//            X = Math.Max(minX, Math.Min(maxX, X));
//            if (pressedKeys.Contains(Keys.Up) || pressedKeys.Contains(Keys.W))
//                aimAngle -= AimStep;
//            if (pressedKeys.Contains(Keys.Down) || pressedKeys.Contains(Keys.S))
//                aimAngle += AimStep;

//            aimAngle = Math.Max(AimMin, Math.Min(AimMax, aimAngle));
//            if (shootCooldown > 0) shootCooldown--;
//            if (invincibleTimer > 0) invincibleTimer--;
//            Arrow arrow = null;
//            if (shootRequested && shootCooldown == 0)
//            {
//                arrow = CreateArrow();
//                shootCooldown = maxCooldown;
//            }
//            shootRequested = false;

//            return arrow;
//        }

//        private Arrow CreateArrow()
//        {
//            float bowTipX = X + W / 2f + 10;
//            float bowTipY = Y - H * 0.55f;

//            float rad = aimAngle * (float)Math.PI / 180f;
//            float speed = 12f;
//            float vx = (float)Math.Cos(rad) * speed;
//            float vy = (float)Math.Sin(rad) * speed;
//            return new Arrow(bowTipX, bowTipY, vx, vy, 650f);
//        }
//        public void TakeHit()
//        {
//            if (IsInvincible) return;
//            Lives--;
//            invincibleTimer = 120; // ~2 s
//        }
//        public void Draw(Graphics g)
//        {
//            if (IsInvincible && (invincibleTimer / 5) % 2 == 0) return;

//            g.SmoothingMode = SmoothingMode.AntiAlias;

//            float cx = X;
//            float gy = Y;
//            float legAnim = (float)Math.Sin(Environment.TickCount * 0.008) * 6f;
//            g.DrawLine(new Pen(Color.FromArgb(50, 100, 40), 6),
//                cx - 5, gy - 20, cx - 8 + legAnim, gy);
//            g.DrawLine(new Pen(Color.FromArgb(50, 100, 40), 6),
//                cx + 5, gy - 20, cx + 8 - legAnim, gy);

//            using (SolidBrush body = new SolidBrush(Color.FromArgb(60, 140, 55)))
//                g.FillRectangle(body, cx - 12, gy - 52, 24, 32);

//            g.FillEllipse(new SolidBrush(Color.FromArgb(220, 180, 120)),
//                cx - 14, gy - 80, 28, 28);

//            g.FillRectangle(Brushes.DarkGreen, cx - 13, gy - 88, 26, 10);
//            g.FillRectangle(Brushes.DarkGreen, cx - 8, gy - 98, 16, 12);

//            g.FillEllipse(Brushes.White, cx + 4, gy - 74, 7, 7);
//            g.FillEllipse(Brushes.Black, cx + 6, gy - 72, 4, 4);
//            float rad = aimAngle * (float)Math.PI / 180f;
//            float armX = cx + 14;
//            float armY = gy - 52;
//            float bowEndX = armX + (float)Math.Cos(rad) * 28;
//            float bowEndY = armY + (float)Math.Sin(rad) * 28;

//            g.DrawLine(new Pen(Color.FromArgb(50, 100, 40), 5), cx, gy - 48, armX, armY);
//            g.DrawLine(new Pen(Color.FromArgb(50, 100, 40), 4), armX, armY, bowEndX, bowEndY);

//            DrawBow(g, bowEndX, bowEndY, aimAngle);

//            if (shootCooldown == 0)
//            {
//                float arrowX = bowEndX - (float)Math.Cos(rad) * 4;
//                float arrowY = bowEndY - (float)Math.Sin(rad) * 4;
//                using (Pen arrowP = new Pen(Color.FromArgb(180, 130, 60), 2))
//                    g.DrawLine(arrowP, arrowX, arrowY,
//                        arrowX + (float)Math.Cos(rad) * 22,
//                        arrowY + (float)Math.Sin(rad) * 22);
//            }
//        }

//        private void DrawBow(Graphics g, float tipX, float tipY, float angleDeg)
//        {
//            float rad = angleDeg * (float)Math.PI / 180f;
//            float perpX = -(float)Math.Sin(rad);
//            float perpY = (float)Math.Cos(rad);

//            float limbLen = 20f;
//            float top1X = tipX + perpX * limbLen;
//            float top1Y = tipY + perpY * limbLen;
//            float bot1X = tipX - perpX * limbLen;
//            float bot1Y = tipY - perpY * limbLen;

//            g.DrawLine(new Pen(Color.Ivory, 1), top1X, top1Y, bot1X, bot1Y);
//            float bulge = 14f;
//            float midX = tipX + (float)Math.Cos(rad) * bulge;
//            float midY = tipY + (float)Math.Sin(rad) * bulge;
//            g.DrawLine(new Pen(Color.SaddleBrown, 3), top1X, top1Y, midX, midY);
//            g.DrawLine(new Pen(Color.SaddleBrown, 3), bot1X, bot1Y, midX, midY);
//        }

//        public float AimAngle { get { return aimAngle; } }
//    }
//}

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BowMaster
{
    class Bowman
    {
        public float X { get; set; }
        public float Y { get; set; }

        private float minX;
        private float maxX;

        public int Lives { get; private set; }

        public bool IsInvincible { get { return invincibleTimer > 0; } }
        private int invincibleTimer;
        private float aimAngle;
        private const float AimMin = -75f;
        private const float AimMax = 0f;
        private const float AimStep = 3f;
        private const float MoveSpeed = 4f;
        private int shootCooldown;
        private int maxCooldown;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private bool shootRequested;
        private Image bowmanImage;
        private bool imageLoaded;
        
        public const int W = 44;
        public const int H = 72;

        public RectangleF Bounds { get { return new RectangleF(X - W / 2, Y - H, W, H); } }

        public Bowman(float startX, float groundY, float minX, float maxX, int shootCooldown)
        {
            X = startX;
            Y = groundY;
            this.minX = minX;
            this.maxX = maxX;
            this.maxCooldown = shootCooldown;
            Lives = 3;
            aimAngle = -30f;

            string imagePath = "C:/Users/user/Downloads/BowMaster_Game (4)/BowMaster/bin/Assets/bowman.jpg.png";

            if (System.IO.File.Exists(imagePath))
            {
                Bitmap bmp = new Bitmap(imagePath);
                bmp.MakeTransparent(Color.Black); 
                bowmanImage = bmp;
                imageLoaded = true;
            }
        }

        public void KeyDown(Keys k)
        {
            pressedKeys.Add(k);
            if (k == Keys.Space) shootRequested = true;
        }

        public void KeyUp(Keys k) { pressedKeys.Remove(k); }

        public Arrow Update()
        {
            if (pressedKeys.Contains(Keys.Left) || pressedKeys.Contains(Keys.A))
                X -= MoveSpeed;
            if (pressedKeys.Contains(Keys.Right) || pressedKeys.Contains(Keys.D))
                X += MoveSpeed;

            X = Math.Max(minX, Math.Min(maxX, X));

            if (pressedKeys.Contains(Keys.Up) || pressedKeys.Contains(Keys.W))
                aimAngle -= AimStep;
            if (pressedKeys.Contains(Keys.Down) || pressedKeys.Contains(Keys.S))
                aimAngle += AimStep;

            aimAngle = Math.Max(AimMin, Math.Min(AimMax, aimAngle));

            if (shootCooldown > 0) shootCooldown--;
            if (invincibleTimer > 0) invincibleTimer--;

            Arrow arrow = null;
            if (shootRequested && shootCooldown == 0)
            {
                arrow = CreateArrow();
                shootCooldown = maxCooldown;
            }
            shootRequested = false;

            return arrow;
        }

        private Arrow CreateArrow()
        {
            float bowTipX = X + W / 2f + 10;
            float bowTipY = Y - H * 0.55f;

            float rad = aimAngle * (float)Math.PI / 180f;
            float speed = 12f;
            float vx = (float)Math.Cos(rad) * speed;
            float vy = (float)Math.Sin(rad) * speed;
            return new Arrow(bowTipX, bowTipY, vx, vy, 650f);
        }

        public void TakeHit()
        {
            if (IsInvincible) return;
            Lives--;
            invincibleTimer = 120;
        }

        public void Draw(Graphics g)
        {
            if (IsInvincible && (invincibleTimer / 5) % 2 == 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cx = X;
            float gy = Y;
            if (imageLoaded && bowmanImage != null)
            {
                g.DrawImage(bowmanImage,
                    new RectangleF(cx - 40, gy - 130, 120, 130));
            }
            else
            {
                float legAnim = (float)Math.Sin(Environment.TickCount * 0.008) * 6f;
                g.DrawLine(new Pen(Color.FromArgb(50, 100, 40), 6),
                    cx - 5, gy - 20, cx - 8 + legAnim, gy);
                g.DrawLine(new Pen(Color.FromArgb(50, 100, 40), 6),
                    cx + 5, gy - 20, cx + 8 - legAnim, gy);

                using (SolidBrush body = new SolidBrush(Color.FromArgb(60, 140, 55)))
                    g.FillRectangle(body, cx - 12, gy - 52, 24, 32);

                g.FillEllipse(new SolidBrush(Color.FromArgb(220, 180, 120)),
                    cx - 14, gy - 80, 28, 28);
                g.FillRectangle(Brushes.DarkGreen, cx - 13, gy - 88, 26, 10);
                g.FillRectangle(Brushes.DarkGreen, cx - 8, gy - 98, 16, 12);
                g.FillEllipse(Brushes.White, cx + 4, gy - 74, 7, 7);
                g.FillEllipse(Brushes.Black, cx + 6, gy - 72, 4, 4);
            }
            float rad2 = aimAngle * (float)Math.PI / 180f;
            float armX = cx + 14;
            float armY = gy - 52;
            float bowEndX = armX + (float)Math.Cos(rad2) * 28;
            float bowEndY = armY + (float)Math.Sin(rad2) * 28;

            g.DrawLine(new Pen(Color.FromArgb(50, 100, 40), 5), cx, gy - 48, armX, armY);
            g.DrawLine(new Pen(Color.FromArgb(50, 100, 40), 4), armX, armY, bowEndX, bowEndY);
            DrawBow(g, bowEndX, bowEndY, aimAngle);

            if (shootCooldown == 0)
            {
                float arrowX = bowEndX - (float)Math.Cos(rad2) * 4;
                float arrowY = bowEndY - (float)Math.Sin(rad2) * 4;
                using (Pen arrowP = new Pen(Color.FromArgb(180, 130, 60), 2))
                    g.DrawLine(arrowP, arrowX, arrowY,
                        arrowX + (float)Math.Cos(rad2) * 22,
                        arrowY + (float)Math.Sin(rad2) * 22);
            }
        }

        private void DrawBow(Graphics g, float tipX, float tipY, float angleDeg)
        {
            float rad = angleDeg * (float)Math.PI / 180f;
            float perpX = -(float)Math.Sin(rad);
            float perpY = (float)Math.Cos(rad);

            float limbLen = 20f;
            float top1X = tipX + perpX * limbLen;
            float top1Y = tipY + perpY * limbLen;
            float bot1X = tipX - perpX * limbLen;
            float bot1Y = tipY - perpY * limbLen;

            g.DrawLine(new Pen(Color.Ivory, 1), top1X, top1Y, bot1X, bot1Y);
            float bulge = 14f;
            float midX = tipX + (float)Math.Cos(rad) * bulge;
            float midY = tipY + (float)Math.Sin(rad) * bulge;
            g.DrawLine(new Pen(Color.SaddleBrown, 3), top1X, top1Y, midX, midY);
            g.DrawLine(new Pen(Color.SaddleBrown, 3), bot1X, bot1Y, midX, midY);
        }

        public float AimAngle { get { return aimAngle; } }
    }
}
