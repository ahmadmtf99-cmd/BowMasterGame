using System;
using System.Drawing;

namespace BowMaster
{
    class Particle
    {
        private float x, y;
        private float vx, vy;
        private float life;
        private float maxLife;
        private Color color;
        private float size;

        public bool IsDead { get { return life <= 0; } }

        private static Random rand = new Random();

        public Particle(float x, float y, Color color, float speed, float size, int life)
        {
            this.x      = x;
            this.y      = y;
            this.color  = color;
            this.size   = size;
            maxLife     = life;
            this.life   = life;
            float angle = (float)(rand.NextDouble() * Math.PI * 2);
            float spd   = speed * (0.5f + (float)rand.NextDouble() * 0.5f);
            vx = (float)Math.Cos(angle) * spd;
            vy = (float)Math.Sin(angle) * spd;
        }

        public void Update()
        {
            x    += vx;
            y    += vy;
            vy   += 0.12f;
            life -= 1;
        }

        public void Draw(Graphics g)
        {
            if (IsDead) return;
            int alpha = (int)(255f * (life / maxLife));
            using (SolidBrush br = new SolidBrush(Color.FromArgb(alpha, color)))
                g.FillEllipse(br, x - size / 2, y - size / 2, size, size);
        }
    }
}
