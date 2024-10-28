using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameManager
{
    abstract class TileType : GameObject
    {
        public TileType(Game1 game) : base(game)
        {
        }
    }
}
