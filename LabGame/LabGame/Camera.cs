using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    public static class Camera
    {
        private static Vector2 position; // the top left of the camera
        public static Vector2 Position { set { position = value; } }

        public static Vector2 GetOffset() {
            return position;
        }

        // follow the player when in a level
        public static void Update(Level level) {
            Player player = Game1.Game.Player;
            // place player in the middle of the screen, towards the left
            float vertOffset = player.Y - Game1.StartScreenHeight / 2 + 25;
            if(player.Height == 25) {
                vertOffset -= 25;
            }
            if(vertOffset < 0) {
                vertOffset = 0;
            } else if(vertOffset > level.Height - Game1.StartScreenHeight) {
                vertOffset = level.Height - Game1.StartScreenHeight;
            }

            float horiOffset = player.X - Game1.StartScreenWidth * 2 / 5;
            if(horiOffset < 0) {
                horiOffset = 0;
            } else if(horiOffset > level.Width - Game1.StartScreenWidth) {
                horiOffset = level.Width - Game1.StartScreenWidth;
            }

            position = new Vector2(horiOffset, vertOffset);
        }

        public static void Translate(Vector2 translation, Level level) {
            position += translation;

            // bounds
            const int buffer = 200;
            if(position.X < -2 * buffer) {
                position.X = -2 * buffer;
            }
            else if(position.X > level.Width + buffer - Game1.StartScreenWidth) {
                position.X = level.Width + buffer - Game1.StartScreenWidth;
            }
            if(position.Y < -buffer) {
                position.Y = -buffer;
            }
            else if(position.Y > level.Height + buffer - Game1.StartScreenHeight) {
                position.Y = level.Height + buffer - Game1.StartScreenHeight;
            }
        }
    }
}
