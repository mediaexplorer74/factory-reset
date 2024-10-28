using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameManager
{
    public class SpriteEngine
    {
        private Game1 Game;
        private Texture2D SolidTexture;
        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;
        private Effect TileEffect;
        
        public SpriteEngine(Game1 game)
        {
            Game = game;
        }
        
        public void LoadContent(ContentManager content)
        {
            SolidTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            SolidTexture.SetData<Color>(new Color[]{Color.White});
            
            VertexPositionTexture[] vertices = new VertexPositionTexture[] 
            { 
                new VertexPositionTexture(new Vector3(+0.5f, -0.5f, 0), Vector2.One),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, 0), Vector2.UnitY),
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, 0), Vector2.Zero),
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, 0), Vector2.UnitX)
            };
            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 
                                            vertices.Length, BufferUsage.None);
            
            VertexBuffer.SetData(vertices);
            short[] indices = new short[] { 0, 1, 2, 2, 3, 0 };
            IndexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(short), 
                                          indices.Length, BufferUsage.None);
            IndexBuffer.SetData(indices);

            // Create shader
            try
            {
                TileEffect = content.Load<Effect>("Shaders/sprite");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] SpriteEngine - Load Effect error: " + ex.Message);
            }
        }
        
        public void UnloadContent()
        {
            SolidTexture.Dispose();
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
            TileEffect.Dispose();
        }

        public void Draw(Texture2D texture, Vector4 source)
        {
            GraphicsDevice device = Game.GraphicsDevice;

            if (TileEffect != null)
            {
                TileEffect.CurrentTechnique = TileEffect.Techniques["Tile"];
                TileEffect.Parameters["projectionMatrix"].SetValue(Game.Transforms.ProjectionMatrix);
                TileEffect.Parameters["viewMatrix"].SetValue(Game.Transforms.ViewMatrix);
                TileEffect.Parameters["modelMatrix"].SetValue(Game.Transforms.ModelMatrix);
                TileEffect.Parameters["offset"].SetValue(source);
                TileEffect.Parameters["tileset"].SetValue(texture);
            }
            //else
            //{
            //    Debug.WriteLine("[warn] TileEffect is *null*!");
            //}


            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;
            device.BlendState = BlendState.AlphaBlend;

            if (TileEffect != null)
            { 
                foreach (EffectPass pass in TileEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                }
            }
        }
        
        public void Draw(Vector2 pos, Vector2 size)
        {
            Game.Transforms.Push();
            Game.Transforms.Scale(size);
            Game.Transforms.Translate(pos);
            
            Draw(SolidTexture, new Vector4(0, 0, 1, 1));
            Game.Transforms.Pop();
        }

        public void Draw(Vector4 rect)
        {
            Draw(new Vector2(rect.X, rect.Y), new Vector2(rect.Z, rect.W));
        }
    }
}
