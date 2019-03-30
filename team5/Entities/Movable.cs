﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class Movable : BoxEntity
    {
        public Vector2 Velocity = new Vector2();
        protected bool Grounded = false;

        public Movable(Game1 game, Vector2 size):base(game, size)
        {
        }

        protected void HandleCollisions(float dt, Chunk chunk){
            int direction;
            float time;
            RectangleF[] targetBB;
            Vector2[] targetVel;
            Grounded = false;
            while (chunk.CollideSolid(this, dt, out direction, out time, out targetBB, out targetVel))
            {   
                if ((direction & Chunk.Down) != 0)
                {
                    Grounded = true;
                    Velocity.Y = targetVel[0].Y;
                    Position.Y = targetBB[0].Top + Size.Y;
                }
                if ((direction & Chunk.Up) != 0)
                {
                    // <Nicolas> This results in a strange rebound from the top. Why was this done?
                    float relVel = Velocity.Y - targetVel[0].Y;
                    Velocity.Y = targetVel[0].Y - (relVel / 4);
                    Position.Y = targetBB[0].Bottom - Size.Y;
                }
                if ((direction & Chunk.Left) != 0)
                {
                    Velocity.X = targetVel[1].X;
                    Position.X = targetBB[1].Right + Size.X;
                }
                if ((direction & Chunk.Right) != 0)
                {
                    Velocity.X = targetVel[1].X;
                    Position.X = targetBB[1].Left - Size.X;
                }

                Position += Velocity * time * dt;

                dt = (1 - time) * dt;
            }
            Position += Velocity * dt;
        }
    }
}
