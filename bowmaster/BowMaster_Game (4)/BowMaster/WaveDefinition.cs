using System;
using System.Collections.Generic;
using System.Drawing;

namespace BowMaster
{
    struct WaveEntry
    {
        public EnemyType Type;
        public int       DelayFrames;
    }
    struct WavePair
    {
        public EnemyType Type;
        public int       Delay;
        public WavePair(EnemyType t, int d) { Type = t; Delay = d; }
    }
    class LevelDefinition
    {
        public int   LevelNumber;
        public float SpeedMultiplier;
        public int   ShootCooldown;
        public float ArrowRange;
        public List<List<WaveEntry>> Waves;
        public Color SkyTop;
        public Color SkyBottom;
        public Color GroundColor;
        public Color HorizonColor;
        public string Name;
    }
    static class WaveDefinitions
    {
        private static WavePair E(EnemyType t, int delay)
        {
            return new WavePair(t, delay);
        }

        private static List<WaveEntry> MakeWave(WavePair[] entries)
        {
            var list = new List<WaveEntry>();
            foreach (WavePair e in entries)
                list.Add(new WaveEntry { Type = e.Type, DelayFrames = e.Delay });
            return list;
        }

        public static LevelDefinition[] Build()
        {
            return new LevelDefinition[]
            {
                new LevelDefinition
                {
                    LevelNumber     = 1,
                    SpeedMultiplier = 1.0f,
                    ShootCooldown   = 28,
                    ArrowRange      = 650f,
                    Name            = "The Meadow",
                    SkyTop          = Color.FromArgb(100, 160, 230),
                    SkyBottom       = Color.FromArgb(180, 220, 255),
                    GroundColor     = Color.FromArgb(80, 160, 55),
                    HorizonColor    = Color.FromArgb(100, 180, 70),
                    Waves = new List<List<WaveEntry>>
                    {
                        MakeWave(new WavePair[]{
                            E(EnemyType.Blob,   0),
                            E(EnemyType.Blob,  60),
                            E(EnemyType.Blob, 120)
                        }),
                        MakeWave(new WavePair[]{
                            E(EnemyType.Blob,   0),
                            E(EnemyType.Blob,  50),
                            E(EnemyType.Blob, 100),
                            E(EnemyType.Blob, 150)
                        }),
                        MakeWave(new WavePair[]{
                            E(EnemyType.Blob,    0),
                            E(EnemyType.Runner, 40),
                            E(EnemyType.Blob,   80),
                            E(EnemyType.Blob,  140)
                        }),
                    }
                },
                new LevelDefinition
                {
                    LevelNumber     = 2,
                    SpeedMultiplier = 1.55f,
                    ShootCooldown   = 22,
                    ArrowRange      = 700f,
                    Name            = "The Badlands",
                    SkyTop          = Color.FromArgb(180, 100, 40),
                    SkyBottom       = Color.FromArgb(240, 160, 80),
                    GroundColor     = Color.FromArgb(140, 90, 30),
                    HorizonColor    = Color.FromArgb(160, 100, 40),
                    Waves = new List<List<WaveEntry>>
                    {
                        MakeWave(new WavePair[]{
                            E(EnemyType.Runner,  0),
                            E(EnemyType.Blob,   30),
                            E(EnemyType.Runner, 60),
                            E(EnemyType.Blob,   90),
                            E(EnemyType.Blob,  130)
                        }),
                        MakeWave(new WavePair[]{
                            E(EnemyType.Tank,    0),
                            E(EnemyType.Runner, 20),
                            E(EnemyType.Runner, 50),
                            E(EnemyType.Blob,   80),
                            E(EnemyType.Blob,  120)
                        }),
                        MakeWave(new WavePair[]{
                            E(EnemyType.Runner,  0),
                            E(EnemyType.Tank,   10),
                            E(EnemyType.Runner, 40),
                            E(EnemyType.Blob,   70),
                            E(EnemyType.Tank,   90),
                            E(EnemyType.Blob,  130)
                        }),
                        MakeWave(new WavePair[]{
                            E(EnemyType.Tank,    0),
                            E(EnemyType.Runner, 10),
                            E(EnemyType.Runner, 30),
                            E(EnemyType.Tank,   50),
                            E(EnemyType.Blob,   80),
                            E(EnemyType.Blob,  110),
                            E(EnemyType.Runner,150)
                        }),
                    }
                },
                new LevelDefinition
                {
                    LevelNumber     = 3,
                    SpeedMultiplier = 2.0f,
                    ShootCooldown   = 16,
                    ArrowRange      = 780f,
                    Name            = "Boss Fortress",
                    SkyTop          = Color.FromArgb(20, 10, 40),
                    SkyBottom       = Color.FromArgb(60, 20, 80),
                    GroundColor     = Color.FromArgb(50, 30, 60),
                    HorizonColor    = Color.FromArgb(70, 40, 80),
                    Waves = new List<List<WaveEntry>>
                    {
                        MakeWave(new WavePair[]{
                            E(EnemyType.Runner,  0),
                            E(EnemyType.Runner, 10),
                            E(EnemyType.Tank,   20),
                            E(EnemyType.Runner, 40),
                            E(EnemyType.Blob,   60),
                            E(EnemyType.Runner, 80),
                            E(EnemyType.Blob,  110),
                            E(EnemyType.Tank,  130)
                        }),
                        MakeWave(new WavePair[]{
                            E(EnemyType.Tank,    0),
                            E(EnemyType.Tank,   20),
                            E(EnemyType.Runner, 30),
                            E(EnemyType.Tank,   50),
                            E(EnemyType.Runner, 60),
                            E(EnemyType.Runner, 80),
                            E(EnemyType.Tank,  100),
                            E(EnemyType.Blob,  130)
                        }),
                        MakeWave(new WavePair[]{
                            E(EnemyType.Runner,  0),
                            E(EnemyType.Runner, 10),
                            E(EnemyType.Tank,   20),
                            E(EnemyType.Runner, 40),
                            E(EnemyType.Tank,   60),
                            E(EnemyType.Runner, 80),
                            E(EnemyType.Tank,  100),
                            E(EnemyType.Runner,120),
                            E(EnemyType.Blob,  140),
                            E(EnemyType.Blob,  160)
                        }),
                        MakeWave(new WavePair[]{
                            E(EnemyType.Boss,    0),
                            E(EnemyType.Runner, 40),
                            E(EnemyType.Runner, 60),
                            E(EnemyType.Blob,   90),
                            E(EnemyType.Runner,120),
                            E(EnemyType.Blob,  150)
                        }),
                    }
                },
            };
        }
    }
}
