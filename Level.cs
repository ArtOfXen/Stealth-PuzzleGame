using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class Level
    {
        public string name;
        public List<string> layout;
        public List<string> actors;
        public List<int> projectilesAllowed;
        public int numberOfProjectilesAllowed;
        public List<Tile> tiles;
        public float playerStartingAngle;
        public bool unlocked;

        public Level()
        {
            layout = new List<string>();
            actors = new List<string>();
            tiles = new List<Tile>();
            numberOfProjectilesAllowed = 0;
            projectilesAllowed = new List<int>();
            playerStartingAngle = 0;
            unlocked = false;
        }
    }
}
