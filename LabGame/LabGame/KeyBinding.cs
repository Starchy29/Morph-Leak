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
    class KeyBinding<Type>
    {
        private Type[][] keys;

        // Properties
        public Type[] this[Inputs button] {
            get { return keys[(int) button]; }
            set { keys[(int)button] = value; }
        }

        // Constructor
        public KeyBinding(Type[] jump, Type[] ability, Type[] right, Type[] left, Type[] up, Type[] down, Type[] pause, Type[] select, Type[] back) {
            keys = new Type[Enum.GetNames(typeof(Inputs)).Length][]; // Number of buttons
            this[Inputs.Jump] = jump;
            this[Inputs.Ability] = ability;
            this[Inputs.Right] = right;
            this[Inputs.Left] = left;
            this[Inputs.Up] = up;
            this[Inputs.Down] = down;
            this[Inputs.Pause] = pause;
            this[Inputs.Select] = select;
            this[Inputs.Back] = back;
        }
    }
}
