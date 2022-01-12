using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LabGame
{
    public class Level
    {
        private List<Wall> walls;
        private List<Platform> platforms;
        private List<Entity> entities;
        private Stack<Entity> entityQueue; // entities to add when no longer updating

        // Properties
        public int Width { get; set; }
        public int Height { get; set; }
        public String Name { get; set; }
        public Type StartForm { get; set; }
        public int StartY { get; set; }
        public bool IsCustom { get; set; }

        public List<Wall> Walls { get { return walls; } }
        public List<Platform> Platforms { get { return platforms; } }
        public List<Entity> Entities { get { return entities; } } // Enemies, chemicals, coatings, etc.

        // Constructors
        public Level() {
            // make new empty in level editor
            Width = 1200;
            Height = 900;
            StartY = 500;
            StartForm = Type.Bouncy;
            walls = new List<Wall>();
            platforms = new List<Platform>();
            entities = new List<Entity>();
            entityQueue = new Stack<Entity>();
            IsCustom = true;
            // don't need to call Start() because no entities yet
        }

        public Level(String name, bool isCustom) {
            // load a level file by name
            IsCustom = isCustom;
            walls = new List<Wall>();
            platforms = new List<Platform>();
            entities = new List<Entity>();
            entityQueue = new Stack<Entity>();
            Name = name;

            String extension = "";
            if(isCustom) {
                extension = "Custom" + Game1.GetOsDelimeter();
            }

            BinaryReader input = null; 
            try {
                input = new BinaryReader(File.OpenRead($"Content{Game1.GetOsDelimeter()}Levels{Game1.GetOsDelimeter()}" + extension + name + ".lvl"));
                Width = input.ReadInt32();
                Height = input.ReadInt32();
                StartY = input.ReadInt32();
                StartForm = (Type)input.ReadByte();

                int numEntities = input.ReadInt32();
                for(int i = 0; i < numEntities; i++) {
                    EntityType type = (EntityType)input.ReadByte();
                    switch(type) {
                        case EntityType.Wall:
                            walls.Add(new Wall(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32()));
                            break;
                        case EntityType.Platform:
                            platforms.Add(new Platform(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32()));
                            break;
                        case EntityType.Slippery:
                            entities.Add(new Chemical(new Vector2(input.ReadInt32(), input.ReadInt32()), (Type)input.ReadByte()));
                            break;
                        case EntityType.Creeper:
                            entities.Add(new Creeper(input.ReadInt32(), input.ReadInt32(), (Direction)input.ReadByte()));
                            break;
                        case EntityType.Tar:
                            entities.Add(new Tar(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), (Direction)input.ReadByte()));
                            break;
                        case EntityType.Spitter:
                            entities.Add(new Spitter(input.ReadInt32(), input.ReadInt32(), (Direction)input.ReadByte(), input.ReadBoolean()));
                            break;
                        case EntityType.CrumbleWall:
                            walls.Add(new CrumbleWall(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32()));
                            break;
                        case EntityType.MovingWall:
                            walls.Add(new MovingWall(new Vector2(input.ReadInt32(), input.ReadInt32()), new Vector2(input.ReadInt32(), input.ReadInt32()), input.ReadInt32()));
                            break;
                        case EntityType.Trapper:
                            entities.Add(new Trapper(input.ReadInt32(), input.ReadInt32(), (Direction)input.ReadByte()));
                            break;
                    }
                }
            }
            catch(Exception e) {
                // make an empty level with just a floor instead
                Width = 1200;
                Height = 900;
                StartY = 650;
                StartForm = Type.Bouncy;
                walls.Add(new Wall(0, 700, 1200, 200));
                walls.Add(new Wall(0, 0, 1200, 200));
            }
            finally {
                if(input != null) {
                    input.Close();
                }
            }

            walls.Sort(); // order walls top to bottom to avoid crease bugs

            // manually add button signs and decorations to levels that need them
            if(Game1.Game.GameState == GameState.Game) {
                if(name == "Broken Glass") {
                    entities.Insert(0, new Decoration(100, 550, 200, 200, Game1.SpawnTube)); // insert so it draws behind
                    entities.Insert(0, new Decoration(400, 550, 100, 100, Game1.ControllerSign));
                    entities.Add(new ButtonSign(400, 800, new Inputs[] {Inputs.Left, Inputs.Right})); // left right
                    entities.Add(new ButtonSign(775, 650, new Inputs[] {Inputs.Jump})); // jump
                    entities.Add(new ButtonSign(1300, 500, new Inputs[] {Inputs.Down, Inputs.Jump}, '+')); // platform drop
                }
                else if(name == "How to Bounce") {
                    entities.Insert(0, new Decoration(1300, 1400, 100, 100, Game1.BounceSign));
                    entities.Add(new ButtonSign(150, 1400, new Inputs[] { Inputs.Jump, Inputs.Jump }, '>')); // double jump
                    entities.Add(new ButtonSign(575, 975, new Inputs[] { Inputs.Right, Inputs.Jump }, '+')); // wall jump
                    entities.Add(new ButtonSign(1325, 1575, new Inputs[] { Inputs.Ability })); // bounce
                }
                else if(name == "Dropdown") {
                    entities.Add(new ButtonSign(300, 1700, new Inputs[] { Inputs.Left, Inputs.Ability }, '+')); // platform drop
                }
                else if(name == "How to Slip") {
                    entities.Add(new ButtonSign(1300, 525, new Inputs[] { Inputs.Ability })); // dash
                    entities.Add(new ButtonSign(1650, 675, new Inputs[] { Inputs.Right, Inputs.Jump }, '+')); // wall climb
                    entities.Add(new ButtonSign(1950, 325, new Inputs[] { Inputs.Ability, Inputs.Down }, '>')); // slide
                }
                else if(name == "Slip and Slide") {
                    entities.Add(new ButtonSign(1800, 375, new Inputs[] { Inputs.Ability, Inputs.Jump, Inputs.Ability }, '>')); // launch jump
                    entities.Insert(0, new Decoration(2300, 300, 200, 200, Game1.JumpCancelSign));
                }
                else if(name == "Spitter Hall") {
                    entities.Add(new ButtonSign(4925, 225, new Inputs[] {Inputs.Down, Inputs.Ability}, '+')); // fast fall
                }
                else if(name == "How to Stick") {
                    entities.Add(new ButtonSign(1250, 800, new Inputs[] { Inputs.Ability })); // grapple
                    entities.Add(new ButtonSign(2150, 525, new Inputs[] { Inputs.Jump })); // glide
                    entities.Add(new ButtonSign(2875, 750, new Inputs[] { Inputs.Ability }, '+')); // swing
                    entities.Insert(0, new Decoration(2825, 250, 150, 150, Game1.SwingSign));
                }
                else if(name == "Hallway") {
                    // stop music for level right before end
                    Music.Queue(Songs.None);
                }
                else if(name == "The Monstrosity") {
                    // add final boss to last level
                    entities.Add(new Monstrosity());
                }
            }

            Start();
        }
  
        public Level(int width, int height, int startY, Type startForm, List<Wall> walls, List<Platform> platforms, List<Entity> entities) {
            // hard code a level. I don't think this is used anymore
            Width = width;
            Height = height;
            this.platforms = platforms;
            this.entities = entities;
            StartY = startY;
            StartForm = startForm;

            // Order walls top to bottom to avoid problems with being pinched.
            walls.Sort();
            this.walls = walls;
            IsCustom = false;
            entityQueue = new Stack<Entity>();

            Start();
        }

        public void Update() {
            foreach(Wall wall in walls) {
                wall.Update();
            }

            foreach(Entity entity in entities) {
                entity.Update();
            }

            // add new projectiles
            while(entityQueue.Count > 0) {
                entities.Add(entityQueue.Pop());
            }

            // clear dead projectiles
            for(int i = 0; i < entities.Count; i++) {
                if(entities[i] is Spit && !((Spit)entities[i]).IsActive) {
                    entities.RemoveAt(i);
                    i--;
                }
            }

            if(!Game1.Game.Player.Alive) {
                Restart();
            }
        }

        public void Draw(SpriteBatch sb) {
            // Midground
            foreach(Platform platform in platforms) {
                platform.Draw(sb);
            }

            // Main
            foreach(Wall wall in walls) {
                wall.Draw(sb);
            }

            // Foreground
            foreach(Entity entity in entities) { // Coatings must go over walls
                entity.Draw(sb);
            }
        }

        // puts the level back to its initial condition
        public void Restart() {
            // clear projectiles
            for(int i = 0; i < entities.Count; i++) {
                if(entities[i] is Spit) {
                    entities.RemoveAt(i);
                    i--;
                }
            }

            foreach(Wall wall in walls) {
                wall.Restart();
            }
            foreach(Entity entity in entities) {
                entity.Restart();
            }
            Game1.Game.Player.Restart();

            Game1.Game.Player.Y = StartY;
            Game1.Game.Player.X = 0;
            Game1.Game.Player.SetForm(StartForm);

            if(Game1.Game.GameState != GameState.Editor) {
                Camera.Update(this);
            }
        }

        public void Remove(Entity entity) {
            if(entity is Wall) {
                walls.Remove((Wall)entity);
            }
            else if(entity is Platform) {
                platforms.Remove((Platform)entity);
            }
            else { 
                entities.Remove(entity);
            }
        }

        public void Add(Entity entity) {
            if(entity is Wall) {
                walls.Add((Wall)entity);
            }
            else if(entity is Platform) {
                platforms.Add((Platform)entity);
            }
            else { 
                entities.Add(entity);
            }
        }

        // saves the level to a file, returns false if save fails
        public bool Save() {
            BinaryWriter output = null;
            bool savedSuccesfull = true;
            try {
                output = new BinaryWriter(File.OpenWrite($"Content{Game1.GetOsDelimeter()}Levels{Game1.GetOsDelimeter()}Custom{Game1.GetOsDelimeter()}" + Name + ".lvl"));

                // start conditions
                output.Write(Width);
                output.Write(Height);
                output.Write(StartY);
                output.Write((byte)StartForm);

                // entities
                output.Write(walls.Count + platforms.Count + entities.Count); // number of entities
                foreach(Wall wall in walls) {
                    if(wall is CrumbleWall) {
                        output.Write((byte)EntityType.CrumbleWall);
                        output.Write((int)wall.X);
                        output.Write((int)wall.Y);
                        output.Write((int)wall.Width);
                        output.Write((int)wall.Height);
                    }
                    else if(wall is MovingWall) {
                        MovingWall movingWall = (MovingWall)wall;
                        Vector2 start = movingWall.Start;
                        Vector2 end = movingWall.End;
                        output.Write((byte)EntityType.MovingWall);
                        output.Write((int)start.X);
                        output.Write((int)start.Y);
                        output.Write((int)end.X);
                        output.Write((int)end.Y);
                        output.Write(movingWall.NumberBlocks);
                    }
                    else {
                        output.Write((byte)EntityType.Wall);
                        output.Write((int)wall.X);
                        output.Write((int)wall.Y);
                        output.Write((int)wall.Width);
                        output.Write((int)wall.Height);
                    }
                }

                foreach(Platform platform in platforms) {
                    output.Write((byte)EntityType.Platform);
                    output.Write((int)platform.X);
                    output.Write((int)platform.Y);
                    output.Write((int)platform.Width);
                    output.Write((int)platform.Height);
                }

                foreach(Entity entity in entities) {
                    if(entity is Chemical) {
                        Chemical chemical = (Chemical)entity;
                        output.Write((byte)EntityType.Slippery);
                        output.Write((int)chemical.X);
                        output.Write((int)chemical.Y);
                        output.Write((byte)chemical.StartType);
                    }
                    else if(entity is Creeper) {
                        Creeper creeper = (Creeper)entity;
                        output.Write((byte)EntityType.Creeper);
                        output.Write((int)creeper.X);
                        output.Write((int)creeper.Y);
                        output.Write((byte)creeper.Side);
                    }
                    else if(entity is Tar) {
                        Tar tar = (Tar)entity;
                        output.Write((byte)EntityType.Tar);
                        output.Write((int)tar.X);
                        output.Write((int)tar.Y);
                        output.Write((int)tar.Width);
                        output.Write((int)tar.Height);
                        output.Write((byte)tar.Side);
                    }
                    else if(entity is Spitter) {
                        Spitter spitter = (Spitter)entity;
                        output.Write((byte)EntityType.Spitter);
                        output.Write((int)spitter.X);
                        output.Write((int)spitter.Y);
                        output.Write((byte)spitter.Facing);
                        output.Write(spitter.Offset);
                    }
                    else if(entity is Trapper) {
                        Trapper trapper = (Trapper)entity;
                        output.Write((byte)EntityType.Trapper);
                        output.Write((int)trapper.X);
                        output.Write((int)trapper.Y);
                        output.Write((byte)trapper.WallSide);
                    }
                }
            }
            catch(Exception e) {
                savedSuccesfull = false;
            }
            finally {
                if(output != null) {
                    output.Close();
                }
            }

            return savedSuccesfull;
        }

        public void AddProjectile(Entity entity) {
            entityQueue.Push(entity);
        }

        // allows the entities to make decisions based on what other entities exist in the level. Called after all entities are loaded
        private void Start() {
            foreach(Wall wall in walls) {
                wall.Start(walls, entities);
            }

            foreach(Entity entity in entities) {
                entity.Start(walls, entities);
            }
        }
    }
}
