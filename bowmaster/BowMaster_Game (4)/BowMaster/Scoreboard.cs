using System;
using System.Collections.Generic;
using System.IO;

namespace BowMaster
{
    class ScoreEntry
    {
        public string Name  { get; private set; }
        public int    Score { get; private set; }
        public int    Level { get; private set; }
        public string Date  { get; private set; }

        public ScoreEntry(string name, int score, int level, string date)
        {
            Name  = name;
            Score = score;
            Level = level;
            Date  = date;
        }

        public string ToFileString()
        {
            return Name + "|" + Score + "|" + Level + "|" + Date;
        }

        public static ScoreEntry FromFileString(string line)
        {
            string[] p = line.Split('|');
            if (p.Length != 4) return null;
            int s, l;
            if (!int.TryParse(p[1], out s) || !int.TryParse(p[2], out l)) return null;
            return new ScoreEntry(p[0], s, l, p[3]);
        }
    }
    class Scoreboard
    {
        private string           filePath;
        private List<ScoreEntry> entries = new List<ScoreEntry>();
        private const int        MaxEntries = 10;

        public List<ScoreEntry> Entries { get { return entries; } }

        public Scoreboard(string filePath)
        {
            this.filePath = filePath;
            Load();
        }

        public void AddScore(string name, int score, int level)
        {
            entries.Add(new ScoreEntry(name, score, level,
                DateTime.Now.ToString("yyyy-MM-dd")));
            entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            if (entries.Count > MaxEntries)
                entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
            Save();
        }

        private void Save()
        {
            try
            {
                using (StreamWriter w = new StreamWriter(filePath, false))
                    foreach (var e in entries)
                        w.WriteLine(e.ToFileString());
            }
            catch { }
        }

        private void Load()
        {
            if (!File.Exists(filePath)) return;
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        ScoreEntry e = ScoreEntry.FromFileString(line);
                        if (e != null) entries.Add(e);
                    }
                }
                entries.Sort((a, b) => b.Score.CompareTo(a.Score));
                if (entries.Count > MaxEntries)
                    entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
            }
            catch { }
        }
    }
}
