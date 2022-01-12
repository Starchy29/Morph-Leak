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
    // The type of action the player can input
    public enum Inputs {
        None, // Used for buffer
        Jump,
        Ability,
        Right, 
        Left,
        Up,
        Down,
        Pause,
        Select,
        Back
    }

    static class Input
    {
        private static KeyboardState lastkb;
        private static KeyboardState keyboard;
        private static GamePadState lastgp;
        private static GamePadState gamepad;
        private static Inputs bufferInput; // The input stored as a buffer
        private static int bufferFrames;
        private static bool lastClicked = false;
        private static bool mouseClicked = false;

        // set up default keybindings
        private static KeyBinding<Keys> keyboardBinds = new KeyBinding<Keys>(
            new Keys[] { Keys.W, Keys.Up }, // jump
            new Keys[] { Keys.Space }, // ability
            new Keys[] { Keys.D, Keys.Right }, // right
            new Keys[] { Keys.A, Keys.Left },  // left
            new Keys[] { Keys.W, Keys.Up }, // up
            new Keys[] { Keys.S, Keys.Down }, // down
            new Keys[] { Keys.Enter, Keys.Escape }, // pause
            new Keys[] { Keys.Enter, Keys.Space }, // select
            new Keys[] { Keys.Back, Keys.Escape } // back
        ); 
        private static KeyBinding<Buttons> gamePadBinds = new KeyBinding<Buttons>(
            new Buttons[] { Buttons.B, Buttons.A }, // jump
            new Buttons[] { Buttons.Y, Buttons.X, Buttons.LeftTrigger }, // ability
            new Buttons[] { Buttons.DPadRight, Buttons.LeftThumbstickRight }, // right
            new Buttons[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft }, // left
            new Buttons[] { Buttons.DPadUp, Buttons.LeftThumbstickUp }, // up
            new Buttons[] { Buttons.DPadDown , Buttons.LeftThumbstickDown, Buttons.RightTrigger }, // down
            new Buttons[] { Buttons.Back, Buttons.Start }, // pause
            new Buttons[] { Buttons.A, Buttons.X }, // select
            new Buttons[] { Buttons.Back, Buttons.B } // back
        );

        // Properties
        public static Inputs BufferInput { get { return bufferInput; } }
        public static bool IsGamepadConnected { get { return gamepad.IsConnected; } }

        public static void Update() {
            lastClicked = mouseClicked;
            mouseClicked = IsMouseClicked();

            lastkb = keyboard;
            keyboard = Keyboard.GetState();

            lastgp = gamepad;
            gamepad = GamePad.GetState(PlayerIndex.One);

            // Update buffer
            if(Game1.Game.GameState == GameState.Game) {
                if(JustPressed(Inputs.Jump)) { 
                    bufferInput = Inputs.Jump;
                    bufferFrames = 4;
                }
                else if(JustPressed(Inputs.Ability)) { 
                    bufferInput = Inputs.Ability;
                    bufferFrames = 8;
                }
                else if(bufferFrames > 0){
                    bufferFrames--;
                }
                else {
                    bufferInput = Inputs.None;
                }
            }
        }

        public static bool IsPressed(Inputs input) {
            if(gamepad.IsConnected) {
                if(Game1.Game.GameState == GameState.Game && input == Inputs.Down) { // If crouching, use a less sensitive check
                    Buttons[] binds = gamePadBinds[input];
                    foreach(Buttons bind in binds) {
                        if(IsHeld(bind)) {
                            return true;
                        }
                    }
                    return false;
                } else {
                    Buttons[] binds = gamePadBinds[input];
                    foreach(Buttons bind in binds) {
                        if(gamepad.IsButtonDown(bind)) {
                            return true;
                        }
                    }
                    return false;
                }
            } else {
                // mouse works as ability
                if(input == Inputs.Ability && IsMouseClicked()) {
                    return true;
                }

                Keys[] binds = keyboardBinds[input];
                foreach(Keys bind in binds) {
                    if(keyboard.IsKeyDown(bind)) {
                        return true;
                    }
                }
                return false;
            }           
        }

        public static bool IsUnpressed(Inputs input) {
            return !IsPressed(input);            
        }

        public static bool JustPressed(Inputs input) {
            if(IsUnpressed(input)) {
                return false;
            }

            if(gamepad.IsConnected) {
                Buttons[] binds = gamePadBinds[input];
                foreach(Buttons bind in binds) {
                    if(lastgp.IsButtonDown(bind)) {
                        return false;
                    }
                }

                return true;
            } else {
                Keys[] binds = keyboardBinds[input];
                foreach(Keys bind in binds) {
                    if(lastkb.IsKeyDown(bind)) {
                        return false;
                    }
                }

                // mouse works as ability
                if(lastClicked) {
                    return false;
                }

                return true;
            } 
        }

        // for typing
        public static bool JustPressed(Keys key) {
            return keyboard.IsKeyDown(key) && lastkb.IsKeyUp(key);
        }

        public static bool AnyKeyPressed() {
            if(gamepad.IsConnected) {
                return gamepad.PacketNumber != lastgp.PacketNumber; // Detects change in gamepad state. Assuming there were no button presses last frame, it works fine.
            } else {
                return keyboard.GetPressedKeys().Length > 0;
            }            
        }

        // returns the mouse's screen location converted from actual screen to standard 1200x900 dims. Use Camera.GetOffset() to convert to world coords
        public static Vector2 GetMousePos() {
            MouseState mouse = Mouse.GetState();
            Game1 game = Game1.Game;
            Vector2 scale = game.GameDims;
            return new Vector2((mouse.X - game.XOffset) * Game1.StartScreenWidth / scale.X, (mouse.Y - game.YOffset) * Game1.StartScreenHeight / scale.Y);
        }

        public static bool IsMouseClicked() {
            return Mouse.GetState().LeftButton == ButtonState.Pressed;
        }

        public static bool MouseJustClicked() {
            return !lastClicked && mouseClicked;
        }

        // returns a char that correspends to a key pressed on the keyboard
        public static String GetLetter() {
            Keys[] pressedKeys = keyboard.GetPressedKeys();
            Keys justPressedKey = Keys.None;
            foreach(Keys key in pressedKeys) {
                if(!lastkb.IsKeyDown(key)) {
                    justPressedKey = key;
                }
            }

            if(pressedKeys.Length > 0) {
                // convert Keys to string
                String result = "";

                switch(justPressedKey) {
                    case Keys.Space:
                        result = " ";
                        break;
                    case Keys.A:
                        result = "a";
                        break;
                    case Keys.B:
                        result = "b";
                        break;
                    case Keys.C:
                        result = "c";
                        break;
                    case Keys.D:
                        result = "d";
                        break;
                    case Keys.E:
                        result = "e";
                        break;
                    case Keys.F:
                        result = "f";
                        break;
                    case Keys.G:
                        result = "g";
                        break;
                    case Keys.H:
                        result = "h";
                        break;
                    case Keys.I:
                        result = "i";
                        break;
                    case Keys.J:
                        result = "j";
                        break;
                    case Keys.K:
                        result = "k";
                        break;
                    case Keys.L:
                        result = "l";
                        break;
                    case Keys.M:
                        result = "m";
                        break;
                    case Keys.N:
                        result = "n";
                        break;
                    case Keys.O:
                        result = "o";
                        break;
                    case Keys.P:
                        result = "p";
                        break;
                    case Keys.Q:
                        result = "q";
                        break;
                    case Keys.R:
                        result = "r";
                        break;
                    case Keys.S:
                        result = "s";
                        break;
                    case Keys.T:
                        result = "t";
                        break;
                    case Keys.U:
                        result = "u";
                        break;
                    case Keys.V:
                        result = "v";
                        break;
                    case Keys.W:
                        result = "w";
                        break;
                    case Keys.X:
                        result = "x";
                        break;
                    case Keys.Y:
                        result = "y";
                        break;
                    case Keys.Z:
                        result = "z";
                        break;
                }

                if(keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift)) {
                    result = result.ToUpper();
                }
                return result;
            }
            else {
                return "";
            }
        }

        // Returns the angle as a unit vector for grappling. Returns zero vector if no aim
        public static Vector2 GetAimAngle() {
            Vector2 angle = Vector2.Zero;
            if(gamepad.IsConnected) {
                angle = gamepad.ThumbSticks.Left;
                angle.Y = -angle.Y;
                if(angle == Vector2.Zero) {
                    // if not using left stick, check for d pad input
                    if(gamepad.IsButtonDown(Buttons.DPadUp)) {
                        angle.Y -= 1;
                    }
                    if(gamepad.IsButtonDown(Buttons.DPadDown)) {
                        angle.Y += 1;
                    }
                    if(gamepad.IsButtonDown(Buttons.DPadLeft)) {
                        angle.X -= 1;
                    }
                    if(gamepad.IsButtonDown(Buttons.DPadRight)) {
                        angle.X += 1;
                    }
                    if(angle != Vector2.Zero) {
                        angle.Normalize();
                    }
                }
            } else {
                Vector2 worldMouse = GetMousePos() + Camera.GetOffset();
                angle = GetMousePos() + Camera.GetOffset() - Game1.Game.Player.Midpoint;
            }

            if(angle.Length() == 0) {
                return new Vector2(0, 0);
            }

            angle.Normalize();

            return angle;
        }

        // Allows crouching to have a less sensitive check than the default.
        private static bool IsHeld(Buttons button) {
            Vector2 stickPosition;

            switch (button) {
                case Buttons.LeftThumbstickUp:
                    stickPosition = gamepad.ThumbSticks.Left;
                    return stickPosition.Y >= 0.7f;

                case Buttons.LeftThumbstickRight:
                    stickPosition = gamepad.ThumbSticks.Left;
                    return stickPosition.X >= 0.7f;

                case Buttons.LeftThumbstickDown:
                    stickPosition = gamepad.ThumbSticks.Left;
                    return stickPosition.Y <= -0.7f;

                case Buttons.LeftThumbstickLeft:
                    stickPosition = gamepad.ThumbSticks.Left;
                    return stickPosition.X <= -0.7f;

                case Buttons.RightThumbstickUp:
                    stickPosition = gamepad.ThumbSticks.Right;
                    return stickPosition.Y >= 0.7f;

                case Buttons.RightThumbstickRight:
                    stickPosition = gamepad.ThumbSticks.Right;
                    return stickPosition.X >= 0.7f;

                case Buttons.RightThumbstickDown:
                    stickPosition = gamepad.ThumbSticks.Right;
                    return stickPosition.Y <= -0.7f;

                case Buttons.RightThumbstickLeft:
                    stickPosition = gamepad.ThumbSticks.Right;
                    return stickPosition.X <= -0.7f;

                default: // If crouch is bound to a standard button, use a regular check
                    return gamepad.IsButtonDown(button);
            }           
        }

        // this just exists because there was an issue with fast falling immediately out of crouch jump
        public static bool DownJustHeld() {
            if(gamepad.IsConnected) {
                if(gamepad.IsButtonDown(Buttons.DPadDown) && !lastgp.IsButtonDown(Buttons.DPadDown)) {
                    return true;
                }

                if(!IsHeld(Buttons.LeftThumbstickDown)) {
                    return false;
                }

                return !(lastgp.ThumbSticks.Left.Y <= -0.7f);
            } else {
                return JustPressed(Inputs.Down);
            }
        }
    }
}
