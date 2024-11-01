﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameManager
{
    class Door : BoxEntity
    {
        private class DoorCollision : BoxEntity, IOccludingEntity
        {
            public static readonly RectangleF NoCollide = new RectangleF(float.PositiveInfinity, float.PositiveInfinity, 0, 0);
            public bool Open = false;

            public DoorCollision(Vector2 position, Game1 game) : base(game, new Vector2(1, 0.99F*Chunk.TileSize))
            {
                Position = position;
            }

            public RectangleF GetOcclusionBox()
            {
                return GetBoundingBox();
            }

            public override RectangleF GetBoundingBox()
            {
                if (!Open)
                {
                    return base.GetBoundingBox();
                }
                else
                {
                    return NoCollide;
                }
            }
        }

        private readonly DoorCollision doorCollision;

        public BoxEntity GetSolidEntity { get{ return doorCollision; } }

        public IOccludingEntity GetOcclusionEntity { get { return doorCollision; } }

        private AnimatedSprite Sprite;

        public enum EState
        {
            Open,
            Opening,
            Closed,
            Crash
        };

        public EState State { get; private set; } = EState.Closed;

        public Door(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize * 3, Chunk.TileSize))
        {
            Position = position + Vector2.UnitY * (Chunk.TileSize / 2);
            Sprite = new AnimatedSprite(null, game, new Vector2(46, 32));
            doorCollision = new DoorCollision(Position, game);
        }

        private void Close(Chunk chunk)
        {
            Sprite.Play("closed");
            State = EState.Closed;
            doorCollision.Open = false;
            chunk.SolidTiles[chunk.GetTileLocation(Position - Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.AerialDroneWall;
            chunk.SolidTiles[chunk.GetTileLocation(Position + Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.AerialDroneWall;
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["door"];
            Sprite.Add("closed", 0, 1, 1.0);
            Sprite.Add("closing", 2, 8, 0.7, -1, 0);
            Sprite.Add("opening", 2, 8, 0.7, -1, 4);
            Sprite.Add("crash", 8, 14, 0.4, -1, 4);
            Sprite.Add("open", 14, 15, 1.0);
            Sprite.Play("closed");

            Game.SoundEngine.Load("open", "Door_Open");
            Game.SoundEngine.Load("crash", "Door_Crash");
        }

        public void Interact(Chunk chunk, bool fast, bool fromRight)
        {
            if (State != EState.Closed)
                return;
            if (fast)
            {
                State = EState.Crash;
                if (fromRight)
                    Game.Controller.Vibrate(1f, 0.5f, 0.4f);
                else
                    Game.Controller.Vibrate(0.5f, 1f, 0.4f);
                chunk.Level.Camera.Shake(6, 0.2F);
                Sprite.Play("crash");
                var sound = Game.SoundEngine.Play("crash", Position, 1F);
                chunk.MakeSound(sound, 90, Position);
            }
            else
            {
                State = EState.Opening;
                Sprite.Play("opening");
                var sound = Game.SoundEngine.Play("open", Position, 1F);
                chunk.MakeSound(sound, 60, Position);
            }

            if (fromRight)
                Sprite.Direction = -1;
        }

        public override void Update(Chunk chunk)
        {
            base.Update(chunk);

            Sprite.Update(Game1.DeltaT);

            switch (State)
            {
                case EState.Closed:
                case EState.Open:
                    break;
                case EState.Opening:
                    if (Sprite.Frame == 6)
                    {
                        doorCollision.Open = true;
                        chunk.SolidTiles[chunk.GetTileLocation(Position - Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.Empty;
                        chunk.SolidTiles[chunk.GetTileLocation(Position + Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.Empty;
                    }

                    if (Sprite.Frame == 14)
                    {
                        Sprite.Play("open");
                        Sprite.Update(0);
                        State = EState.Open;
                        // FIXME: Proper sounds
                    }
                    break;
                case EState.Crash:
                    if (Sprite.Frame == 9)
                    {
                        doorCollision.Open = true;
                        chunk.SolidTiles[chunk.GetTileLocation(Position - Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.Empty;
                        chunk.SolidTiles[chunk.GetTileLocation(Position + Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.Empty;
                    }
                    if (Sprite.Frame == 14)
                    {
                        Sprite.Play("open");
                        Sprite.Update(0);
                        State = EState.Open;

                    }
                    break;
            }
        }

        public override void DrawBackground()
        {
            Sprite.Draw(Position);
        }

        public override void Respawn(Chunk chunk)
        {
            //Close(chunk);
        }
    }
}
