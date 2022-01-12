using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    class Spit : Entity
    {
        private const int LENGTH = 25;
        private const int SPEED = 15;
        private Vector2 velocity;
        private bool splattering; // doing splatter animation
        public bool IsActive { get; set; } // tells the level to delete this
        private Animation deathAnimation;

        // place the spit by its middle
        public Spit(int x, int y, Direction direction) : base(x - LENGTH / 2, y - LENGTH / 2, LENGTH, LENGTH) {
            switch(direction) {
                case Direction.Up:
                    velocity = new Vector2(0, -SPEED);
                    break;
                case Direction.Down:
                    velocity = new Vector2(0, SPEED);
                    break;
                case Direction.Left:
                    velocity = new Vector2(-SPEED, 0);
                    break;
                case Direction.Right:
                    velocity = new Vector2(SPEED, 0);
                    break;
            }

            IsActive = true;
            splattering = false;
            deathAnimation = new Animation(new Texture2D[] {Game1.Spit1, Game1.Spit2, Game1.Spit3, Game1.Spit4, Game1.Spit5}, AnimationType.End, 1);
        }

        public override void Update() {
            if(splattering) {
                return;
            }

            // Check if colliding with the player
            if(this.Hitbox.Intersects(Game1.Game.Player.Hitbox)) {
                Game1.Game.Player.Die();
                IsActive = false; // delete this without splat animation
            }

            position += velocity;

            // check offscreen
            if(X < -LENGTH || X > Game1.Game.CurrentLevel.Width || Y > Game1.Game.CurrentLevel.Height || Y < -LENGTH) {
                IsActive = false;
                return;
            }

            // check for collisions
            foreach(Wall wall in Game1.Game.CurrentLevel.Walls) {
                if(wall.Collides && wall.Hitbox.Intersects(this.Hitbox)) {
                    splattering = true;

                    // shift to edge of wall for animation
                    if(velocity.X > 0) {
                        X = wall.X - LENGTH;
                    }
                    else if(velocity.X < 0) {
                        X = wall.X + wall.Width;
                    }
                    else if(velocity.Y > 0) {
                        Y = wall.Y - LENGTH;
                    }
                    else if(velocity.Y < 0) {
                        Y = wall.Y + wall.Height;
                    }

                    if(wall is MovingWall) {
                        ((MovingWall)wall).AddSpit(this); // move death animation with moving wall
                    }
                    return;
                }
            }

            if(velocity.Y > 0) {
                foreach(Platform platform in Game1.Game.CurrentLevel.Platforms) {
                    if(Y < platform.Y && Y + height > platform.Y && X + width > platform.X && X < platform.X + platform.Width) {
                        splattering = true;
                        return;
                    }
                }
            }

        }

        public override void Draw(SpriteBatch sb) {
            SpriteEffects orientation = SpriteEffects.None;
            float rotation = 0f;
            int shift = 0;
            if(velocity.X < 0 || velocity.Y < 0) {
                orientation = SpriteEffects.FlipHorizontally;
            }
            if(velocity.Y != 0) {
                rotation = (float)Math.PI / 2;
                shift = LENGTH;
            }

            Texture2D sprite = Game1.SpitTexture;
            if(splattering) {
                sprite = deathAnimation.GetNext();
                if(deathAnimation.Complete) {
                    IsActive = false;
                    return;
                }
            }

            sb.Draw(sprite, new Rectangle(DrawBox.X + shift, DrawBox.Y, DrawBox.Width, DrawBox.Height), null, Color.White, rotation, Vector2.Zero, orientation, 0f);
        }

        public override void Restart() {
            Delete();
        }
    }
}
