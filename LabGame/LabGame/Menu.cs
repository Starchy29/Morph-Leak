using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    public class Menu
    {
        private Texture2D background;
        private List<Button> buttons;
        private Button selected;

        public Menu BackMenu { get; set; } // the menu to go to when the player goes back one menu

        public Menu(Texture2D background) {
            this.background = background;
            buttons = new List<Button>();
        }

        public void Update() {
            if(buttons.Count == 0) {
                return;
            }

            if(Input.IsGamepadConnected) {
                // use gamepad
                if(selected == null) {
                    selected = buttons[0];
                }

                if(Input.JustPressed(Inputs.Up)) {
                    if(selected.Up != null) {
                        selected = selected.Up;
                    }
                }
                else if(Input.JustPressed(Inputs.Down)) {
                    if(selected.Down != null) {
                        selected = selected.Down;
                    }
                }
                else if(Input.JustPressed(Inputs.Left)) {
                    if(selected.Left != null) {
                        selected = selected.Left;
                    }
                }
                else if(Input.JustPressed(Inputs.Right)) {
                    if(selected.Right != null) {
                        selected = selected.Right;
                    }
                }
                else if(Input.JustPressed(Inputs.Select)) { 
                    selected.Click();
                }

            } else {
                // use mouse hovering
                selected = null;
                Vector2 mouse = Input.GetMousePos();
                foreach(Button button in buttons) {
                    if(button.IsMouseHovering(mouse)) {
                        selected = button;
                    }
                }

                if(selected != null && Input.MouseJustClicked()) {
                    selected.Click();
                }
            }
        }

        public void AddButton(Button button) {
            buttons.Add(button);
        }

        public void Draw(SpriteBatch sb) {
            // draw backround
            sb.Draw(background, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);

            // draw buttons
            foreach(Button button in buttons) {
                button.Draw(sb, button == selected);
            }
        }

        // used for menus that can change over the course of the program
        public void ClearButtons() {
            buttons.Clear();
            selected = null;
        }

        // connects all buttons as a vertical column, using the list order as top to bottom
        public void ConnectVertically() {
            if(buttons.Count > 0) {
                // loop top and bottom
                buttons[0].Up = buttons[buttons.Count - 1];
                buttons[buttons.Count - 1].Down = buttons[0];
            }
            for(int i = 0; i < buttons.Count - 1; i++) {
                buttons[i].Down = buttons[i + 1];
                buttons[i + 1].Up = buttons[i];
            }
        }

        public void Select(int index) {
            selected = buttons[index];
        }
    }
}
