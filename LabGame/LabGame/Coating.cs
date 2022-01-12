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
    // A substance applied to one side of a wall. This was going to be used for mutliple classes, but it ended being just for the tar class
    public abstract class Coating : Entity
    {
        protected Direction side; // Which side of the wall the coating is on
        private Color color;
        
        // Properties
        public Direction Side { get { return side; } }

        // Constructors
        public Coating(int x, int y, int length, Direction side, Color color) 
            : base(x, y, (side == Direction.Left || side == Direction.Right ? 25 : length), (side == Direction.Left || side == Direction.Right ? length : 25)) {
            this.side = side;
            this.color = color;

            if(side == Direction.None) {
                throw new ArgumentException("The side of the coating was set to none.");
            }
        }

        public Coating(int x, int y, int width, int height, Direction side, Color color) : base(x, y, width, height) {
            this.side = side;
            this.color = color;

            if(side == Direction.None) {
                throw new ArgumentException("The side of the coating was set to none.");
            }
        }
    } 
}
