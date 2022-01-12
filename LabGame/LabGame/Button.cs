using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    public delegate void DoAction();

    public class Button
    {
        private Rectangle rectangle;
        private DoAction action;
        private String text;
        private Texture2D image;
        private Color color;
        private DoAction otherAction;
        private String otherText;
        private bool toggle;

        // adjacent buttons when using gamepad
        public Button Up { get; set; }
        public Button Down { get; set; }
        public Button Left { get; set; }
        public Button Right { get; set; }

        public Button(int x, int y, int width, int height, String text, DoAction action) {
            Up = null;
            Down = null;
            Left = null;
            Right = null;

            rectangle = new Rectangle(x, y, width, height);
            this.text = text;
            this.action = action;
            image = null;
        }

        public Button(int x, int y, int width, int height, Texture2D image, DoAction action, Color color) {
            Up = null;
            Down = null;
            Left = null;
            Right = null;

            rectangle = new Rectangle(x, y, width, height);
            this.image = image;
            this.action = action;
            text = null;
            this.color = color;
        }

        // make a button that toggles
        public Button(int x, int y, int width, int height, String startText, String endText, DoAction onAction, DoAction offAction)
            : this(x, y, width, height, startText, onAction) {
            otherText = endText;
            otherAction = offAction;
        }

        public bool IsMouseHovering(Vector2 mouse) {
            return mouse.X > rectangle.X && mouse.X < rectangle.X + rectangle.Width && mouse.Y > rectangle.Y && mouse.Y < rectangle.Y + rectangle.Height;
        }

        public void Click() {
            if(otherAction != null && toggle) {
                otherAction();
            } else {
                action();
            }

            toggle = !toggle;
        }

        public void Draw(SpriteBatch sb, bool selected = false) {
            const int border = 7;
            Rectangle innerRect = new Rectangle(rectangle.X + border, rectangle.Y + border, rectangle.Width - 2 * border, rectangle.Height - 2 * border);
            sb.Draw(Game1.Pixel, rectangle, selected ? Color.Cyan : Color.Gray);
            sb.Draw(Game1.Pixel, innerRect, Color.Black);
            
            if(text != null) {
                if(otherText != null && toggle) {
                    Vector2 textDims = Game1.Font.MeasureString(otherText);
                    sb.DrawString(Game1.Font, otherText, new Vector2(rectangle.X + (rectangle.Width - textDims.X) / 2, rectangle.Y + (rectangle.Height - textDims.Y) / 2), selected ? Color.Cyan : Color.White);
                } else {
                    Vector2 textDims = Game1.Font.MeasureString(text);
                    sb.DrawString(Game1.Font, text, new Vector2(rectangle.X + (rectangle.Width - textDims.X) / 2, rectangle.Y + (rectangle.Height - textDims.Y) / 2), selected ? Color.Cyan : Color.White);
                }
            }
            if(image != null) {
                Color drawColor = color;
                // update color with colorblind option
                if(Game1.Game.Colorblind) {
                    if(color == Color.LightBlue) {
                        drawColor = Color.Purple;
                    }
                    else if(color == Color.Pink) {
                        drawColor = Color.Orange;
                    }
                }
                sb.Draw(image, innerRect, drawColor);
            }
        }
    }
}
