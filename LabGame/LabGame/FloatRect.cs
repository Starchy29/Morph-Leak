using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    public class FloatRect
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public FloatRect(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersects(FloatRect other) {
            return ! (this.X + this.Width <= other.X || this.X >= other.X + other.Width || this.Y + this.Height <= other.Y || this.Y >= other.Y + other.Height);
        }

        public static implicit operator FloatRect(Rectangle rectangle) {
            return new FloatRect((float) rectangle.X, (float) rectangle.Y, (float) rectangle.Width, (float) rectangle.Height);
        }
    }
}
