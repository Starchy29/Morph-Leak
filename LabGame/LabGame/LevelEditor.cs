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
    enum EntityType {
        None,
        Eraser,
        Wall,
        Slippery,
        Sticky,
        Bouncy,
        Platform,
        Tar,
        Creeper,
        Spitter,
        CrumbleWall,
        MovingWall,
        Trapper
    }

    public static class LevelEditor
    {
        private static Level level;
        public static Level Level { set { level = value; } }
        private static EntityType selected;
        private static Button hovered;
        private static Wall referenceWall; // used for placing coatings
        private static Vector2 startPosition = new Vector2(-1, -1); // used for 2 part placement
        private static List<Button> buttons;
        private static String message = "Nothing Selected";
        private static Vector2 lastMouse = new Vector2(); // used for moving the level view
        private static Vector2 mouseStart = new Vector2(); // don't interact with level if just scrolling
        private static bool typing = false;

        public static void Update() {
            if(!Game1.Game.IsActive) {
                return;
            }

            if(typing) {
                String pressedKey = Input.GetLetter();
                if(pressedKey != "" && message == "Type a name, then hit enter") {
                    message = "";
                    message += pressedKey;
                }
                else if(message.Length > 0 && Input.JustPressed(Keys.Back)) {
                    message = message.Substring(0, message.Length - 1);
                }
                else if(Keyboard.GetState().IsKeyDown(Keys.Enter)) {
                    message.Trim();
                    level.Name = message;
                    Select(EntityType.None);
                    level.Save();
                    message = "Level Saved";
                    typing = false;
                }
                else {
                    message += pressedKey;
                }
                return;
            }

            hovered = null;
            Vector2 mouse = Input.GetMousePos(); // screen coordinates

            foreach(Button button in buttons) {
                if(button.IsMouseHovering(mouse)) {
                    hovered = button;
                }
            }

            if(Input.MouseJustClicked()) {
                mouseStart = mouse;
                
                if(hovered != null) {
                    hovered.Click();
                }
            }
            if(!Input.IsMouseClicked()) {
                if(mouse.X > 400 && (mouseStart - mouse).Length() <= 5) { // if unclicked and didn't move from start click
                    // click grid
                    Vector2 gridLocation = GetGridLocation(mouse); // grid coordinates
                    Vector2 gridLock = new Vector2(lockToGrid(gridLocation.X), lockToGrid(gridLocation.Y)); // grid coordinates locked to top left of square
                    List<Wall> walls = level.Walls;

                    if(gridLocation.X > 0) { // if clicked on grid
                        if(selected != EntityType.Eraser) {
                            // modify existing entities first
                            foreach(Entity entity in level.Entities) {
                                if(entity.Intersects(gridLocation)) { // clicked
                                    if(entity is Spitter) {
                                        Spitter spitter = (Spitter)entity;
                                        spitter.Offset = !spitter.Offset;
                                        level.Restart();
                                        Select(EntityType.None); // don't place selected entity
                                        break;
                                    }
                                }
                            }
                        }

                        switch(selected) {
                            // single select
                            case EntityType.Slippery:
                                level.Add(new Chemical(lockToGrid(gridLocation.X) + 25, lockToGrid(gridLocation.Y) + 25, Type.Slippery)); // chemicals place by middle
                                break;
                            case EntityType.Sticky:
                                level.Add(new Chemical(lockToGrid(gridLocation.X) + 25, lockToGrid(gridLocation.Y) + 25, Type.Sticky));
                                break;
                            case EntityType.Bouncy:
                                level.Add(new Chemical(lockToGrid(gridLocation.X) + 25, lockToGrid(gridLocation.Y) + 25, Type.Bouncy));
                                break;
                            case EntityType.Creeper:
                                // make sure edge of wall is selected
                                foreach(Wall wall in walls) {
                                    if(wall.Intersects(gridLocation)) {
                                        if(gridLock.Y == wall.Y) { // top
                                            level.Add(new Creeper(lockToGrid(gridLocation.X), lockToGrid(gridLocation.Y) - 75, Direction.Up));
                                        }
                                        if(gridLock.X == wall.X) { // left
                                            level.Add(new Creeper(lockToGrid(gridLocation.X) - 75, lockToGrid(gridLocation.Y), Direction.Left));
                                        }
                                        if(gridLock.X == wall.X + wall.Width - 50) { // right
                                            level.Add(new Creeper(lockToGrid(gridLocation.X) + 50, lockToGrid(gridLocation.Y), Direction.Right));
                                        }
                                        if(gridLock.Y == wall.Y + wall.Height - 50) { // bottom
                                            level.Add(new Creeper(lockToGrid(gridLocation.X), lockToGrid(gridLocation.Y) + 50, Direction.Down));
                                        }
                                    }
                                }
                                break;
                            case EntityType.Spitter:
                                // make sure edge of wall is selected
                                foreach(Wall wall in walls) {
                                    if(wall.Intersects(gridLocation)) {
                                        if(gridLock.Y == wall.Y) { // top
                                            level.Add(new Spitter(lockToGrid(gridLocation.X) - (Spitter.LENGTH - 50) / 2, lockToGrid(gridLocation.Y) - Spitter.LENGTH, Direction.Up, false));
                                        }
                                        if(gridLock.X == wall.X) { // left
                                            level.Add(new Spitter(lockToGrid(gridLocation.X) - Spitter.LENGTH, lockToGrid(gridLocation.Y) - (Spitter.LENGTH - 50) / 2, Direction.Left, false));
                                        }
                                        if(gridLock.X == wall.X + wall.Width - 50) { // right
                                            level.Add(new Spitter(lockToGrid(gridLocation.X) + 50, lockToGrid(gridLocation.Y) - (Spitter.LENGTH - 50) / 2, Direction.Right, false));
                                        }
                                        if(gridLock.Y == wall.Y + wall.Height - 50) { // bottom
                                            level.Add(new Spitter(lockToGrid(gridLocation.X) - (Spitter.LENGTH - 50) / 2, lockToGrid(gridLocation.Y) + 50, Direction.Down, false));
                                        }
                                    }
                                }
                                break;

                            case EntityType.Trapper:
                                // make sure edge of wall is selected
                                foreach(Wall wall in walls) {
                                    if(wall.Intersects(gridLocation)) {
                                        if(gridLock.Y == wall.Y) { // top
                                            level.Add(new Trapper(lockToGrid(gridLocation.X) - (Trapper.LENGTH - 50) / 2, lockToGrid(gridLocation.Y) - Trapper.LENGTH, Direction.Down));
                                        }
                                        if(gridLock.X == wall.X) { // left
                                            level.Add(new Trapper(lockToGrid(gridLocation.X) - Trapper.LENGTH, lockToGrid(gridLocation.Y) - (Trapper.LENGTH - 50) / 2, Direction.Right));
                                        }
                                        if(gridLock.X == wall.X + wall.Width - 50) { // right
                                            level.Add(new Trapper(lockToGrid(gridLocation.X) + 50, lockToGrid(gridLocation.Y) - (Trapper.LENGTH - 50) / 2, Direction.Left));
                                        }
                                        if(gridLock.Y == wall.Y + wall.Height - 50) { // bottom
                                            level.Add(new Trapper(lockToGrid(gridLocation.X) - (Trapper.LENGTH - 50) / 2, lockToGrid(gridLocation.Y) + 50, Direction.Up));
                                        }
                                    }
                                }
                                break;

                            // double select
                            case EntityType.Wall:
                                if(startPosition.X < 0) {
                                    startPosition = new Vector2(lockToGrid(gridLocation.X), lockToGrid(gridLocation.Y));
                                    message = "Place end below start";
                                } 
                                else if(gridLocation.Y >= startPosition.Y && gridLocation.X >= startPosition.X) {
                                    level.Add(new Wall((int)startPosition.X, (int)startPosition.Y, lockToGrid(gridLocation.X) - (int)startPosition.X + 50, lockToGrid(gridLocation.Y) - (int)startPosition.Y + 50));
                                    Select(EntityType.Wall);
                                }
                                break;
                            case EntityType.MovingWall:
                                // check for adding a block to an existing track first
                                bool addedBlock = false;
                                foreach(Wall wall in walls) {
                                    if(wall is MovingWall && ((MovingWall)wall).TrackAreaClicked(gridLocation)) {
                                        ((MovingWall)wall).AddBlock();
                                        addedBlock = true;
                                        break;
                                    }
                                }
                                if(addedBlock) {
                                    break;
                                }

                                if(startPosition.X < 0) {
                                    startPosition = new Vector2(lockToGrid(gridLocation.X), lockToGrid(gridLocation.Y));
                                    message = "Place end in a line";
                                } 
                                else if(lockToGrid(gridLocation.Y) == startPosition.Y || lockToGrid(gridLocation.X) == startPosition.X) {
                                    level.Add(new MovingWall(new Vector2(startPosition.X + 25, startPosition.Y + 25), new Vector2(lockToGrid(gridLocation.X) + 25, lockToGrid(gridLocation.Y) + 25)));
                                    Select(EntityType.MovingWall);
                                }
                                break;
                            case EntityType.CrumbleWall:
                                if(startPosition.X < 0) {
                                    startPosition = new Vector2(lockToGrid(gridLocation.X), lockToGrid(gridLocation.Y));
                                    message = "Place end below start";
                                } 
                                else if(gridLocation.Y >= startPosition.Y && gridLocation.X >= startPosition.X) {
                                    level.Add(new CrumbleWall((int)startPosition.X, (int)startPosition.Y, lockToGrid(gridLocation.X) - (int)startPosition.X + 50, lockToGrid(gridLocation.Y) - (int)startPosition.Y + 50));
                                    Select(EntityType.CrumbleWall);
                                }
                                break;
                            case EntityType.Platform:
                                if(startPosition.X < 0) {
                                    startPosition = new Vector2(lockToGrid(gridLocation.X), lockToGrid(gridLocation.Y));
                                    message = "Place end below start";
                                } 
                                else if(gridLocation.Y >= startPosition.Y && gridLocation.X >= startPosition.X) {
                                    level.Add(new Platform((int)startPosition.X, (int)startPosition.Y, lockToGrid(gridLocation.X) - (int)startPosition.X + 50, lockToGrid(gridLocation.Y) - (int)startPosition.Y + 50));
                                    Select(EntityType.Platform);
                                }
                                break;
                            case EntityType.Tar:
                                if(startPosition.X < 0) { // first selection
                                    // make sure edge of wall is selected
                                    foreach(Wall wall in walls) {
                                        if(wall.Intersects(gridLocation) && (gridLock.X == wall.X || gridLock.Y == wall.Y
                                            || gridLock.X == wall.X + wall.Width - 50 || gridLock.Y == wall.Y + wall.Height - 50)
                                        ) {
                                            startPosition = gridLock;
                                            referenceWall = wall;
                                            message = "Place tar end";
                                            break;
                                        }
                                    }                            
                                } 
                                else if(referenceWall.Intersects(gridLocation) && gridLocation.Y >= startPosition.Y && gridLocation.X >= startPosition.X
                                    && (gridLock.X == startPosition.X || gridLock.Y == startPosition.Y)
                                ) {
                                    if(gridLock.X == startPosition.X && gridLock.Y == startPosition.Y) { // same block: make one on each side
                                        if(gridLock.X == referenceWall.X) {
                                            level.Add(new Tar((int)startPosition.X, (int)startPosition.Y, (int)(gridLock.Y + 50 - startPosition.Y), Direction.Left));
                                        }
                                        if(gridLock.X == referenceWall.X + referenceWall.Width - 50) {
                                            level.Add(new Tar((int)startPosition.X + 25, (int)startPosition.Y, (int)(gridLock.Y + 50 - startPosition.Y), Direction.Right));
                                        }
                                        if(gridLock.Y == referenceWall.Y) {
                                            level.Add(new Tar((int)startPosition.X, (int)startPosition.Y, (int)(gridLock.X + 50 - startPosition.X), Direction.Up));
                                        }
                                        if(gridLock.Y == referenceWall.Y + referenceWall.Height - 50) {
                                            level.Add(new Tar((int)startPosition.X, (int)startPosition.Y + 25, (int)(gridLock.X + 50 - startPosition.X), Direction.Down));
                                        }
                                    }
                                    else if(gridLock.X == startPosition.X) { // vertical
                                        if(gridLock.X == referenceWall.X) { // left
                                            level.Add(new Tar((int)startPosition.X, (int)startPosition.Y, (int)(gridLock.Y + 50 - startPosition.Y), Direction.Left));
                                        } else { // right
                                            level.Add(new Tar((int)startPosition.X + 25, (int)startPosition.Y, (int)(gridLock.Y + 50 - startPosition.Y), Direction.Right));
                                        }
                                    } else { // horizontal
                                        if(gridLock.Y == referenceWall.Y) { // top
                                            level.Add(new Tar((int)startPosition.X, (int)startPosition.Y, (int)(gridLock.X + 50 - startPosition.X), Direction.Up));
                                        } else { // bottom
                                            level.Add(new Tar((int)startPosition.X, (int)startPosition.Y + 25, (int)(gridLock.X + 50 - startPosition.X), Direction.Down));
                                        }
                                    }
                                    Select(EntityType.Tar);
                                }
                                break;

                            // unique
                            case EntityType.Eraser:
                                bool erased = false;
                                List<Entity> entities = level.Entities;
                                foreach(Entity entity in entities) {
                                    if(entity.Intersects(gridLocation)) {
                                        level.Remove(entity);
                                        erased = true;
                                        break;
                                    }
                                }
                                if(erased) break;
                                List<Platform> platforms = level.Platforms;
                                foreach(Platform platform in platforms) {
                                    if(platform.Intersects(gridLocation)) {
                                        level.Remove(platform);
                                        erased = true;
                                        break;
                                    }
                                }
                                if(erased) break;
                                foreach(Wall wall in walls) {
                                    if(wall.Intersects(gridLocation)) {
                                        level.Remove(wall);
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                }

                mouseStart = new Vector2(-1000, -1000);
            }

            // drag screen
            if(Input.IsMouseClicked()) {
                if(lastMouse == Vector2.Zero) {
                    lastMouse = Input.GetMousePos();
                }
                Camera.Translate(lastMouse - Input.GetMousePos(), level);
                lastMouse = Input.GetMousePos();
            } else {
                lastMouse = Vector2.Zero;
            }
        }

        public static void Draw(SpriteBatch sb) {
            level.Draw(sb);
            Game1.Game.Player.Draw(sb);

            // draw grid;
            Vector2 offset = Camera.GetOffset();
            const int blockDim = 50;
            for(int x = 0; x <= level.Width; x += blockDim) { // columns
                sb.Draw(Game1.Pixel, new Rectangle((int)-offset.X + x - 1, (int)-offset.Y, 2, level.Height), Color.Gray);
            }
            for(int y = 0; y <= level.Height; y += blockDim) { // rows
                sb.Draw(Game1.Pixel, new Rectangle((int)-offset.X, (int)-offset.Y + y - 1, level.Width, 2), Color.Gray);
            }

            if(startPosition.X >= 0) {
                sb.Draw(Game1.Pixel, new Rectangle((int)(startPosition.X - offset.X), (int)(startPosition.Y - offset.Y), blockDim, blockDim), Color.LightBlue);
            }

            // draw UI
            sb.Draw(Game1.Pixel, new Rectangle(0, 0, 400, Game1.StartScreenHeight), Color.FromNonPremultiplied(80, 80, 80, 180)); // background
            sb.Draw(Game1.Pixel, new Rectangle(98, 560, 4, 70), Color.Gray); // cross vert
            sb.Draw(Game1.Pixel, new Rectangle(65, 592, 70, 4), Color.Gray); // cross hori

            foreach (Button button in buttons) {
                button.Draw(sb, hovered == button);
            }

            Vector2 textDims = Game1.Font.MeasureString(message);
            sb.DrawString(Game1.Font, message, new Vector2(200 - textDims.X / 2, 10 + textDims.Y), Color.White);
        }

        // get the location of the grid that is clicked
        private static Vector2 GetGridLocation(Vector2 screenCoords) {
            Vector2 newPosition = screenCoords + Camera.GetOffset();
            if(newPosition.X < 0 || newPosition.Y < 0 || newPosition.X > level.Width || newPosition.Y > level.Height) {
                // out of bounds: return "null"
                return new Vector2(-1, -1);
            }
            return newPosition;
        }

        private static int lockToGrid(float value) {
            return (int)value / 50 * 50;
        }

        // called at the beginning of the program
        public static void SetupUI() {
            buttons = new List<Button>();

            buttons.Add(new Button(100, 700, 200, 50, "Playtest", () => { // playtest
                Game1.Game.GameState = GameState.Game; 
                Game1.Game.CurrentLevel = level;
                //Music.Play(Songs.Game);
            }));
            buttons.Add(new Button(100, 760, 200, 50, "Save", () => { // save
                if(level.Name == null) {
                    // set new name
                    typing = true;
                    message = "Type a name, then hit enter";
                } else {
                    Select(EntityType.None);
                    if(level.Save()) { // false means save failed
                        message = "Level Saved";
                    } else {
                        message = "Level save failed.";
                    }
                }
            }));
            buttons.Add(new Button(100, 820, 200, 50, "Back", () => { Game1.Game.GameState = GameState.Menu; Game1.Game.CurrentMenu = Game1.Game.LevelEditorSelect; Music.Play(Songs.Menu); })); // back

            buttons.Add(new Button(15, 570, 50, 50, "-", () => { if(level.Width >= Game1.StartScreenWidth + 50) level.Width -= 50; })); // shrink x
            buttons.Add(new Button(135, 570, 50, 50, "+", () => { level.Width += 50; })); // grow x
            buttons.Add(new Button(215, 570, 50, 50, Game1.Pixel, () => { level.StartForm = Type.Slippery; Game1.Game.Player.SetForm(Type.Slippery); }, Chemical.ColorOf(Type.Slippery))); // start slippery
            buttons.Add(new Button(275, 570, 50, 50, Game1.Pixel, () => { level.StartForm = Type.Bouncy; Game1.Game.Player.SetForm(Type.Bouncy); }, Chemical.ColorOf(Type.Bouncy))); // start bouncy
            buttons.Add(new Button(335, 570, 50, 50, Game1.Pixel, () => { level.StartForm = Type.Sticky; Game1.Game.Player.SetForm(Type.Sticky); }, Chemical.ColorOf(Type.Sticky))); // start sticky

            buttons.Add(new Button(75, 510, 50, 50, "-", () => { // shrink y
                if(level.Height >= Game1.StartScreenHeight + 50) {
                    level.Height -= 50;
                    if(Game1.Game.Player.Y >= level.Height) {
                        Game1.Game.Player.Y -= 50;
                    }
                }
            }));
            buttons.Add(new Button(75, 630, 50, 50, "+", () => { level.Height += 50; })); // grow y
            buttons.Add(new Button(275, 510, 50, 50, "^", () => { // spawn up
                if(level.StartY > 0) { 
                    level.StartY -= 50;
                    Game1.Game.Player.Y -= 50; 
                } 
            }));
            buttons.Add(new Button(275, 630, 50, 50, "v", () => { //spawn down
                if(level.StartY < level.Height - 50) {
                    level.StartY += 50;
                    Game1.Game.Player.Y += 50;
                }
            }));

            // placing entities
            const int buttonDim = 60;
            buttons.Add(new Button(70, 140, buttonDim, buttonDim, Game1.EraserLogo, () => { Select(EntityType.Eraser); }, Color.White)); // eraser
            buttons.Add(new Button(170, 140, buttonDim, buttonDim, Game1.Pixel, () => { Select(EntityType.Wall); }, Color.DarkGray)); // wall
            buttons.Add(new Button(270, 140, buttonDim, buttonDim, Game1.Pixel, () => { Select(EntityType.Tar); }, Color.Black)); // tar

            buttons.Add(new Button(70, 220, buttonDim, buttonDim, Game1.FlaskOut, () => { Select(EntityType.Slippery); }, Chemical.ColorOf(Type.Slippery))); // slippery chemical
            buttons.Add(new Button(170, 220, buttonDim, buttonDim, Game1.FlaskOut, () => { Select(EntityType.Sticky); }, Chemical.ColorOf(Type.Sticky))); // sticky chemical
            buttons.Add(new Button(270, 220, buttonDim, buttonDim, Game1.FlaskOut, () => { Select(EntityType.Bouncy); }, Chemical.ColorOf(Type.Bouncy))); // bouncy chemical
            
            buttons.Add(new Button(70, 300, buttonDim, buttonDim, Game1.CreeperLeftContact, () => { Select(EntityType.Creeper); }, Color.White)); // creeper
            buttons.Add(new Button(170, 300, buttonDim, buttonDim, Game1.SpitterIdle, () => { Select(EntityType.Spitter); }, Color.White)); // spitter
            buttons.Add(new Button(270, 300, buttonDim, buttonDim, Game1.Trapper, () => { Select(EntityType.Trapper); }, Color.White)); // trapper

            buttons.Add(new Button(70, 380, buttonDim, buttonDim, Game1.Shelf, () => { Select(EntityType.Platform); }, Color.White)); // platform
            buttons.Add(new Button(170, 380, buttonDim, buttonDim, Game1.Gearbox[0], () => { Select(EntityType.MovingWall); }, Color.White)); // moving wall
            buttons.Add(new Button(270, 380, buttonDim, buttonDim, Game1.Crumbler, () => { Select(EntityType.CrumbleWall); }, Color.White)); // crumbleWall       

            // to add a new entity to the editor, add it to the enum, add it to the buttons list, add its custom message, then add code in Update() and implement saving / loading
        }

        private static void Select(EntityType type) {
            // update selected message
            switch(type) {
                case EntityType.None:
                    message = "Nothing Selected";
                    break;
                case EntityType.Eraser:
                    message = "Eraser Selected";
                    break;
                case EntityType.Wall:
                    message = "Place wall start";
                    break;
                case EntityType.Slippery:
                    message = "Slippery Selected";
                    break;
                case EntityType.Sticky:
                    message = "Sticky Selected";
                    break;
                case EntityType.Bouncy:
                    message = "Bouncy Selected";
                    break;
                case EntityType.Platform:
                    message = "Place platform start";
                    break;
                case EntityType.Tar:
                    message = "Place tar on wall";
                    break;
                case EntityType.Creeper:
                    message = "Place creeper on wall";
                    break;
                case EntityType.Spitter:
                    message = "Place spitter on wall";
                    break;
                case EntityType.CrumbleWall:
                    message = "Crumbling Wall selected";
                    break;
                case EntityType.MovingWall:
                    message = "Moving Wall selected";
                    break;
                case EntityType.Trapper:
                    message = "Place Trapper on wall";
                    break;
            }

            selected = type;
            startPosition = new Vector2(-1, -1);
            referenceWall = null;
        }
    }
}