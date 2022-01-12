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
    public class Wall : Entity, IComparable<Wall>
    {
        protected bool collides; // whether or not this collides, subclasses can use it
        public virtual bool Collides { get { return collides; } } // whether or not entities collide with this
        public bool PlayerColliding { get; set; } // when player updates, tells this whether or not it is colliding this frame
        private List<Vector2> tileSpots; // list of line segments representing where on the top to draw tiles. X is the beginning, y is the end. Uses range 0 - width

        // Constructors
        public Wall(int x, int y, int width, int height) : base(x, y, width, height) {
            collides = true; 
            tileSpots = new List<Vector2>();
            tileSpots.Add(new Vector2(0, width));
        }

        public override void Draw(SpriteBatch sb) {
            sb.Draw(Game1.Pixel, DrawBox, new Color(50, 50, 50));

            // draw floor
            if(Y > 20) {
                foreach(Vector2 segment in tileSpots) {
                    for(int x = (int)segment.X; x < segment.Y; x += 50) {
                        sb.Draw(Game1.Floor, new Rectangle(DrawBox.X + x, DrawBox.Y, 50, 50), Color.White);
                    }
                }
            }
        }

        public int CompareTo(Wall other) { // Used to order walls top to bottom to prevent pinch bugs
            return (int) (this.Y - other.Y);
        }

        public override bool Intersects(Vector2 point) {
            return Collides && base.Intersects(point);
        }

        protected bool IsAdjacent(Entity entity) {
            return (entity.X + entity.Width == this.X && entity.Y + entity.Height > this.Y && entity.Y < this.Y + this.height) // player on left wall
                || (entity.X == this.X + this.width && entity.Y + entity.Height > this.Y && entity.Y < this.Y + this.height) // player on right wall
                || (entity.Y + entity.Height == this.Y && entity.X + entity.Width > this.X && entity.X < this.X + this.width) // player on top
                || (entity.Y == this.Y + this.height && entity.X + entity.Width > this.X && entity.X < this.X + this.width); // player on bottom
        }

        public override void Update() {}

        public override void Start(List<Wall> walls, List<Entity> entities) {
            foreach(Wall other in walls) {
                if(!(other is MovingWall) && !(other is CrumbleWall) && other != this) {
                    // check if wall is above this
                    if(other.Y + other.height == this.Y && other.X + other.width >= this.X && other.X <= this.X + this.width) {
                        // find the line segment that this intersects
                        for(int i = 0; i < tileSpots.Count; i++) {
                            Vector2 segment = tileSpots[i]; // x: start, y: end
                            if(other.X <= this.X + segment.Y && other.X + other.width >= this.X + segment.X) {
                                tileSpots[i] = new Vector2(segment.X, other.X - this.X); // left segment
                                tileSpots.Insert(i + 1, new Vector2(other.X + other.width - this.X, segment.Y)); // right segment
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
