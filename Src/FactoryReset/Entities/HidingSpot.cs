using Microsoft.Xna.Framework;

namespace GameManager
{
    class HidingSpot : BoxEntity
    {
        public HidingSpot(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize * 1.5f))
        {
            Position = position;
        }
    }
}
