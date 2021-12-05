using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPI411.SimpleEngine
{
    public class ParticleManager
    {
        //Inner data structure
        public struct VertexInfo : IVertexType
        {
            public Vector4 xyz;
            public Vector2 uv;
            public Vector4 pos;
            public Vector4 param;

            public VertexInfo(Vector4 _xyz, Vector2 _uv)
            {
                xyz = _xyz;
                uv = _uv;
                pos = Vector4.Zero;
                param = Vector4.Zero;
            }

            public readonly static VertexDeclaration vertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.Position, 2)
            );

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return vertexDeclaration; }
            }
        }
        //end of inner class

        public Particle[] particles;
        VertexInfo[] vtx;
        DynamicVertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        int activeMax;
        int particleMax;
        int index;

        public ParticleManager(GraphicsDevice g, int max)
        {
            particleMax = max;
            particles = new Particle[max];
            for (int i = 0; i < max; i++) particles[i] = new Particle();
            vtx = new VertexInfo[max * 4];
            for (int i = 0; i < max; i++)
            {
                vtx[i * 4 + 0] = new VertexInfo(new Vector4(-1, -1, 0, 1), new Vector2(0, 1));
                vtx[i * 4 + 1] = new VertexInfo(new Vector4(-1, 1, 0, 1), new Vector2(0, 0));
                vtx[i * 4 + 2] = new VertexInfo(new Vector4(1, -1, 0, 1), new Vector2(1, 1));
                vtx[i * 4 + 3] = new VertexInfo(new Vector4(1, 1, 0, 1), new Vector2(1, 0));
            }
            vertexBuffer = new DynamicVertexBuffer(g, typeof(VertexInfo), max * 4, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vtx);

            ushort[] idx = new ushort[max * 6];
            for (ushort i = 0; i < max; i++)
            {
                idx[i * 6 + 0] = (ushort)(i * 4 + 0);
                idx[i * 6 + 1] = (ushort)(i * 4 + 1);
                idx[i * 6 + 2] = (ushort)(i * 4 + 2);

                idx[i * 6 + 3] = (ushort)(i * 4 + 1);
                idx[i * 6 + 4] = (ushort)(i * 4 + 3);
                idx[i * 6 + 5] = (ushort)(i * 4 + 2);
            }
            indexBuffer = new IndexBuffer(g, typeof(ushort), idx.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(idx);
        }

        int next()
        {
            for (int i = index; i < particleMax; i++)
                if (particles[i].IsActive() == false)
                    return index = i;
            for (int i = 0; i < index; i++)
                if (particles[i].IsActive() == false)
                    return index = i;
            return 0;
        }

        public Particle getNext() { return particles[next()]; }
        public void Update(float ElapseGameTime)
        {
            activeMax = 0;
            for (int i = 0; i < particleMax; i++)
            {
                particles[i].Update(ElapseGameTime);
                if (particles[i].IsActive())
                {
                    int ax = activeMax * 4;
                    Particle tempP = particles[i];
                    vtx[ax + 0].pos.X = tempP.Position.X;
                    vtx[ax + 0].pos.Y = tempP.Position.Y;
                    vtx[ax + 0].pos.Z = tempP.Position.Z;

                    vtx[ax + 1].pos.X = tempP.Position.X;
                    vtx[ax + 1].pos.Y = tempP.Position.Y;
                    vtx[ax + 1].pos.Z = tempP.Position.Z;

                    vtx[ax + 2].pos.X = tempP.Position.X;
                    vtx[ax + 2].pos.Y = tempP.Position.Y;
                    vtx[ax + 2].pos.Z = tempP.Position.Z;

                    vtx[ax + 3].pos.X = tempP.Position.X;
                    vtx[ax + 3].pos.Y = tempP.Position.Y;
                    vtx[ax + 3].pos.Z = tempP.Position.Z;

                    vtx[ax + 0].param.X = tempP.Age;
                    vtx[ax + 1].param.X = tempP.Age;
                    vtx[ax + 2].param.X = tempP.Age;
                    vtx[ax + 3].param.X = tempP.Age;

                    vtx[ax + 0].param.Y = tempP.MaxAge;
                    vtx[ax + 1].param.Y = tempP.MaxAge;
                    vtx[ax + 2].param.Y = tempP.MaxAge;
                    vtx[ax + 3].param.Y = tempP.MaxAge;

                    activeMax++;
                }
            }
        }

        public void Draw(GraphicsDevice g)
        {
            if (activeMax <= 0) return;
            vertexBuffer.SetData(vtx);
            g.SetVertexBuffer(vertexBuffer);
            g.Indices = indexBuffer;
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, activeMax * 2);
        }
    }
}
