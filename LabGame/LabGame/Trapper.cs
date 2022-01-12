using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    class Trapper : Entity
    {
        private Direction wallDirection;
        private int timer;
        private Animation biteAnimation;

        private const int TRIGGER_TIME = 50; // amount of time after trigger before hitbox is active
        public const int LENGTH = 100;

        public Direction WallSide { get { return wallDirection; } }

        public override FloatRect Hitbox { get { return new FloatRect(position.X + 15, position.Y + 15, width - 30, height - 30); } } 

        public Trapper(int x, int y, Direction wallDirection) : base(x, y, LENGTH, LENGTH) {
            this.wallDirection = wallDirection;
            timer = 0;
            biteAnimation = new Animation(Game1.TrapperBite, AnimationType.End, 0);
        }

        public override void Update() {
            Player player = Game1.Game.Player;
            if(timer < 0) { // triggered, animating
                timer++;
                if(timer >= 0) {
                    timer = 1; // stays as positive to indicate always attacking
                }
            }
            else if(timer > 0) { // attacking
                if(this.Hitbox.Intersects(player.Hitbox)) {
                    player.Die();
                }
            }
            else if(this.Hitbox.Intersects(player.Hitbox)) { // waiting
                timer = -TRIGGER_TIME;
                biteAnimation.Restart();
            }
        }

        public override void Draw(SpriteBatch sb) {
            // determine orientation
            float rotation = 0f;
            int shift = 0;
            SpriteEffects orientation = SpriteEffects.None;
            if(wallDirection == Direction.Right || wallDirection == Direction.Down) {
                orientation = SpriteEffects.FlipHorizontally;
            }
            if(wallDirection == Direction.Down || wallDirection == Direction.Up) {
                rotation = (float)Math.PI / 2;
                shift = LENGTH;
            }

            Texture2D sprite = Game1.TrapperTeeth;
            if(timer < 0 && timer >= -10) {
                // bite animation
                sprite = biteAnimation.GetNext();
                if(sprite == null) {
                    sprite = Game1.Trapper;
                }
            }
            else if(timer > 0) {
                // closed mouth
                sprite = Game1.Trapper;
            }
            // else just teeth

            sb.Draw(sprite, new Rectangle(DrawBox.X + shift, DrawBox.Y, DrawBox.Width, DrawBox.Height), null, Color.White, rotation, Vector2.Zero, orientation, 0f);
        }

        public override void Restart() {
            base.Restart();
            timer = 0;
        }
    }
}
