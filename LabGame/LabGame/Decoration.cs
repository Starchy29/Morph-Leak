using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    class Decoration : Entity
    {
        private Texture2D texture;
        public Decoration(int x, int y, int w, int h, Texture2D texture) : base(x, y, w, h) {
            this.texture = texture;
        }

        public override void Update() {}

        public override void Draw(SpriteBatch sb) {
            sb.Draw(texture, DrawBox, Color.White);
        }
    }
}
