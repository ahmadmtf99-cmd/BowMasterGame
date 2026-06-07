using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BowMaster
{
    
    class Arrow
    {
       
        public float X { get; private set; }
        public float Y { get; private set; }

        
        private float vx;
        private float vy;

       
        private float distanceTravelled;
        private float maxRange;

        
        public RectangleF Bounds { get { return new RectangleF(X - 4, Y - 4, 20, 8); } }

        
        public bool IsDead { get; private set; }

        
        private float angle;

        public Arrow(float x, float y, float vx, float vy, float maxRange)
        {
            X               = x;
            Y               = y;
            this.vx         = vx;
            this.vy         = vy;
            this.maxRange   = maxRange;
            distanceTravelled = 0;
            angle           = (float)Math.Atan2(vy, vx) * 180f / (float)Math.PI;
        }

        public void Update()
        {
            X += vx;
            Y += vy;
            distanceTravelled += (float)Math.Sqrt(vx * vx + vy * vy);
            if (distanceTravelled > maxRange)
                IsDead = true;
        }

        public void Kill() { IsDead = true; }

        public void Draw(Graphics g)
        {
            if (IsDead) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            
            var state = g.Save();
            g.TranslateTransform(X, Y);
            g.RotateTransform(angle);

            
            using (Pen shaft = new Pen(Color.FromArgb(180, 130, 60), 2))
                g.DrawLine(shaft, -24, 0, 6, 0);

            
            Point[] head = { new Point(6, 0), new Point(-2, -4), new Point(-2, 4) };
            g.FillPolygon(Brushes.LightGray, head);
            g.DrawLine(new Pen(Color.Crimson, 1), -24, 0, -20, -5);
            g.DrawLine(new Pen(Color.Crimson, 1), -24, 0, -20,  5);
            g.DrawLine(new Pen(Color.Orange,  1), -22, 0, -18, -4);
            g.DrawLine(new Pen(Color.Orange,  1), -22, 0, -18,  4);
            g.Restore(state);
        }
    }
}
