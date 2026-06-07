using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BowMaster
{
    enum GameState { Playing, WaveComplete, LevelComplete, GameOver }
    class GameEngine
    {
        private int W, H;
        private int groundY;
        private Bowman         player;
        private List<Arrow>    arrows    = new List<Arrow>();
        private List<Enemy>    enemies   = new List<Enemy>();
        private List<Particle> particles = new List<Particle>();
        private Scoreboard     scoreboard;
        private LevelDefinition[] levels;
        private int  levelIndex;
        private int  waveIndex;     

        private List<WaveEntry> currentWave;
        private int  waveFrame;

        private int  spawnCursor;
        private int       score;
        private GameState state;
        private bool      isPaused;
        private bool      isWin;    
        private int transitionTimer;
        private const int WaveTransitionDuration  =  90;
        private const int LevelTransitionDuration = 180;
        private struct FloatText { public string text; public float x, y; public int timer; public Color col; }
        private List<FloatText> floatTexts = new List<FloatText>();
        private float cloudScroll;

        private Random rand = new Random();
        public int       Score      { get { return score; } }
        public int       Level      { get { return levelIndex + 1; } }
        public int       Wave       { get { return waveIndex + 1; } }
        public int       TotalWaves { get { return levels[levelIndex].Waves.Count; } }
        public bool      IsGameOver { get { return state == GameState.GameOver; } }
        public bool      IsPaused   { get { return isPaused; } }
        public GameState State      { get { return state; } }
        public Bowman    Player     { get { return player; } }
        public GameEngine(int w, int h, Scoreboard sb)
        {
            W          = w;
            H          = h;
            groundY    = H - 60;
            scoreboard = sb;
            levels     = WaveDefinitions.Build();

            StartLevel(0);
        }
        private void StartLevel(int li)
        {
            levelIndex = li;
            waveIndex  = -1;  

            arrows.Clear();
            enemies.Clear();
            particles.Clear();
            floatTexts.Clear();

            LevelDefinition def = levels[levelIndex];
            player = new Bowman(
                startX      : 120,
                groundY     : groundY,
                minX        : 30,
                maxX        : W / 3f,        
                shootCooldown: def.ShootCooldown);

            state      = GameState.Playing;
            isPaused   = false;

            AdvanceWave();
        }

        private void AdvanceWave()
        {
            waveIndex++;
            LevelDefinition def = levels[levelIndex];

            if (waveIndex >= def.Waves.Count)
            {
               
                state           = GameState.LevelComplete;
                transitionTimer = LevelTransitionDuration;
                scoreboard.AddScore("Archer", score, levelIndex + 1);
                return;
            }

            currentWave  = def.Waves[waveIndex];
            waveFrame    = 0;
            spawnCursor  = 0;
            state        = GameState.Playing;
        }
        public void TogglePause() { if (state == GameState.Playing) isPaused = !isPaused; }

        public void Restart()
        {
            score = 0;
            StartLevel(0);
        }
        public void Update()
        {
            if (isPaused) return;

            cloudScroll += 0.4f;
            if (cloudScroll > W + 200) cloudScroll = -200;
            if (state == GameState.WaveComplete)
            {
                transitionTimer--;
                if (transitionTimer <= 0) AdvanceWave();
                return;
            }

            if (state == GameState.LevelComplete)
            {
                transitionTimer--;
                if (transitionTimer <= 0)
                {
                    int next = levelIndex + 1;
                    if (next >= levels.Length)
                    {
                        state = GameState.GameOver;     
                        isWin = true;
                    }
                    else
                    {
                        score += 300;   
                        StartLevel(next);
                    }
                }
                return;
            }

            if (state == GameState.GameOver) return;
            waveFrame++;
            Arrow shot = player.Update();
            if (shot != null) arrows.Add(shot);
            LevelDefinition def = levels[levelIndex];
            while (spawnCursor < currentWave.Count &&
                   waveFrame >= currentWave[spawnCursor].DelayFrames)
            {
                SpawnEnemy(currentWave[spawnCursor].Type, def.SpeedMultiplier);
                spawnCursor++;
            }
            for (int i = 0; i < arrows.Count; i++) arrows[i].Update();
            for (int i = 0; i < enemies.Count; i++) enemies[i].Update(W);
            for (int i = 0; i < particles.Count; i++) particles[i].Update();
            for (int i = floatTexts.Count - 1; i >= 0; i--)
            {
                FloatText t = floatTexts[i];
                t.timer--; t.y -= 0.5f;
                floatTexts[i] = t;
                if (t.timer <= 0) floatTexts.RemoveAt(i);
            }
            CheckArrowEnemyCollisions();
            CheckEnemyPlayerCollisions();
            CleanUp();
            if (spawnCursor >= currentWave.Count && enemies.Count == 0)
            {
                if (waveIndex + 1 < def.Waves.Count)
                {
                    state           = GameState.WaveComplete;
                    transitionTimer = WaveTransitionDuration;
                    AddFloat("Wave " + (waveIndex + 1) + " Cleared!", W / 2f, H / 2f, Color.LimeGreen);
                }
                else
                {
                    AdvanceWave();
                }
            }
        }
        private void SpawnEnemy(EnemyType t, float speedMult)
        {
            float ey = groundY - rand.Next(0, 30) - GetEnemyHeight(t) - 50;
            enemies.Add(Enemy.Create(t, W + 40, ey, speedMult));
        }

        private int GetEnemyHeight(EnemyType t)
        {
            switch (t)
            {
                case EnemyType.Boss:   return 80;
                case EnemyType.Tank:   return 52;
                case EnemyType.Runner: return 40;
                default:               return 36;
            }
        }
        private void CheckArrowEnemyCollisions()
        {
            for (int ai = arrows.Count - 1; ai >= 0; ai--)
            {
                if (arrows[ai].IsDead) continue;
                for (int ei = enemies.Count - 1; ei >= 0; ei--)
                {
                    if (!arrows[ai].Bounds.IntersectsWith(enemies[ei].Bounds)) continue;
                    bool killed = enemies[ei].Hit();
                    arrows[ai].Kill();
                    SpawnParticles(arrows[ai].X, arrows[ai].Y, Color.OrangeRed, 6, false);

                    if (killed)
                    {
                        int pts = 0;
                        Enemy e = enemies[ei];
                        pts = e.ScoreValue;
                        SpawnParticles(e.Bounds.X + e.Bounds.Width / 2,
                                       e.Bounds.Y + e.Bounds.Height / 2,
                                       Color.Gold, 18, true);
                        AddFloat("+" + pts, e.Bounds.X + 10, e.Bounds.Y - 10, Color.Yellow);
                        score += pts;
                        enemies.RemoveAt(ei);
                    }
                    break;
                }
            }
        }

        private void CheckEnemyPlayerCollisions()
        {
            if (player.IsInvincible) return;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i].Bounds.IntersectsWith(player.Bounds)) continue;

                player.TakeHit();
                SpawnParticles(player.X, player.Y - 40, Color.Crimson, 14, true);
                AddFloat("OUCH!", player.X - 20, player.Y - 80, Color.Red);

                if (player.Lives <= 0)
                {
                    state = GameState.GameOver;
                    scoreboard.AddScore("Archer", score, levelIndex + 1);
                    SpawnParticles(player.X, player.Y - 36, Color.OrangeRed, 40, true);
                }
                break;
            }
        }
        private void SpawnParticles(float x, float y, Color col, int count, bool big)
        {
            for (int i = 0; i < count; i++)
                particles.Add(new Particle(x, y, col,
                    big ? 4f : 2.5f,
                    big ? rand.Next(4, 9) : rand.Next(2, 5),
                    big ? rand.Next(25, 45) : rand.Next(12, 22)));
        }
        private void AddFloat(string text, float x, float y, Color col)
        {
            FloatText ft;
            ft.text = text; ft.x = x; ft.y = y;
            ft.timer = 60; ft.col = col;
            floatTexts.Add(ft);
        }
        private void CleanUp()
        {
            for (int i = arrows.Count    - 1; i >= 0; i--) if (arrows[i].IsDead)     arrows.RemoveAt(i);
            for (int i = enemies.Count   - 1; i >= 0; i--) if (enemies[i].IsOffScreen || enemies[i].IsDead) enemies.RemoveAt(i);
            for (int i = particles.Count - 1; i >= 0; i--) if (particles[i].IsDead)  particles.RemoveAt(i);
        }
        public void Draw(Graphics g)
        {
            LevelDefinition def = levels[levelIndex];
            using (var skyBrush = new LinearGradientBrush(
                    new Point(0, 0), new Point(0, H),
                    def.SkyTop, def.SkyBottom))
                g.FillRectangle(skyBrush, 0, 0, W, H);
            DrawClouds(g, def);
            DrawGround(g, def);
            for (int i = 0; i < enemies.Count;   i++) enemies[i].Draw(g);
            for (int i = 0; i < particles.Count;  i++) particles[i].Draw(g);
            for (int i = 0; i < arrows.Count;     i++) arrows[i].Draw(g);
            player.Draw(g);
            DrawAimGuide(g);
            DrawFloatTexts(g);
            DrawHUD(g, def);
            if (isPaused)                              DrawPause(g);
            if (state == GameState.WaveComplete)       DrawWaveComplete(g);
            if (state == GameState.LevelComplete)      DrawLevelComplete(g, def);
            if (state == GameState.GameOver)           DrawGameOver(g);
        }
        private void DrawClouds(Graphics g, LevelDefinition def)
        {
            int level = levelIndex + 1;
            if (level == 3)
            {
                DrawRock(g, (int)(cloudScroll),              H / 5);
                DrawRock(g, (int)(cloudScroll + W / 3),     H / 4 + 20);
                DrawRock(g, (int)(cloudScroll + 2 * W / 3), H / 5 + 30);
            }
            else
            {
                DrawCloud(g, (int)(cloudScroll),              60,  Color.FromArgb(200, Color.White));
                DrawCloud(g, (int)(cloudScroll + 220),        40,  Color.FromArgb(180, Color.White));
                DrawCloud(g, (int)(cloudScroll + 520),        80,  Color.FromArgb(200, Color.White));
                if (level == 2)  
                    DrawCloud(g, (int)(cloudScroll + 750), 50, Color.FromArgb(160, 255, 180, 100));
            }
        }

        private void DrawCloud(Graphics g, int x, int y, Color col)
        {
            using (SolidBrush br = new SolidBrush(col))
            {
                g.FillEllipse(br, x,       y,      80, 40);
                g.FillEllipse(br, x + 50,  y - 20, 60, 40);
                g.FillEllipse(br, x + 100, y,      70, 35);
            }
        }

        private void DrawRock(Graphics g, int x, int y)
        {
            using (SolidBrush br = new SolidBrush(Color.FromArgb(100, 80, 100, 120)))
            {
                Point[] poly = { new Point(x, y+30), new Point(x+20,y), new Point(x+50,y+10),
                                 new Point(x+70,y+5), new Point(x+80,y+35), new Point(x+40,y+50) };
                g.FillPolygon(br, poly);
            }
        }

        private void DrawGround(Graphics g, LevelDefinition def)
        {
            g.FillRectangle(new SolidBrush(def.HorizonColor), 0, groundY - 30, W, 30);

            g.FillRectangle(new SolidBrush(def.GroundColor), 0, groundY, W, H - groundY);

            g.DrawLine(new Pen(Color.FromArgb(60, Color.Black), 2), 0, groundY, W, groundY);
            if (levelIndex == 2)
            {
                using (Pen crack = new Pen(Color.FromArgb(200, Color.OrangeRed), 2))
                {
                    g.DrawLine(crack, 0, groundY + 15, 200, groundY + 20);
                    g.DrawLine(crack, 300, groundY + 10, 500, groundY + 25);
                    g.DrawLine(crack, 650, groundY + 18, 900, groundY + 12);
                }
            }
        }
        private void DrawAimGuide(Graphics g)
        {
            float px     = player.X + 22;
            float py     = player.Y - 40;
            float rad    = player.AimAngle * (float)Math.PI / 180f;
            float guideLen = 80f;

            using (Pen dotted = new Pen(Color.FromArgb(100, Color.White), 1))
            {
                dotted.DashStyle = DashStyle.Dot;
                g.DrawLine(dotted, px, py,
                    px + (float)Math.Cos(rad) * guideLen,
                    py + (float)Math.Sin(rad) * guideLen);
            }
        }
        private void DrawHUD(Graphics g, LevelDefinition def)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(160, Color.Black)), 5, 5, 220, 96);

            using (Font hf = new Font("Arial", 12, FontStyle.Bold))
            using (Font lf = new Font("Arial", 14, FontStyle.Bold))
            {
                g.DrawString("Score:  " + score,                   hf, Brushes.White,  12, 10);
                g.DrawString("Level:  " + (levelIndex + 1) + " / " + levels.Length
                             + "  — " + def.Name, hf, Brushes.LightYellow, 12, 30);
                g.DrawString("Wave:   " + (waveIndex + 1) + " / " + def.Waves.Count,
                             hf, Brushes.Cyan, 12, 50);
                g.DrawString(new string('♥', Math.Max(0, player.Lives)),
                             lf, Brushes.Crimson, 12, 70);
            }
            if (scoreboard.Entries.Count > 0)
                using (Font bf = new Font("Arial", 11, FontStyle.Bold))
                    g.DrawString("Best: " + scoreboard.Entries[0].Score,
                        bf, Brushes.Gold, W - 160, 10);
            using (Font hint = new Font("Arial", 8))
                g.DrawString("← → Move   ↑ ↓ Aim   SPACE Shoot   P Pause   R Restart",
                    hint, new SolidBrush(Color.FromArgb(160, Color.White)), W - 330, H - 20);
        }
        private void DrawFloatTexts(Graphics g)
        {
            using (Font f = new Font("Arial", 12, FontStyle.Bold))
            {
                for (int i = 0; i < floatTexts.Count; i++)
                {
                    FloatText t = floatTexts[i];
                    int alpha = Math.Min(255, t.timer * 4);
                    g.DrawString(t.text, f,
                        new SolidBrush(Color.FromArgb(alpha, t.col)), t.x, t.y);
                }
            }
        }
        private void DrawPause(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(145, Color.Black)), 0, 0, W, H);
            using (Font big = new Font("Arial", 36, FontStyle.Bold))
            using (Font sub = new Font("Arial", 16))
            {
                DrawCentered(g, "PAUSED",            big, Brushes.White,     H / 2 - 40);
                DrawCentered(g, "Press P to resume", sub, Brushes.LightGray, H / 2 + 12);
            }
        }

        private void DrawWaveComplete(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Black)), 0, 0, W, H);
            using (Font f = new Font("Arial", 30, FontStyle.Bold))
            using (Font s = new Font("Arial", 16))
            {
                DrawCentered(g, "Wave " + waveIndex + " Cleared!",   f, Brushes.LimeGreen,  H / 2 - 30);
                DrawCentered(g, "Get ready for wave " + (waveIndex + 1) + "…", s, Brushes.White, H / 2 + 20);
            }
        }

        private void DrawLevelComplete(Graphics g, LevelDefinition def)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(160, Color.Black)), 0, 0, W, H);
            using (Font big = new Font("Arial", 36, FontStyle.Bold))
            using (Font med = new Font("Arial", 18, FontStyle.Bold))
            {
                DrawCentered(g, "LEVEL " + (levelIndex + 1) + " COMPLETE!", big, Brushes.Gold,    H / 2 - 70);
                DrawCentered(g, def.Name + " conquered!",                    med, Brushes.LimeGreen, H / 2 - 16);
                DrawCentered(g, "Score: " + score + "  (+300 bonus)",        med, Brushes.White,    H / 2 + 22);
                if (levelIndex + 1 < levels.Length)
                    DrawCentered(g, "Preparing Level " + (levelIndex + 2) + "…", med, Brushes.Cyan, H / 2 + 60);
            }
        }

        private void DrawGameOver(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(178, Color.Black)), 0, 0, W, H);
            string headline = isWin ? "YOU WIN!" : "GAME OVER";
            Brush  hBrush   = isWin ? Brushes.LimeGreen : Brushes.OrangeRed;

            using (Font bigF  = new Font("Arial", 40, FontStyle.Bold))
            using (Font medF  = new Font("Arial", 18, FontStyle.Bold))
            using (Font smlF  = new Font("Arial", 13))
            {
                DrawCentered(g, headline,                                    bigF, hBrush,       H / 2 - 160);
                DrawCentered(g, "Score: " + score + "   Level: " + (levelIndex + 1),
                             medF, Brushes.White, H / 2 - 106);
                DrawCentered(g, "── HIGH SCORES ──",                         medF, Brushes.Gold, H / 2 - 68);

                int yy = H / 2 - 26;
                for (int i = 0; i < Math.Min(5, scoreboard.Entries.Count); i++)
                {
                    var e    = scoreboard.Entries[i];
                    string l = (i + 1) + ".   Score: " + e.Score + "   Level " + e.Level + "   " + e.Date;
                    DrawCentered(g, l, smlF,
                        i == 0 ? Brushes.Gold : Brushes.White, yy);
                    yy += 24;
                }
            }
        }

        private void DrawCentered(Graphics g, string text, Font font, Brush brush, float y)
        {
            SizeF sz = g.MeasureString(text, font);
            g.DrawString(text, font, brush, (W - sz.Width) / 2f, y);
        }
    }
}
