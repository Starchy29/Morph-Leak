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
    public enum Type {
        Slippery, 
        Sticky, 
        Bouncy
    }

    public class Chemical : Entity
    {
        private Type type;
        private int cooldown;
        private Type startType;

        public const int SIDE_LENGTH = 50;
        private const int RESPAWN_FRAMES = 100;

        private bool OffCooldown { get { return cooldown == 0; } }
        public Type StartType { get { return startType; } }

        // Constructor - Sets middle position of chemical
        public Chemical(int x, int y, Type type) : base(x - SIDE_LENGTH / 2, y - SIDE_LENGTH / 2, SIDE_LENGTH, SIDE_LENGTH) {
            this.type = type;
            startType = type;
            cooldown = 0;
        }

        // make chemical based on top left corner
        public Chemical(Vector2 position, Type type) : base((int)position.X, (int)position.Y, SIDE_LENGTH, SIDE_LENGTH) {
            this.type = type;
            startType = type;
            cooldown = 0;
        }

        public override void Update() {
            if(OffCooldown) {
                Player player = Game1.Game.Player;
                // Check for player collision
                if(this.Hitbox.Intersects(player.Hitbox) && player.Form != this.type) {
                    // Swap forms
                    Type thisType = type;
                    type = player.Form;
                    player.SetForm(thisType);

                    // Apply cooldown
                    cooldown = RESPAWN_FRAMES;
                }
            }
            else { 
                cooldown--;
            }
        }

        public override void Draw(SpriteBatch sb) {
            if(OffCooldown) {
                sb.Draw(Game1.FlaskIn, DrawBox, ColorOf(type));
            }
            sb.Draw(Game1.FlaskOut, DrawBox, Color.White);
        }

        // Returns the color corresponding to the given chemical type
        public static Color ColorOf(Type type) {
            switch(type) {
                case Type.Bouncy:
                    return Color.LightGreen;

                case Type.Slippery:
                    if(Game1.Game.Colorblind) {
                        return Color.Purple;
                    }
                    return Color.LightBlue;

                case Type.Sticky:
                    if(Game1.Game.Colorblind) {
                        return Color.Orange;
                    }
                    return Color.Pink;
            }

            throw new ArgumentException("The input type does not exist.");
        }

        public override void Restart() {
            base.Restart();
            type = startType;
            cooldown = 0;
        }
    }
}
