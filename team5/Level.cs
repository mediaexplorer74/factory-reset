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
    class Level:Window
    {
        public List<Chunk> Chunks = new List<Chunk>();
        public Chunk ActiveChunk = null;
        public Player Player;
        public Camera Camera;
        public Alarm Alarm;
        public int collected = 0;
        
        public readonly string Name;
        private readonly string Description;
        
        private readonly Game1 Game;
        private bool ChunkTrans = false;
        private List<Chunk> TransitionChunks = new List<Chunk>();
        private int TransitionDirection = 0;
        private Chunk LastActiveChunk;
        private Chunk TargetChunk;
        private int TransitionLingerCounter = 0;
        private const int TransitionLingerDuration = 40;

        public bool Paused = false;
        private readonly List<Container> Popups = new List<Container>();

        public void Pause()
        {
            Paused = !Paused;
            Popups.Add(new TextBox("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", 
                "welbut",12,Game, this,Vector2.Zero,Chunk.Down | Chunk.Left));
        }

        public Level(Game1 game, string name)
        {
            Player = new Player(new Vector2(0, 0), game);
            Camera = new Camera(Player, game);
            Game = game;
            Name = name;
            Alarm = new Alarm(game);
        }

        public void ClosePopup()
        {
            Popups.RemoveAt(Popups.Count - 1);
            if(Popups.Count == 0)
            {
                Paused = false;
            }
        }

        public void ClosePopup(Container popup)
        {
            Popups.Remove(popup);
            if (Popups.Count == 0)
            {
                Paused = false;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            var data = content.Load<LevelContent>("Levels/"+Name);
            
            foreach(var chunkdata in data.chunks)
            {
                Chunk chunk = new Chunk(Game, this, chunkdata);
                chunk.LoadContent(content);
                Chunks.Add(chunk);
            }
            
            Player.LoadContent(content);
            ActiveChunk = Chunks[data.startChunk];
            Player.Position = ActiveChunk.SpawnPosition;
            ActiveChunk.Activate(Player);
            LastActiveChunk = ActiveChunk;
            
            
            //  Force camera to be still
            Camera.Position.X = Player.Position.X;
            Camera.Position.Y = Player.Position.Y;
            Camera.UpdateChunk(ActiveChunk);
            Camera.SnapToLocation();
            //Alarm sound
            Alarm.LoadContent(content);

            TextBox.LoadStaticContent(content);

        }
        
        public override void Resize(int width, int height)
        {
            Camera.Resize(width, height);
        }

        public override void Update()
        {
            if (Paused)
            {
                Popups.Last().Update();
                Camera.Update();
            }
            else
            {
                Camera.Update();

                RectangleF PlayerBB = Player.GetBoundingBox();

                if (!ChunkTrans)
                {
                    if (PlayerBB.Right + Game1.DeltaT * Player.Velocity.X > ActiveChunk.BoundingBox.Right && Player.Velocity.X > 0)
                    {
                        TransitionDirection = Chunk.Right;
                        ChunkTrans = true;
                    }
                    else if (PlayerBB.Left + Game1.DeltaT * Player.Velocity.X < ActiveChunk.BoundingBox.Left && Player.Velocity.X < 0)
                    {
                        TransitionDirection = Chunk.Left;
                        ChunkTrans = true;
                    }
                    else if (PlayerBB.Top >= ActiveChunk.BoundingBox.Top && Player.Velocity.Y > 0)
                    {
                        TransitionDirection = Chunk.Up;
                        ChunkTrans = true;
                    }
                    else if (PlayerBB.Bottom <= ActiveChunk.BoundingBox.Bottom && Player.Velocity.Y < 0)
                    {
                        TransitionDirection = Chunk.Down;
                        ChunkTrans = true;
                    }

                    if (ChunkTrans)
                    {
                        TransitionLingerCounter = 0;
                        TargetChunk = null;
                        foreach (var chunk in Chunks)
                        {
                            PlayerBB.X += Math.Min(0, Game1.DeltaT * Player.Velocity.X);
                            PlayerBB.Width += Math.Abs(Game1.DeltaT * Player.Velocity.X);
                            if (PlayerBB.Intersects(chunk.BoundingBox))
                            {
                                if (chunk != ActiveChunk)
                                {
                                    TargetChunk = chunk;
                                }
                            }
                        }

                        if (TargetChunk == null)
                        {
                            TargetChunk = LastActiveChunk;
                            if (TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right)
                            {
                                ChunkTrans = false;
                            }
                        }
                        if (ChunkTrans)
                        {
                            if ((TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right) &&
                                TargetChunk.CollideSolid(Player, Game1.DeltaT, out int direction, out float time, out RectangleF[] targetBB, out Vector2[] targetvel))
                            {
                                ChunkTrans = false;
                            }
                            else
                            {
                                if (TransitionDirection == Chunk.Up)
                                {
                                    Player.Velocity.Y = 350;
                                }
                                Player.Position.X += Player.Velocity.X * Game1.DeltaT;
                                ActiveChunk.Deactivate();
                                LastActiveChunk = ActiveChunk;
                                ActiveChunk = null;
                                Camera.UpdateChunk(TargetChunk);
                            }
                        }
                    }
                }

                if (ChunkTrans)
                {
                    TransitionChunks.Clear();

                    TargetChunk.Update();

                    foreach (var chunk in Chunks)
                    {
                        if (PlayerBB.Intersects(chunk.BoundingBox))
                        {
                            TransitionChunks.Add(chunk);
                        }
                    }

                    if (TransitionChunks.Count == 1)
                    {
                        if ((TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right))
                        {
                            TransitionLingerCounter++;
                        }
                        else
                        {
                            TransitionLingerCounter = TransitionLingerDuration;
                        }
                    }

                    if (TransitionLingerCounter == TransitionLingerDuration)
                    {
                        TransitionLingerCounter = 0;
                        ActiveChunk = TargetChunk;
                        ActiveChunk.Activate(Player);
                        ChunkTrans = false;
                        TransitionChunks.Clear();
                        TransitionDirection = 0;
                    }
                    else if (TransitionChunks.Count == 0)
                    {
                        ActiveChunk = LastActiveChunk;
                        ActiveChunk.Activate(Player,false);
                        ActiveChunk.Die(Player);
                        ChunkTrans = false;
                        TransitionChunks.Clear();
                        TransitionDirection = 0;
                        Player.Velocity = new Vector2(0);
                        return;
                    }

                    Player.Update(TransitionDirection, TransitionLingerCounter, TargetChunk);
                }
                else
                {
                    if (ActiveChunk != null)
                    {
                        ActiveChunk.Update();
                        Alarm.Update(ActiveChunk);
                    }
                }
            }
        }

        public override void Draw()
        {

            if (ChunkTrans)
                Player.Draw();

            foreach (Chunk chunk in Chunks)
                if (Camera.IsVisible(chunk.BoundingBox))
                    chunk.Draw();

            Alarm.Draw(this);

            foreach (Container container in Popups)
                container.Draw();
        }

        public override void OnQuitButon()
        {
            Pause();
        }
    }
}
