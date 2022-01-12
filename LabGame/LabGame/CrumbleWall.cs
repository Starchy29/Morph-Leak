using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;

namespace LabGame
{
    class CrumbleWall : Wall
    {
        private const int CRUMBLE_TIME = 30;
        private int timer = 0;
        private Animation currentAnimation = null;

        public override bool Collides { get { return timer <= 0; } }

        public CrumbleWall(int x, int y, int w, int h) : base(x, y, w, h) { }

        public override void Draw(SpriteBatch sb) {
            if(timer <= 0) {
                // draw corners
                sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X, DrawBox.Y, 50, 50), new Rectangle(0, 0, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X + DrawBox.Width - 50, DrawBox.Y, 50, 50), new Rectangle(200, 0, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X + DrawBox.Width - 50, DrawBox.Y + DrawBox.Height - 50, 50, 50), new Rectangle(200, 200, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X, DrawBox.Y + DrawBox.Height - 50, 50, 50), new Rectangle(0, 200, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                
                for(int x = 50; x < width - 50; x += 50) {
                    // draw top row
                    sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X + x, DrawBox.Y, 50, 50), new Rectangle(100, 0, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                    // draw middle rows
                    for(int y = 50; y < height - 50; y += 50) {
                        // draw middle middles
                        sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X + x, DrawBox.Y + y, 50, 50), new Rectangle(100, 100, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                    }

                    // draw bottom
                    sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X + x, DrawBox.Y + DrawBox.Height - 50, 50, 50), new Rectangle(100, 200, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                }

                for(int y = 50; y < height - 50; y += 50) {
                    // draw left edge
                    sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X, DrawBox.Y + y, 50, 50), new Rectangle(0, 100, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                    // draw right edge
                    sb.Draw(Game1.Crumbler, new Rectangle(DrawBox.X + DrawBox.Width - 50, DrawBox.Y + y, 50, 50), new Rectangle(200, 100, 100, 100), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                }
            }

            if(currentAnimation != null) {
                Texture2D sprite = currentAnimation.GetNext();
                if(sprite != null) {
                    for(int x = 0; x < width; x += 50) {
                        for(int y = 0; y < height; y += 50) {
                            sb.Draw(sprite, new Rectangle(DrawBox.X + x, DrawBox.Y + y, 50, 50), Color.White);
                        }
                    }
                }
            }
        }

        public override void Update() {
            Player player = Game1.Game.Player;

            if(timer == 0) { // waiting 
                if(PlayerColliding || player.GrappledWall == this) {
                    timer = -CRUMBLE_TIME;

                    // choose an animation speed
                    if(player.GrappledWall == this && player.State == Player.PlayerState.GrapplePull) { 
                        currentAnimation = new Animation(Game1.Cracking, AnimationType.End, 0);
                    } else {
                        currentAnimation = new Animation(Game1.Cracking, AnimationType.End, 4);
                    }
                }
            }
            else if(timer < 0) { // crumbling
                timer++;
                if(player.GrappledWall == this && player.State == Player.PlayerState.GrapplePull) { // shorten timer from pull grapple to make grapple launch work
                    timer += 4; // adds 5 total
                }
                if(timer == 0) {
                    timer = 1; // disable hitbox
                    currentAnimation = new Animation(Game1.Crumbling, AnimationType.End, 2);
                }
            }

            PlayerColliding = false; // reset check for next frame
        }

        public override void Restart() {
            timer = 0;
        }
    }
}
