using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    // the final boss of the game. It only moves sideways and chases after the player
    class Monstrosity : Entity
    {
        private const float SPEED = 6.4f; // player: 7
        private const int LENGTH = 500;
        private const int START_Y = 200;
        private const int TRIGGER_DIST = 300; // the distance into the level that triggers the first attempt to begin
        private bool firstAttempt;
        private Animation animation;

        public Monstrosity() : base(-LENGTH, START_Y, LENGTH, LENGTH) {
            // On the first attempt, the level is quiet and the boss does not move yet. Afterwards, the action keeps going between attempts.
            firstAttempt = true;

            animation = new Animation(Game1.Monstrosity, AnimationType.Oscillate, 5);
        }

        public override void Update() {
            if(firstAttempt) {
                // wait to move until player gets partially into the middle of the screen
                if(Game1.Game.Player.X >= TRIGGER_DIST) {
                    firstAttempt = false;
                    Music.Play(Songs.Boss);
                }
            } else if(position.X < Game1.Game.CurrentLevel.Width - Game1.StartScreenWidth - width) {
                // move until end of the level
                position.X += SPEED;
            }

            // kill player on collision
            if(this.Hitbox.Intersects(Game1.Game.Player.Hitbox)) {
                Game1.Game.Player.Die();
            }
        }

        public override void Restart() {
            if(firstAttempt) {
                position = new Vector2(-LENGTH, START_Y);
            } else {
                position = new Vector2(-(LENGTH + TRIGGER_DIST), START_Y);
            }
        }

        public override void Draw(SpriteBatch sb) {
            sb.Draw(animation.GetNext(), DrawBox, Color.White);
        }
    }
}
