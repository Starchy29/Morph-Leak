using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    // an enemy that stands still and periodically spits tar
    class Spitter : Entity
    {
        private Direction facing;
        private int cooldown;
        private const int FIRERATE = 80; // assume 60 fps
        public bool Offset { get; set; } // whether or not this shoots half a cycle offset
        public const int LENGTH = 60;
        private Animation animation;

        public Direction Facing { get { return facing; } }

        public override Rectangle DrawBox { get { 
            Vector2 offset = Camera.GetOffset();
            int scale = 6; // the amount of pixels increased in each dimension
            int xShift = -scale / 2;
            int yShift = -scale / 2;
            switch(facing) {
                case Direction.Down:
                    yShift = 0;
                    break;
                case Direction.Up:
                    yShift = -scale;
                    break;
                case Direction.Right:
                    xShift = 0;
                    break;
                case Direction.Left:
                    xShift = -scale;
                    break;
            }

            return new Rectangle((int)(position.X - offset.X + xShift), (int)(position.Y - offset.Y + yShift), width + scale, height + scale);
        } }

        public override FloatRect Hitbox { get { 
            if(facing == Direction.Down || facing == Direction.Up) {
                return new FloatRect(position.X + 15, position.Y + 8, width - 30, height - 16);
            } else {
                return new FloatRect(position.X + 8, position.Y + 15, width - 16, height - 30);
            }
        } } 

        public Spitter(int x, int y, Direction facing, bool offset) : base(x, y, LENGTH, LENGTH) {
            this.facing = facing;
            cooldown = 0;

            Offset = offset;
            if(offset) {
                cooldown = FIRERATE / 2;
            }

            animation = new Animation(new Texture2D[]{Game1.Spitter1, Game1.Spitter2, Game1.Spitter3, Game1.Spitter4}, AnimationType.Rebound, 2);
        }

        public override void Draw(SpriteBatch sb) {
            if(cooldown == 4 * 2) { // 4 frames before spit * waitframes
                animation.Restart();
            }
            Texture2D sprite = animation.GetNext();
            if(sprite == null) {
                sprite = Game1.SpitterIdle;
            }

            Color color = Color.White;
            if(Offset && Game1.Game.GameState == GameState.Editor) {
                color = Color.Gray;
            }

            float rotation = 0f;
            int shift = 0;
            SpriteEffects orientation = SpriteEffects.None;
            if(facing == Direction.Left || facing == Direction.Up) {
                orientation = SpriteEffects.FlipHorizontally;
            }
            if(facing == Direction.Up || facing == Direction.Down) {
                rotation = (float)Math.PI / 2;
                shift = LENGTH;
            }
            sb.Draw(sprite, new Rectangle(DrawBox.X + shift, DrawBox.Y, DrawBox.Width, DrawBox.Height), null, color, rotation, Vector2.Zero, orientation, 0f);
        }

        public override void Update() {
            // Check if colliding with the player
            if(this.Hitbox.Intersects(Game1.Game.Player.Hitbox)) {
                Game1.Game.Player.Die();
            }

            cooldown--;
            if(cooldown <= 0) {
                cooldown = FIRERATE;

                // spawn spit projectile
                int x = (int)this.X;
                int y = (int)this.Y;
                switch(facing) {
                    case Direction.Up:
                        x = (int)X + width / 2;
                        break;
                    case Direction.Down:
                        x = (int)X + width / 2;
                        y += height;
                        break;
                    case Direction.Left:
                        y = (int)Y + height / 2;
                        break;
                    case Direction.Right:
                        y = (int)Y + width / 2;
                        x += width;
                        break;
                }

                Game1.Game.CurrentLevel.AddProjectile(new Spit(x, y, facing));
            }
        }

        public override void Restart() {
            base.Restart();
            cooldown = 0;
            if(Offset) {
                cooldown = FIRERATE / 2;
            }
        }
    }
}
