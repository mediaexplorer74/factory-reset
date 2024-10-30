﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace GameManager
{
    public class TriangleEngine
    {
        public Game1 Game;
        private Effect TriangleEffect;
        private const int Triangles = 5;
        private static readonly Vector4 ConeColor = new Vector4(1, 0, 0, 0.3F);

        public TriangleEngine(Game1 game)
        {
            Game = game;
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                TriangleEffect = content.Load<Effect>("Shaders/triangle");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] TriangleEngine - Load Triangle Effect error: " + ex.Message);
            }

        }
        
        public void UnloadContent()
        {
            TriangleEffect.Dispose();
        }

        public void DrawTriangles(List<Vector2> triangles, Vector4 color)
        {
            GraphicsDevice device = Game.GraphicsDevice;

            if (TriangleEffect != null)
            {
                TriangleEffect.CurrentTechnique = TriangleEffect.Techniques["Triangle"];
                TriangleEffect.Parameters["projectionMatrix"].SetValue(Game.Transforms.ProjectionMatrix);
                TriangleEffect.Parameters["viewMatrix"].SetValue(Game.Transforms.ViewMatrix);
                TriangleEffect.Parameters["modelMatrix"].SetValue(Game.Transforms.ModelMatrix);
                TriangleEffect.Parameters["color"].SetValue(color);
            }

            VertexPosition[] vertices = new VertexPosition[triangles.Count];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new VertexPosition(new Vector3(triangles[i], 0));
            }

            //RnD
            //device.BlendState = BlendState.AlphaBlend;

            if (TriangleEffect != null)
            {
                foreach (EffectPass pass in TriangleEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 
                                              0, vertices.Length / 3,
                                              VertexPosition.VertexDeclaration);
                }
            }
        }

        public void DrawTriangles(Vector2 position, List<Vector2> triangles, Color coneColor)
        {
            Game.Transforms.Push();
            Game.Transforms.Translate(position);
            DrawTriangles(triangles, coneColor.ToVector4());
            Game.Transforms.Pop();
        }
    }
}
