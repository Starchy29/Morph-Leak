using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    // a sign that tells the player what button to press
    class ButtonSign : Entity
    {
        private Inputs[] inputs;
        private char connecter;

        // connecter is either a + or -> between each input
        public ButtonSign(int x, int y, Inputs[] inputs, char connecter = '-') : base(x, y, 0, 50) {
            int symbols = inputs.Length;
            this.connecter = '_';
            if(connecter == '+' || connecter == '>') {
                symbols += symbols - 1;
                this.connecter = connecter;
            }
            width = 50 * symbols;

            this.inputs = inputs;
        }

        public override void Draw(SpriteBatch sb) {
            sb.Draw(Game1.Pixel, new Rectangle(DrawBox.X - 4, DrawBox.Y - 4, width + 8, height + 8), Color.Gray); // border
            sb.Draw(Game1.Pixel, DrawBox, Color.Black);

            int position = 0;
            foreach(Inputs input in inputs) {
                switch(input) {
                    case Inputs.Ability:
                        if(Input.IsGamepadConnected) {
                            sb.Draw(Game1.Ring, GetSegment(position), Color.White);
                            sb.DrawString(Game1.Font, "X", GetTextBox(Game1.Font.MeasureString("X"), position), Color.White);
                        } else {
                            sb.Draw(Game1.Pixel, GetKeyBack(position), Color.White);
                            sb.DrawString(Game1.Font, "_", GetTextBox(Game1.Font.MeasureString("_"), position), Color.Black);
                        }
                        break;

                    case Inputs.Jump:
                        if(Input.IsGamepadConnected) {
                            sb.Draw(Game1.Ring, GetSegment(position), Color.White);
                            sb.DrawString(Game1.Font, "A", GetTextBox(Game1.Font.MeasureString("A"), position), Color.White);
                        } else {
                            sb.Draw(Game1.Pixel, GetKeyBack(position), Color.White);
                            sb.DrawString(Game1.Font, "W", GetTextBox(Game1.Font.MeasureString("W"), position), Color.Black);
                        }
                        break;

                    case Inputs.Down:
                        if(Input.IsGamepadConnected) {
                            sb.Draw(Game1.Ring, GetSegment(position), Color.White);

                            Rectangle normalSegment = GetSegment(position);
                            sb.Draw(Game1.Arrow, new Rectangle(normalSegment.X + 25, normalSegment.Y + 25, 50, 50), null, Color.White, (float)Math.PI / 2, new Vector2(Game1.Arrow.Width / 2, Game1.Arrow.Height / 2), SpriteEffects.None, 0f);
                        } else {
                            sb.Draw(Game1.Pixel, GetKeyBack(position), Color.White);
                            sb.DrawString(Game1.Font, "S", GetTextBox(Game1.Font.MeasureString("S"), position), Color.Black);
                        }
                        break;

                    case Inputs.Up:
                        if(Input.IsGamepadConnected) {
                            sb.Draw(Game1.Ring, GetSegment(position), Color.White);

                            Rectangle normalSegment = GetSegment(position);
                            sb.Draw(Game1.Arrow,  new Rectangle(normalSegment.X + 25, normalSegment.Y + 25, 50, 50), null, Color.White, -(float)Math.PI / 2, new Vector2(Game1.Arrow.Width / 2, Game1.Arrow.Height / 2), SpriteEffects.None, 0f);
                        } else {
                            sb.Draw(Game1.Pixel, GetKeyBack(position), Color.White);
                            sb.DrawString(Game1.Font, "W", GetTextBox(Game1.Font.MeasureString("W"), position), Color.Black);
                        }
                        break;

                    case Inputs.Right:
                        if(Input.IsGamepadConnected) {
                            sb.Draw(Game1.Ring, GetSegment(position), Color.White);
                            sb.Draw(Game1.Arrow, GetSegment(position), Color.White);
                        } else {
                            sb.Draw(Game1.Pixel, GetKeyBack(position), Color.White);
                            sb.DrawString(Game1.Font, "D", GetTextBox(Game1.Font.MeasureString("D"), position), Color.Black);
                        }
                        break;

                    case Inputs.Left:
                        if(Input.IsGamepadConnected) {
                            sb.Draw(Game1.Ring, GetSegment(position), Color.White);
                            sb.Draw(Game1.Arrow, GetSegment(position), null, Color.White, 0f, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0f);
                        } else {
                            sb.Draw(Game1.Pixel, GetKeyBack(position), Color.White);
                            sb.DrawString(Game1.Font, "A", GetTextBox(Game1.Font.MeasureString("A"), position), Color.Black);
                        }
                        break;
                }

                position++;

                if(50 * position >= Width) {
                    // don't draw connecter after sequence
                    break;
                }

                if(connecter == '+') {
                    sb.DrawString(Game1.Font, "+", GetTextBox(Game1.Font.MeasureString("+"), position), Color.White);
                    position++;
                }
                else if(connecter == '>') {
                    sb.Draw(Game1.Arrow, GetSegment(position), Color.White);
                    position++;
                }
            }
        }

        // returns the rectangle location of the nth symbol
        private Rectangle GetSegment(int num) {
            return new Rectangle(DrawBox.X + 50 * num, DrawBox.Y, 50, 50);
        }

        private Vector2 GetTextBox(Vector2 textDims, int num) {
            Rectangle segment = GetSegment(num);
            Vector2 result = new Vector2(segment.X, segment.Y);
            if(textDims.X < 50) {
                result.X += (50 - textDims.X) / 2;
            }
            if(textDims.Y < 50) {
                result.Y += (50 - textDims.Y) / 2;
            }

            return result;
        }

        private Rectangle GetKeyBack(int num) {
            Rectangle segment = GetSegment(num);
            return new Rectangle(segment.X + 6, segment.Y + 6, segment.Width - 12, segment.Height - 12);
        }

        public override void Update() {}
    }
}
