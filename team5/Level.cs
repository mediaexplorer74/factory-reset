﻿using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class Level
    {
        public ArrayList Chunks;
        public Chunk ActiveChunk;
        public Player Player;
        public Camera Camera;
        public int collected = 0;

        private bool ChunkTrans = false;

        //TESTING ONLY
        public Level(Game1 game)
        {
            Player = new Player(new Vector2(0, 0), game);
            Camera = new Camera(Player, game);
            ActiveChunk = new Chunk(game, Player, this, "Chunks/TestChunk");
        }
        
        public void LoadContent(ContentManager content)
        {
            ActiveChunk.LoadContent(content);
            Player.Position = ActiveChunk.SpawnPosition;
        }
        
        public void Resize(int width, int height)
        {
            Camera.Resize(width, height);
        }

        public void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);
            ActiveChunk.Update(gameTime);
            if (ChunkTrans)
            {
                Player.Update(gameTime, null);
            }
        }

        public void Draw(GameTime gameTime)
        {
            ActiveChunk.Draw(gameTime);
        }
    }
}
