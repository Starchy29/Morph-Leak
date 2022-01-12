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
    // A game object that exists in the world
    public abstract class Entity
    {
        protected Vector2 position; // The top left corner of the entity
        protected int width;
        protected int height;
        protected int startX;
        protected int startY;

        // Properties
        public virtual FloatRect Hitbox { get { return new FloatRect(position.X, position.Y, width, height); } }
        public virtual Rectangle DrawBox { get { 
            Vector2 offset = Camera.GetOffset();
            return new Rectangle((int)(position.X - offset.X), (int)(position.Y - offset.Y), width, height);
        } }
        public float X { 
            get { return position.X; }
            set { position.X = value; }
        }
        public float Y { 
            get { return position.Y; } 
            set { position.Y = value; }
        }
        public float Width { get { return width; } }
        public float Height { get { return height; } }
        public Vector2 Midpoint { 
            get { return new Vector2(position.X + (width / 2), position.Y + (height) / 2); } 
            set { position = new Vector2(value.X - width / 2, value.Y - height / 2); }
        }

        // Constructors
        public Entity(int x, int y, int width, int height) {
            position = new Vector2(x, y);
            this.width = width;
            this.height = height;
            startX = x;
            startY = y;
        }

        public abstract void Update();

        public abstract void Draw(SpriteBatch sb);

        // puts an entity back into its original state when the level restarts
        public virtual void Restart() {
            position.X = startX;
            position.Y = startY;
        }

        public virtual bool Intersects(Vector2 point) {
            return point.X >= position.X && point.Y >= position.Y && point.X <= position.X + width && point.Y <= position.Y + height;
        }

        // removes this entity from its level
        public void Delete() {
            Game1.Game.CurrentLevel.Entities.Remove(this);
        }

        // allows the entity to make decisions based on what other entities exist in the level. Called after all entities are loaded
        public virtual void Start(List<Wall> walls, List<Entity> entities) {}
    }
}
