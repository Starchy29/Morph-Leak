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
    public class Tar : Coating
    {
        private const int BUFFER = 3;
        private List<Rectangle> overlapSpots; // smooth corners that are shared, position relative to this

        // Constructor
        public Tar(int x, int y, int length, Direction side) : base(x, y, length, side, Color.White) { 
            overlapSpots = new List<Rectangle>();
        }
        public Tar(int x, int y, int width, int height, Direction side) : base(x, y, width, height, side, Color.White) {
            overlapSpots = new List<Rectangle>();
        }

        public override void Update() {
            Player player = Game1.Game.Player;
            // Kill player if colliding
            if( (side == Direction.Up || side == Direction.Down) // Vertical direction
                && player.Y + player.Height >= this.Y && player.Y <= this.Y + this.Height 
                && player.X + player.Width - BUFFER > this.X && player.X + BUFFER < this.X + this.Width
            ) {
                player.Die(Game1.Opposite(side));
            }
            else if( (side == Direction.Left || side == Direction.Right) // Horizontal direction
                && player.X + player.Width >= this.X && player.X <= this.X + this.Width
                && player.Y + player.Height - BUFFER > this.Y && player.Y + BUFFER < this.Y + this.Height
            ) {
                player.Die(Game1.Opposite(side));
            }
        }

        public override void Draw(SpriteBatch sb) {
            int length = width;
            if(side == Direction.Left || side == Direction.Right) {
                length = height;
            }

            SpriteEffects orientation = SpriteEffects.None;
            if(side == Direction.Down || side == Direction.Left) {
                orientation = SpriteEffects.FlipVertically;
            }

            
            if(side == Direction.Down || side == Direction.Up) { // horizontal
                // draw ends
                sb.Draw(Game1.TarTexture, new Rectangle(DrawBox.X, DrawBox.Y, 25, DrawBox.Height), new Rectangle(0, 0, 25, 50), Color.White, 0f, Vector2.Zero, orientation, 0f);
                sb.Draw(Game1.TarTexture, new Rectangle(DrawBox.X + DrawBox.Width - 25, DrawBox.Y, 25, DrawBox.Height), new Rectangle(75, 0, 25, 50), Color.White, 0f, Vector2.Zero, orientation, 0f);

                // draw middles
                for(int shift = 25; shift < length - 50; shift += 50) {
                    sb.Draw(Game1.TarTexture, new Rectangle(DrawBox.X + shift, DrawBox.Y, 50, DrawBox.Height), new Rectangle(25, 0, 50, 50), Color.White, 0f, Vector2.Zero, orientation, 0f);
                }
            }
            else { // vertical
                // draw ends
                sb.Draw(Game1.TarTexture, new Rectangle(DrawBox.X + 25, DrawBox.Y, 25, 25), new Rectangle(0, 0, 25, 50), Color.White, (float)Math.PI / 2, new Vector2(0, 0), orientation, 0f);
                sb.Draw(Game1.TarTexture, new Rectangle(DrawBox.X + 25, DrawBox.Y + DrawBox.Height - 25, 25, 25), new Rectangle(75, 0, 25, 50), Color.White, (float)Math.PI / 2, new Vector2(0, 0), orientation, 0f);

                // draw middles
                for(int shift = 25; shift < length - 50; shift += 50) {
                    sb.Draw(Game1.TarTexture, new Rectangle(DrawBox.X + 25, DrawBox.Y + shift, 50, 25), new Rectangle(25, 0, 50, 50), Color.White, (float)Math.PI / 2, new Vector2(0, 0), orientation, 0f);
                }
            }

            // draw overlaps
            foreach(Rectangle overlap in overlapSpots) {
                sb.Draw(Game1.Pixel, new Rectangle(overlap.X + (int)(position.X - Camera.GetOffset().X), overlap.Y + (int)(position.Y - Camera.GetOffset().Y), overlap.Width, overlap.Height), Color.Black);
            }
        }

        public override void Start(List<Wall> walls, List<Entity> entities) {
            // check for overlapping tar
            foreach(Entity entity in entities) {
                if(entity is Tar && entity != this && entity.Hitbox.Intersects(this.Hitbox)) {
                    int right = (int)Math.Min(entity.X + entity.Width, this.X + this.Width);
                    int left = (int)Math.Max(entity.X, this.X);
                    int bottom = (int)Math.Min(entity.Y + entity.Height, this.Y + this.Height);
                    int top = (int)Math.Max(entity.Y, this.Y);
                    overlapSpots.Add(new Rectangle(left + 2 - (int)position.X, top + 2 - (int)position.Y, right - left - 4, bottom - top - 4));
                }
            }
        }
    }
}
