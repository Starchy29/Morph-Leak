using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    class MovingWall : Wall
    {
        private Direction facing;
        private Vector2 start;
        private Vector2 end;
        private float near;
        private float far;
        private List<Entity> clingers; // entities that stick to this as it moves, including Tar, Spitter, and Creeper
        private bool firstUpdate;
        private Animation animation;
        private int numBlocks; // the number of blocks side by side on this one track

        private const float SPEED = 3f;
        private const int HALF_LENGTH = 75; // half the width of one block

        public Vector2 Start { get { return start; } }
        public Vector2 End { get { return end; } }
        public int NumberBlocks { get { return numBlocks; } }

        // place a 150 x 150 wall based on the track it moves on. Track should always be vertical or horizontal. Now it can have multiple blocks in a row
        public MovingWall(Vector2 start, Vector2 end, int numBlocks = 1): base((int)start.X - HALF_LENGTH, (int)start.Y - HALF_LENGTH, HALF_LENGTH * 2, HALF_LENGTH * 2) {
            if(start.X == end.X) { // vertical
                facing = Direction.Down;
                near = start.Y;
                far = end.Y;
                if(end.Y < start.Y) { // upside down
                    near = end.Y;
                    far = start.Y;
                }              
            }
            else if(start.Y == end.Y) { // horizontal
                width = 2 * HALF_LENGTH * numBlocks;

                facing = Direction.Right;
                near = start.X;
                far = end.X;
                if(end.X < start.X) { // backwards
                    near = end.X;
                    far = start.X;
                }  
            }
            else {
                throw new ArgumentException("Moving Wall was not given a line that is straight vertical or horizontal");
            }

            clingers = new List<Entity>();
            this.start = start;
            this.end = end;
            firstUpdate = true;
            animation = new Animation(Game1.Gearbox, AnimationType.Loop, 5);

            this.numBlocks = numBlocks;
            PlaceBlocks();
        }

        public override void Update() {
            if(firstUpdate) {
                firstUpdate = false;
                // after restarting, find which tars and enemies are attached to this. level always restarts before playing
                clingers = new List<Entity>();
                List<Entity> entities = Game1.Game.CurrentLevel.Entities;
                foreach(Entity entity in entities) {
                    if(entity is Tar && entity.Hitbox.Intersects(this.Hitbox)) {
                        clingers.Add(entity);
                    }
                    else if((entity is Spitter || entity is Creeper || entity is Trapper) && IsAdjacent(entity)) {
                        clingers.Add(entity);
                    }
                }
            }

            Vector2 startPos = position;

            switch(facing) {
                case Direction.Right:
                    position.X += SPEED;
                    if(position.X + width - HALF_LENGTH > far) {
                        position.X = far - width + HALF_LENGTH;
                        facing = Direction.Left;
                    }
                    break;

                case Direction.Left:
                    position.X -= SPEED;
                    if(position.X + HALF_LENGTH < near) {
                        position.X = near - HALF_LENGTH;
                        facing = Direction.Right;
                    }
                    break;

                case Direction.Down:
                    position.Y += SPEED;
                    if(position.Y + height - HALF_LENGTH > far) {
                        position.Y = far - height + HALF_LENGTH;
                        facing = Direction.Up;
                    }
                    break;

                case Direction.Up:
                    position.Y -= SPEED;
                    if(position.Y + HALF_LENGTH < near) {
                        position.Y = near - HALF_LENGTH;
                        facing = Direction.Down;
                    }
                    break;
            }

            // shift player if attached
            Player player = Game1.Game.Player;
            
            Vector2 shift = position - startPos;
            if(PlayerColliding || player.GrappledWall == this) {
                player.X += shift.X;
                player.Y += shift.Y;
            }

            // move player outside of wall if this moved into it
            if(player.State != Player.PlayerState.Dead) { // don't push player around after death
                if(facing == Direction.Left && player.X + player.Width > this.X && player.X + player.Width <= this.X + this.width / 2.0f 
                    && player.Y >= this.Y && player.Y + player.Height <= this.Y + this.height) {
                    // player in left side
                    player.X = this.X - player.Width;

                    // kill if pushed into a wall
                    List<Wall> walls = Game1.Game.CurrentLevel.Walls;
                    foreach(Wall wall in walls) {
                        if(wall.Hitbox.Intersects(player.Hitbox)) {
                            player.Die();
                        }
                    }
                }
                else if(facing == Direction.Right && player.X < this.X + this.width && player.X  > this.X + this.width / 2.0f 
                    && player.Y >= this.Y && player.Y + player.Height <= this.Y + this.height) {
                    // player in right side
                    player.X = this.X + this.width;

                    // kill if pushed into a wall
                    List<Wall> walls = Game1.Game.CurrentLevel.Walls;
                    foreach(Wall wall in walls) {
                        if(wall.Hitbox.Intersects(player.Hitbox)) {
                            player.Die();
                        }
                    }
                }
                else if(facing == Direction.Up && player.Y + player.Height > this.Y && player.Y + player.Height <= this.Y + this.height / 2.0f 
                    && player.X >= this.X && player.X + player.Width <= this.X + this.width) {
                    // player in top
                    player.Y = this.Y - player.Height;

                    // kill if pushed into a wall
                    List<Wall> walls = Game1.Game.CurrentLevel.Walls;
                    foreach(Wall wall in walls) {
                        if(wall.Hitbox.Intersects(player.Hitbox)) {
                            player.Die();
                        }
                    }
                }
                else if(facing == Direction.Down && player.Y < this.Y + this.height && player.Y  > this.Y + this.height / 2.0f 
                    && player.X >= this.X && player.X + player.Width <= this.X + this.width) {
                    // player in bottom
                    player.Y = this.Y + this.height;

                    // kill if pushed into a wall
                    List<Wall> walls = Game1.Game.CurrentLevel.Walls;
                    foreach(Wall wall in walls) {
                        if(wall.Hitbox.Intersects(player.Hitbox)) {
                            player.Die();
                        }
                    }
                }
            }
          
            // shift tars
            foreach(Entity clinger in clingers) {
                clinger.X += shift.X;
                clinger.Y += shift.Y;
            }

            PlayerColliding = false; // reset for next frame's check
        }

        public override void Draw(SpriteBatch sb) {
            // draw track
            Vector2 drawStart = start;
            Vector2 drawEnd = end;
            if(start.X > end.X || start.Y > end.Y) {
                drawStart = end;
                drawEnd = start;
            }

            if(start.X == end.X) { // vertical
                float length = drawEnd.Y - drawStart.Y;

                // draw ends
                sb.Draw(Game1.GearboxMount, new Rectangle((int)(50 + drawStart.X - 25 - Camera.GetOffset().X), (int)(drawStart.Y - 25 - Camera.GetOffset().Y), 50, 50), new Rectangle(0, 0, 50, 50), Color.White, (float)Math.PI / 2, Vector2.Zero, SpriteEffects.None, 0f);
                sb.Draw(Game1.GearboxMount, new Rectangle((int)(50 + drawStart.X - 25 - Camera.GetOffset().X), (int)(drawStart.Y + length - 25 - Camera.GetOffset().Y), 50, 50), new Rectangle(100, 0, 50, 50), Color.White, (float)Math.PI / 2, Vector2.Zero, SpriteEffects.None, 0f);

                // draw middles
                for(int y = 50; y <= length - 50; y += 50) {
                    sb.Draw(Game1.GearboxMount, new Rectangle((int)(50 + drawStart.X - 25 - Camera.GetOffset().X), (int)(drawStart.Y + y - 25 - Camera.GetOffset().Y), 50, 50), new Rectangle(50, 0, 50, 50), Color.White, (float)Math.PI / 2, Vector2.Zero, SpriteEffects.None, 0f);
                }
            }
            else if(start.Y == end.Y) { // horizontal
                float length = drawEnd.X - drawStart.X;

                // draw ends
                sb.Draw(Game1.GearboxMount, new Rectangle((int)(drawStart.X - 25 - Camera.GetOffset().X), (int)(drawStart.Y - 25 - Camera.GetOffset().Y), 50, 50), new Rectangle(0, 0, 50, 50), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                sb.Draw(Game1.GearboxMount, new Rectangle((int)(drawStart.X + length - 25 - Camera.GetOffset().X), (int)(drawStart.Y - 25 - Camera.GetOffset().Y), 50, 50), new Rectangle(100, 0, 50, 50), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

                // draw middles
                for(int x = 50; x <= length - 50; x += 50) {
                    sb.Draw(Game1.GearboxMount, new Rectangle((int)(drawStart.X + x - 25 - Camera.GetOffset().X), (int)(drawStart.Y - 25 - Camera.GetOffset().Y), 50, 50), new Rectangle(50, 0, 50, 50), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                }
            }

            // draw wall
            Texture2D sprite = animation.GetNext();
            for(int i = 0; i < numBlocks; i++) {
                if(start.X == end.X) { // vertical
                    sb.Draw(sprite, new Rectangle(DrawBox.X, DrawBox.Y + 2 * HALF_LENGTH * i, 2 * HALF_LENGTH, 2 * HALF_LENGTH), Color.White);
                }
                else if(start.Y == end.Y) { // horizontal
                    sb.Draw(sprite, new Rectangle(DrawBox.X + 2 * HALF_LENGTH * i, DrawBox.Y, 2 * HALF_LENGTH, 2 * HALF_LENGTH), Color.White);
                }
            }
        }

        public override void Restart() {
            base.Restart();
            if(facing == Direction.Left) {
                facing = Direction.Right;
            }
            else if(facing == Direction.Up) {
                facing = Direction.Down;
            }
            firstUpdate = true;
        }

        // for nice animation, allow spit to stick to this as it dissolves
        public void AddSpit(Spit spit) {
            clingers.Add(spit);
        }

        // allows the level editor to add extra blocks to this track
        public void AddBlock() {
            numBlocks++;
            PlaceBlocks();
        }

        // Sets up the blocks in the right place on the track based on the number of blocks
        private void PlaceBlocks() {
            if(start.X == end.X) { // vertical
                height = 2 * HALF_LENGTH * numBlocks;
                if(end.Y < start.Y) { // upside down
                    position.Y = start.Y - height + HALF_LENGTH;
                }              
            }
            else if(start.Y == end.Y) { // horizontal
                width = 2 * HALF_LENGTH * numBlocks;
                if(end.X < start.X) { // backwards
                    position.X = start.X - width + HALF_LENGTH;
                }  
            }

            startX = (int)position.X;
            startY = (int)position.Y;
        }

        // tells the level editor if the mouse clicked in the area around the track
        public bool TrackAreaClicked(Vector2 mouse) {
            Rectangle trackZone;
            if(end.X < start.X || end.Y < start.Y) {
                trackZone = new Rectangle((int)end.X - HALF_LENGTH, (int)end.Y - HALF_LENGTH, (int)(start.X - end.X) + 2 * HALF_LENGTH, (int)(start.Y - end.Y) + 2 * HALF_LENGTH);
            } else {
                trackZone = new Rectangle((int)start.X - HALF_LENGTH, (int)start.Y - HALF_LENGTH, (int)(end.X - start.X) + 2 * HALF_LENGTH, (int)(end.Y - start.Y) + 2 * HALF_LENGTH);
            }

            return trackZone.Contains(mouse);
        }
    }
}
