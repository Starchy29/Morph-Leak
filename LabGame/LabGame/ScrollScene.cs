using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    // This is basically a singleton for the one scene that works like this
    class ScrollScene : Scene
    {
        private int xShift;
        private Texture2D frontPass;
        private Texture2D backPass;
        private Animation guy;
        private Animation background;

        public ScrollScene(int animationDuration) : base(Game1.WalkAway[0], Game1.WalkAwayGuy[0], 0, animationDuration) {
            xShift = 0;
            this.frontPass = Game1.FrontDebris;
            this.guy = new Animation(Game1.WalkAwayGuy, AnimationType.Loop, 8);
            this.backPass = Game1.BackDebris;
            background = new Animation(Game1.WalkAway, AnimationType.Loop, 8);
        }

        public override void DrawNextFrame(SpriteBatch sb) {
            currentFrame++;
            xShift += 15;
            if(xShift >= Game1.StartScreenWidth && currentFrame < animationDuration - 2 * Game1.StartScreenWidth / 15) {
                xShift = 0;
            }

            if(background != null) {
                sb.Draw(background.GetNext(), new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
            }
            if(guy != null) {
                // back
                sb.Draw(backPass, new Rectangle(-xShift, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
                sb.Draw(backPass, new Rectangle(Game1.StartScreenWidth - xShift, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);

                // mid
                sb.Draw(guy.GetNext(), new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);

                // front
                sb.Draw(frontPass, new Rectangle(-xShift, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
                sb.Draw(frontPass, new Rectangle(Game1.StartScreenWidth - xShift, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
            }
        }

        // just don't transition into or out of one of these
    }
}
