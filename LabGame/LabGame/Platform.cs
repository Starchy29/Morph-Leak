using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LabGame
{
    // A horizontal, semi-solid platform
    public class Platform : Entity
    {
        List<Vector3> items; // x, y, and index representing which item to draw

        // Constructor
        public Platform(int x, int y, int width, int height) : base(x, y, width, height) {
            Random rng = new Random(x * width - y * height); // generate the same item placement every time
            items = new List<Vector3>();
            for(int itemX = 0; itemX < width; itemX += 50) {
                for(int itemY = 0; itemY < height - 50; itemY += 50) {
                    if(rng.NextDouble() < 0.2) { // 10% chance for an item at each spot
                        items.Add(new Vector3(itemX, itemY, rng.Next(0, 3)));
                    }
                }
            }
        }

        public override void Draw(SpriteBatch sb) {
            for(int y = 0; y < height; y += 50) {
                // draw left side
                sb.Draw(Game1.Shelf, new Rectangle(DrawBox.X, DrawBox.Y + y, 50, 50), new Rectangle(0, 0, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                // draw right side
                sb.Draw(Game1.Shelf, new Rectangle(DrawBox.X + width - 50, DrawBox.Y + y, 50, 50), new Rectangle(200, 0, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                // draw middles
                for(int x = 50; x < width - 50; x += 50) {
                    sb.Draw(Game1.Shelf, new Rectangle(DrawBox.X + x, DrawBox.Y + y, 50, 50), new Rectangle(100, 0, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                }
            }

            sb.Draw(Game1.Pixel, new Rectangle(DrawBox.X, DrawBox.Y, DrawBox.Width, 4), new Color(105, 12, 0)); // draw shelf top

            // draw items
            foreach(Vector3 item in items) {
                sb.Draw(Game1.ShelfItems[(int)item.Z], new Rectangle(DrawBox.X + (int)item.X + 5, DrawBox.Y + (int)item.Y + 10, 40, 40), Color.White);
            }
        }

        public override void Update() {}
    }
}
