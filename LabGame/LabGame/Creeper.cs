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
    // an enemy that walks back and forth on floors, walls, and ceilings
    class Creeper : Entity
    {
        private const float WALK_SPEED = 1.25f;
        private Direction side; // which side of the wall this stands on
        private Direction facing;
        private Animation animation;
        private bool alive;

        public override Rectangle DrawBox { get { 
            Vector2 offset = Camera.GetOffset();
            int scale = 15; // border increase, not multiplier
            int xShift = -scale / 2;
            int yShift = -scale / 2;
            switch(side) {
                case Direction.Down:
                    yShift = 0;
                    break;
                case Direction.Up:
                    yShift = -scale;
                    break;
                case Direction.Right:
                    xShift = 0;
                    break;
                case Direction.Left:
                    xShift = -scale;
                    break;
                }

            return new Rectangle((int)(position.X - offset.X + xShift), (int)(position.Y - offset.Y + yShift), width + scale, height + scale);
        } }

        public override FloatRect Hitbox { get { 
            return new FloatRect(position.X + 8, position.Y + 8, width - 16, height - 16);
        } }

        public Direction Side { get { return side; } }

        // Constructor
        public Creeper(int x, int y, Direction side) : base(x, y, 50, 75) {
            this.side = side;
            if(side == Direction.Up || side == Direction.Down) {
                facing = Direction.Left;
            } else {
                facing = Direction.Up;
                width = 75;
                height = 50;
            }
            alive = true;

            animation = new Animation(new Texture2D[]{Game1.CreeperLeftContact, Game1.CreeperLeftShift, Game1.CreeperLeftPass, Game1.CreeperRightPass, Game1.CreeperRightShift, Game1.CreeperRightContact }, AnimationType.Oscillate, 8);
        }

        public override void Update() {
            if(!alive) {
                return;
            }

            // move
            switch(facing) {
                case Direction.Right:
                    position.X += WALK_SPEED;
                    break;
                case Direction.Left:
                    position.X -= WALK_SPEED;
                    break;
                case Direction.Up:
                    position.Y -= WALK_SPEED;
                    break;
                case Direction.Down:
                    position.Y += WALK_SPEED;
                    break;
            }

            // Check if colliding with the player
            if(this.Hitbox.Intersects(Game1.Game.Player.Hitbox)) {
                Game1.Game.Player.Die();
            }

            // check collisions / edge of walls
            List<Wall> walls = Game1.Game.CurrentLevel.Walls;
            foreach(Wall wall in walls) {
                switch(facing) {
                    case Direction.Right:
                        if(this.Hitbox.Intersects(wall.Hitbox)) { // walk into wall
                            facing = Direction.Left;
                            this.X = wall.X - this.width;
                            return;
                        }
                        else if(side == Direction.Up
                            && this.Y + this.height == wall.Y // directly above
                            && this.X + this.width >= wall.X + wall.Width && this.X + this.width < wall.X + wall.Width + 2 * WALK_SPEED // ligned up with edge
                            && !IsWallTopLeft(wall.X + wall.Width, wall.Y) // no adjacent wall
                        ) {
                            facing = Direction.Left;
                            this.X = wall.X + wall.Width - this.width;
                            return;
                        }
                        else if(side == Direction.Down
                            && this.Y == wall.Y + wall.Height // directly below
                            && this.X + this.width >= wall.X + wall.Width && this.X + this.width < wall.X + wall.Width + 2 * WALK_SPEED // ligned up with edge
                            && !IsWallBottomLeft(wall.X + wall.Width, wall.Y + wall.Height) // no adjacent wall
                        ) {
                            facing = Direction.Left;
                            this.X = wall.X + wall.Width - this.width;
                            return;
                        }
                        break;

                    case Direction.Left:
                        if(this.Hitbox.Intersects(wall.Hitbox)) {
                            facing = Direction.Right;
                            this.X = wall.X + wall.Width;
                            return;
                        }
                        else if(side == Direction.Up
                            && this.Y + this.height == wall.Y // directly above
                            && this.X <= wall.X && this.X > wall.X - 2 * WALK_SPEED // ligned up with edge
                            && !IsWallTopRight(wall.X, wall.Y) // no adjacent wall
                        ) {
                            facing = Direction.Right;
                            this.X = wall.X;
                            return;
                        }
                        else if(side == Direction.Down
                            && this.Y == wall.Y + wall.Height // directly below
                            && this.X <= wall.X && this.X > wall.X - 2 * WALK_SPEED // ligned up with edge
                            && !IsWallBottomRight(wall.X, wall.Y + wall.Height) // no adjacent wall
                        ) {
                            facing = Direction.Right;
                            this.X = wall.X;
                            return;
                        }
                        break;

                    case Direction.Up:
                        if(this.Hitbox.Intersects(wall.Hitbox)) { // walk into wall
                            facing = Direction.Down;
                            this.Y = wall.Y + wall.Height;
                            return;
                        }
                        else if(side == Direction.Left
                            && this.X + this.width == wall.X // directly left
                            && this.Y <= wall.Y && this.Y > wall.Y - 2 * WALK_SPEED // ligned up with edge
                            && !IsWallBottomLeft(wall.X, wall.Y) // no adjacent wall
                        ) {
                            facing = Direction.Down;
                            this.Y = wall.Y;
                            return;
                        }
                        else if(side == Direction.Right
                            && this.X == wall.X + wall.Width // directly right
                            && this.Y <= wall.Y && this.Y > wall.Y - 2 * WALK_SPEED // ligned up with edge
                            && !IsWallBottomRight(wall.X + wall.Width, wall.Y) // no adjacent wall
                        ) {
                            facing = Direction.Down;
                            this.Y = wall.Y;
                            return;
                        }
                        break;

                    case Direction.Down:
                        if(this.Hitbox.Intersects(wall.Hitbox)) { // walk into wall
                            facing = Direction.Up;
                            this.Y = wall.Y - this.height;
                            return;
                        }
                        else if(side == Direction.Left
                            && this.X + this.width == wall.X // directly left
                            && this.Y + this.height >= wall.Y + wall.Height && this.Y + this.height < wall.Y + wall.Height + 2 * WALK_SPEED // ligned up with edge
                            && !IsWallTopLeft(wall.X, wall.Y + wall.Height) // no adjacent wall
                        ) {
                            facing = Direction.Up;
                            this.Y = wall.Y + wall.Height - this.Height;
                            return;
                        }
                        else if(side == Direction.Right
                            && this.X == wall.X + wall.Width // directly right
                            && this.Y + this.height >= wall.Y + wall.Height && this.Y + this.height < wall.Y + wall.Height + 2 * WALK_SPEED // ligned up with edge 
                            && !IsWallTopRight(wall.X + wall.Width, wall.Y + wall.Height) // no adjacent wall
                        ) {
                            facing = Direction.Up;
                            this.Y = wall.Y + wall.Height - this.Height;
                            return;
                        }
                        break;
                }
            }
        }

        public override void Draw(SpriteBatch sb) {         
            if(!alive) {
                return;
            }

            SpriteEffects orientation = SpriteEffects.None;
            if(side == Direction.Down || side == Direction.Up) { // horizontal
                if(facing == Direction.Left) {
                    orientation = SpriteEffects.FlipHorizontally;
                }
                if(side == Direction.Down) {
                    orientation |= SpriteEffects.FlipVertically;
                }
                sb.Draw(animation.GetNext(), DrawBox, null, Color.White, 0f, new Vector2(0, 0), orientation, 0f);
            } else { // vertical
                if(facing == Direction.Up) {
                    orientation = SpriteEffects.FlipHorizontally;
                }
                if(side == Direction.Left) {
                    orientation |= SpriteEffects.FlipVertically;
                }

                sb.Draw(animation.GetNext(), new Rectangle((int)(DrawBox.X + 75 + 15), (int)(DrawBox.Y), DrawBox.Height, DrawBox.Width), null, Color.White, (float)Math.PI / 2, new Vector2(0, 0), orientation, 0f);
            }
        }

        public override void Restart() {
            base.Restart();
            alive = true;
            if(side == Direction.Right || side == Direction.Left) {
                facing = Direction.Up;
            } else {
                facing = Direction.Left;
            }
        }

        private bool IsWallTopLeft(float x, float y) {
            List<Wall> walls = Game1.Game.CurrentLevel.Walls;
            foreach(Wall wall in walls) {
                if(wall.X == x && wall.Y == y) {
                    return true;
                }
            }
            return false;
        }

        private bool IsWallTopRight(float x, float y) {
            List<Wall> walls = Game1.Game.CurrentLevel.Walls;
            foreach(Wall wall in walls) {
                if(wall.X + wall.Width == x && wall.Y == y) {
                    return true;
                }
            }
            return false;
        }

        private bool IsWallBottomLeft(float x, float y) {
            List<Wall> walls = Game1.Game.CurrentLevel.Walls;
            foreach(Wall wall in walls) {
                if(wall.X == x && wall.Y + wall.Height == y) {
                    return true;
                }
            }
            return false;
        }

        private bool IsWallBottomRight(float x, float y) {
            List<Wall> walls = Game1.Game.CurrentLevel.Walls;
            foreach(Wall wall in walls) {
                if(wall.X + wall.Width == x && wall.Y + wall.Height == y) {
                    return true;
                }
            }
            return false;
        }
    }
}
