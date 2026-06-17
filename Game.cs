using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLauncher
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public string CoverImagePath { get; set; }
        public string Category { get; set; }
        public string Publisher { get; set; }

        public Game(int id, string name, string executablePath, string coverImagePath = "", string category = "", string publisher = "")
        {
            Id = id;
            Name = name;
            ExecutablePath = executablePath;
            CoverImagePath = coverImagePath;
            Category = category;
            Publisher = publisher;
        }
    }
}
