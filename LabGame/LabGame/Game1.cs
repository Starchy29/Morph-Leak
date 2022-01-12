using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace LabGame
{
    public enum GameState {
        Game, 
        Menu, 
        Pause,
        Editor,
        Transition,
        Cutscene
    }

    public enum Direction {
        None,
        Right,
        Left, 
        Up, 
        Down,
    }

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Sound
        private static SoundEffect menuSong;
        private static SoundEffect gameSong;
        private static SoundEffect bossSong;

        // Graphics
        public static Color OffBlack = new Color(20, 20, 20);
        public static Texture2D Pixel { get; set; }
        public static Texture2D TempPlayerTexture { get; set; }
        public static Texture2D EraserLogo { get; set; }
        public static Texture2D Ring { get; set; }
        public static Texture2D Arrow { get; set; }
        public static Texture2D TarTexture { get; set; }
        public static Texture2D FlaskOut { get; set; }
        public static Texture2D FlaskIn { get; set; }
        public static Texture2D KeyTexture { get; set; }
        public static Texture2D SpawnTube { get; set; }
        public static Texture2D GrapplePoint { get; set; }
        public static Texture2D Aimer { get; set; }
        public static Texture2D ControllerSign { get; set; }
        public static Texture2D BounceSign { get; set; }
        public static Texture2D[] Monstrosity { get; set; }
        public static Texture2D Shelf { get; set; }
        public static Texture2D[] ShelfItems { get; set; }
        public static Texture2D Floor { get; set; }
        public static Texture2D SwingSign { get; set; }
        public static Texture2D JumpCancelSign { get; set; }

        public static Texture2D GearMenu { get; set; }
        public static Texture2D VialMenu { get; set; }
        public static Texture2D MainMenu { get; set; }

        public static Texture2D PlayerTexture { get; set; }
        public static Texture2D PlayerNeutral { get; set; }
        public static Texture2D PlayerCrouch { get; set; }
        public static Texture2D PlayerSlide { get; set; }
        public static Texture2D PlayerIdle { get; set; }
        public static Texture2D[] PlayerDeath { get; set; }
        public static Texture2D[] PlayerNeutralDeath { get; set; }
        public static Texture2D[] PlayerBounce { get; set; }
        public static Texture2D PlayerWalk { get; set; }
        public static Texture2D PlayerWalkTransition { get; set; }
        public static Texture2D PlayerAirBottom { get; set; }
        public static Texture2D PlayerAirLower { get; set; }
        public static Texture2D PlayerAirMiddle { get; set; }
        public static Texture2D PlayerAirUpper { get; set; }
        public static Texture2D PlayerAirTop { get; set; }
        public static Texture2D PlayerPlummet { get; set; }
        public static Texture2D PlayerWall { get; set; }
        public static Texture2D PlayerWallDown { get; set; }
        public static Texture2D PlayerWallUp { get; set; }
        public static Texture2D PlayerHang { get; set; }
        public static Texture2D[] PlayerGlide { get; set; }

        public static Texture2D CreeperLeftContact { get; set; }
        public static Texture2D CreeperLeftShift { get; set; }
        public static Texture2D CreeperLeftPass { get; set; }
        public static Texture2D CreeperRightContact { get; set; }
        public static Texture2D CreeperRightShift { get; set; }
        public static Texture2D CreeperRightPass { get; set; }

        public static Texture2D SpitterIdle { get; set; }
        public static Texture2D Spitter1 { get; set; }
        public static Texture2D Spitter2 { get; set; }
        public static Texture2D Spitter3 { get; set; }
        public static Texture2D Spitter4 { get; set; }

        public static Texture2D SpitTexture { get; set; }
        public static Texture2D Spit1 { get; set; }
        public static Texture2D Spit2 { get; set; }
        public static Texture2D Spit3 { get; set; }
        public static Texture2D Spit4 { get; set; }
        public static Texture2D Spit5 { get; set; }

        public static Texture2D GearboxMount { get; set; }
        public static Texture2D[] Gearbox { get; set; }

        public static Texture2D TrapperTeeth { get; set; }
        public static Texture2D Trapper { get; set; }
        public static Texture2D[] TrapperBite { get; set; }

        public static Texture2D Crumbler { get; set; }
        public static Texture2D[] Cracking { get; set; }
        public static Texture2D[] Crumbling { get; set; }

        public static Texture2D LabWide { get; set; }
        public static Texture2D[] LabExplosion { get; set; }
        public static Texture2D LabInterior { get; set; }
        public static Texture2D[] WakeUp { get; set; }
        public static Texture2D[] StandUp { get; set; }

        public static Texture2D LabExit { get; set; }
        public static Texture2D Leaving { get; set; }
        public static Texture2D LabDebris { get; set; }
        public static Texture2D[] Twitching { get; set; }
        public static Texture2D[] WalkAway { get; set; }
        public static Texture2D FrontDebris { get; set; }
        public static Texture2D BackDebris { get; set; }
        public static Texture2D[] WalkAwayGuy { get; set; }
        public static Texture2D[] TurnStart { get; set; }
        public static Texture2D[] TurnEnd { get; set; }
        public static Texture2D WideView { get; set; }
        public static Texture2D[] SlideDown { get; set; }
        public static Texture2D[] RiverWalk { get; set; }
        public static Texture2D[] RiverStare { get; set; }
        public static Texture2D[] RiverEnd { get; set; }
        public static Texture2D[] Dissipate { get; set; }

        public static SpriteFont Font { get; set; }

        public const int StartScreenWidth = 1200; // do not
        public const int StartScreenHeight = 900; // change these
        private Matrix transforms = Matrix.Identity;
        private int xOffset;
        public int XOffset { get { return xOffset; } }
        private int yOffset;
        public int YOffset { get { return yOffset; } }
        private Vector2 gameDims = new Vector2(StartScreenWidth, StartScreenHeight); // dims of game window, accounts for scale but not translation
        public Vector2 GameDims { get { return gameDims; } }
        public bool Colorblind { get; set; }

        // Gameplay
        public static Game1 Game;
        private Player player;
        private String[] levels = new String[] {
            "Broken Glass", "How to Bounce", "Dropdown", "Bounce House", "Tunnel", "Rebound",
            "How to Slip", "Slip and Slide", "Speedrun", "Spitter Hall", "Through the Cracks", "Metamorphosis", "Lock and Key", "Heartbeat",
            "How to Stick", "Pendulum", "The Labyrinth", "Clockwork", "Assembly Line", "Factory Floor",
            "Push and Pull", "Steep Hill", "The Pit", "Fast and Slow", "Dark Alley", "Jump Scare",
            "Decision Paralysis", "Enhanced Abilities", "Breakdown", "Slingshot", "Scavenger Hunt", "Ascension", "Transfusion", "Hallway", "The Monstrosity" 
        };
        
        private int levelNumber;

        // Menus
        private Menu mainMenu;
        private Menu optionsMenu;
        private Menu levelEditorSelect;
        private Menu loadMenu;
        private Menu pauseMenu;
        private Menu levelSelect;
        private Menu customLevelSelect;
        private Menu[] chapters;

        public Menu LevelEditorSelect { get { return levelEditorSelect; } } // allow the level editor to go back to this menu

        // game states
        private GameState gameState = GameState.Menu;
        public GameState GameState { get { return gameState; } set { gameState = value; } }
        private bool playtesting = false; // when a player beats a custom level, level select or level editor
        private Transition currentTransition;

        // Properties
        public Player Player { get { return player; } }
        public Level CurrentLevel { get; set; }
        public Menu CurrentMenu { get; set; }
        public Cutscene CurrentCutscene { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = StartScreenWidth;
            graphics.PreferredBackBufferHeight = StartScreenHeight;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;
            Game = this;

            Colorblind = false;
            this.Window.Title = "Morph Leak";
        }

        protected override void Initialize()
        {
            base.Initialize();
            Game.IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            menuSong = Content.Load<SoundEffect>("lost in thought");
            gameSong = Content.Load<SoundEffect>("unwanted");
            bossSong = Content.Load<SoundEffect>("critical mass");
            Music.IntroSong = Content.Load<SoundEffect>("catastrophe");
            Music.EndSong = Content.Load<SoundEffect>("downhill");
            Music.MenuMusic = menuSong.CreateInstance();
            Music.MenuMusic.IsLooped = true;
            Music.GameMusic = gameSong.CreateInstance();
            Music.GameMusic.IsLooped = true;
            Music.BossMusic = bossSong.CreateInstance();
            Music.BossMusic.IsLooped = true;
            Music.Play(Songs.Menu);
            SoundEffect.MasterVolume = 0.5f;

            TempPlayerTexture = Content.Load<Texture2D>("TempPlayer");
            Pixel = Content.Load<Texture2D>("TempWall");
            Font = Content.Load<SpriteFont>("Text");
            Arrow = Content.Load<Texture2D>("arrow");
            Ring = Content.Load<Texture2D>("ring");
            EraserLogo = Content.Load<Texture2D>("eraser");
            TarTexture = Content.Load<Texture2D>("tar");
            FlaskIn = Content.Load<Texture2D>("flask in");
            FlaskOut = Content.Load<Texture2D>("flask out");
            KeyTexture = Content.Load<Texture2D>("key");
            SpawnTube = Content.Load<Texture2D>("spawn tube");
            GrapplePoint = Content.Load<Texture2D>("grapple point");
            Aimer = Content.Load<Texture2D>("aimer");
            ControllerSign = Content.Load<Texture2D>("controller");
            BounceSign = Content.Load<Texture2D>("bounce sign");
            Shelf = Content.Load<Texture2D>("shelf");
            Floor = Content.Load<Texture2D>("floor");
            SwingSign = Content.Load<Texture2D>("swing sign");
            JumpCancelSign = Content.Load<Texture2D>("jump cancel sign");

            ShelfItems = new Texture2D[3];
            ShelfItems[0] = Content.Load<Texture2D>("shelf purple");
            ShelfItems[1] = Content.Load<Texture2D>("shelf red");
            ShelfItems[2] = Content.Load<Texture2D>("shelf green");

            GearMenu = Content.Load<Texture2D>("gear menu");
            VialMenu = Content.Load<Texture2D>("vial menu");
            MainMenu = Content.Load<Texture2D>("main menu");

            PlayerTexture = Content.Load<Texture2D>("player");
            PlayerIdle = Content.Load<Texture2D>("player standard");
            PlayerCrouch = Content.Load<Texture2D>("player crouch");
            PlayerSlide = Content.Load<Texture2D>("player slide");
            PlayerWalk = Content.Load<Texture2D>("player walk");
            PlayerWalkTransition = Content.Load<Texture2D>("player walk transition");
            PlayerAirBottom = Content.Load<Texture2D>("player air bottom");
            PlayerAirLower = Content.Load<Texture2D>("player air lower");
            PlayerAirMiddle = Content.Load<Texture2D>("player air middle");
            PlayerAirUpper = Content.Load<Texture2D>("player air upper");
            PlayerAirTop = Content.Load<Texture2D>("player air top");
            PlayerPlummet = Content.Load<Texture2D>("player plummet");
            PlayerWall = Content.Load<Texture2D>("player wall");
            PlayerWallDown = Content.Load<Texture2D>("player wall down");
            PlayerWallUp = Content.Load<Texture2D>("player wall up");
            PlayerHang = Content.Load<Texture2D>("player hang");

            CreeperLeftContact = Content.Load<Texture2D>("creeper left contact");
            CreeperLeftShift = Content.Load<Texture2D>("creeper left shift");
            CreeperLeftPass = Content.Load<Texture2D>("creeper left pass");
            CreeperRightContact = Content.Load<Texture2D>("creeper right contact");
            CreeperRightShift = Content.Load<Texture2D>("creeper right shift");
            CreeperRightPass = Content.Load<Texture2D>("creeper right pass");

            SpitterIdle = Content.Load<Texture2D>("spitter idle");
            Spitter1 = Content.Load<Texture2D>("spitter 1");
            Spitter2 = Content.Load<Texture2D>("spitter 2");
            Spitter3 = Content.Load<Texture2D>("spitter 3");
            Spitter4 = Content.Load<Texture2D>("spitter 4");

            SpitTexture = Content.Load<Texture2D>("spit");
            Spit1 = Content.Load<Texture2D>("spit 1");
            Spit2 = Content.Load<Texture2D>("spit 2");
            Spit3 = Content.Load<Texture2D>("spit 3");
            Spit4 = Content.Load<Texture2D>("spit 4");
            Spit5 = Content.Load<Texture2D>("spit 5");

            PlayerGlide = new Texture2D[3];
            PlayerGlide[2] = Content.Load<Texture2D>("player glide");
            PlayerGlide[1] = Content.Load<Texture2D>("player glide between");
            PlayerGlide[0] = Content.Load<Texture2D>("player glide start");

            GearboxMount = Content.Load<Texture2D>("gearbox mount");
            Gearbox = new Texture2D[3];
            Gearbox[0] = Content.Load<Texture2D>("gearbox");
            Gearbox[1] = Content.Load<Texture2D>("gearbox shift 1");
            Gearbox[2] = Content.Load<Texture2D>("gearbox shift 2");

            TrapperTeeth = Content.Load<Texture2D>("trapperTeeth");
            Trapper = Content.Load<Texture2D>("Trapper");

            Crumbler = Content.Load<Texture2D>("crumbler");
            Cracking = new Texture2D[6];
            for(int i = 0; i < 6; i++) {
                Cracking[i] = Content.Load<Texture2D>("cracks " + (i+1));
            }

            Crumbling = new Texture2D[6];
            for(int i = 0; i < 6; i++) {
                Crumbling[i] = Content.Load<Texture2D>("crumble " + (i+1));
            }

            LabWide = Content.Load<Texture2D>("intro lab");
            LabInterior = Content.Load<Texture2D>("interior close");

            TrapperBite = new Texture2D[10];
            TrapperBite[9] = Trapper;
            for(int i = 0; i < 9; i++) {
                TrapperBite[i] = Content.Load<Texture2D>("bite " + (i+1));
            }

            PlayerDeath = new Texture2D[17];
            PlayerNeutralDeath = new Texture2D[17];
            for(int i = 0; i < 17; i++) {
                PlayerDeath[i] = Content.Load<Texture2D>("player death " + (i+1));
                PlayerNeutralDeath[i] = Content.Load<Texture2D>("neutral death " + (i+1));
            }

            Monstrosity = new Texture2D[5];
            for(int i = 0; i < 5; i++) {
                Monstrosity[i] = Content.Load<Texture2D>("monstrosity " + (i + 1));
            }

            PlayerBounce = new Texture2D[6];
            for(int i = 0; i < 6; i++) {
                PlayerBounce[i] = Content.Load<Texture2D>("bounce " + (i+1));
            }
            PlayerNeutral = PlayerBounce[1];

            LabExplosion = new Texture2D[11];
            LabExplosion[0] = null;
            for(int i = 1; i < 10; i++) {
                LabExplosion[i] = Content.Load<Texture2D>("explosion " + i);
            }
            LabExplosion[10] = Pixel;

            WakeUp = new Texture2D[6];
            for(int i = 0; i < 6; i++) {
                WakeUp[i] = Content.Load<Texture2D>("wake " + i);
            }

            StandUp = new Texture2D[6];
            for(int i = 0; i < 6; i++) {
                StandUp[i] = Content.Load<Texture2D>("stand up " + i);
            }

            LabExit = Content.Load<Texture2D>("lab exit");
            Leaving = Content.Load<Texture2D>("exit");
            LabDebris = Content.Load<Texture2D>("outside scene");

            Twitching = new Texture2D[5];
            for(int i = 0; i < 5; i++) {
                Twitching[i] = Content.Load<Texture2D>("twitch " + (i+1));
            }

            WalkAway = new Texture2D[3];
            WalkAway[0] = Content.Load<Texture2D>("walk away");
            WalkAway[1] = Content.Load<Texture2D>("walk away 2");
            WalkAway[2] = Content.Load<Texture2D>("walk away 3");
            FrontDebris = Content.Load<Texture2D>("front debris");
            BackDebris = Content.Load<Texture2D>("back debris");
            WalkAwayGuy = new Texture2D[3];
            WalkAwayGuy[0] = Content.Load<Texture2D>("walk away guy");
            WalkAwayGuy[1] = Content.Load<Texture2D>("walk away guy 2");
            WalkAwayGuy[2] = Content.Load<Texture2D>("walk away guy 3");
            TurnStart = new Texture2D[3];
            TurnStart[0] = Content.Load<Texture2D>("turn 1");
            TurnStart[1] = Content.Load<Texture2D>("turn 2");
            TurnStart[2] = Content.Load<Texture2D>("turn 3");
            TurnEnd = new Texture2D[3];
            TurnEnd[0] = TurnStart[2];
            TurnEnd[1] = Content.Load<Texture2D>("turn 4");
            TurnEnd[2] = Content.Load<Texture2D>("turn 5");
            WideView = Content.Load<Texture2D>("wide view");

            SlideDown = new Texture2D[3];
            SlideDown[0] = Content.Load<Texture2D>("slide down 1");
            SlideDown[1] = Content.Load<Texture2D>("slide down 2");
            SlideDown[2] = Content.Load<Texture2D>("slide down 3");

            RiverWalk = new Texture2D[22];
            for(int i = 0; i < 22; i++) {
                RiverWalk[i] = Content.Load<Texture2D>("wash away " + (i + 1));
            }

            RiverStare = new Texture2D[3];
            RiverStare[0] = RiverWalk[21];
            RiverStare[1] = Content.Load<Texture2D>("stare 2");
            RiverStare[2] = Content.Load<Texture2D>("stare 3");

            Dissipate = new Texture2D[26];
            for(int i = 0; i < 26; i++) {
                Dissipate[i] = Content.Load<Texture2D>("dissipate " + (i + 1));
            }

            RiverEnd = new Texture2D[3];
            RiverEnd[0] = Content.Load<Texture2D>("river 1");
            RiverEnd[1] = Content.Load<Texture2D>("river 2");
            RiverEnd[2] = Content.Load<Texture2D>("river 3");

            player = new Player();

            // set up levels
            levelNumber = 0;

            // set up menus
            MakeMenus();
            CurrentMenu = mainMenu;

            LevelEditor.SetupUI();
        }

        public void NextLevel() {
            if(CurrentLevel.IsCustom) {
                if(playtesting) {
                    Edit();
                } else {
                    Music.Queue(Songs.Menu);
                    currentTransition = new Transition(CurrentLevel, customLevelSelect, 120);
                    gameState = GameState.Transition;
                }
            } else {
                levelNumber++;
                if(levelNumber > levels.Length - 1) {
                    // win game
                    GameState = GameState.Transition;
                    currentTransition = new Transition(CurrentLevel, new Cutscene(false)); // transition to final cutscene
                    Music.Queue(Songs.End); // play closing song
                } else {
                    CurrentLevel = new Level(levels[levelNumber], false);
                    CurrentLevel.Restart();
                    Camera.Update(CurrentLevel);
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {}

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Input.Update();
            Music.Update();

            switch(gameState) {
                case GameState.Menu:
                    if(CurrentMenu.BackMenu != null && Input.JustPressed(Inputs.Back)) {
                        TransitionToMenu(CurrentMenu.BackMenu);
                    }

                    CurrentMenu.Update();
                    break;

                case GameState.Game:
                    player.Update();
                    CurrentLevel.Update();
                    Camera.Update(CurrentLevel);

                    if(Input.JustPressed(Inputs.Pause)) {
                        Pause();
                    }

                    // win level if off screen on right
                    if(Player.X > CurrentLevel.Width) {
                        NextLevel();
                    }
                    break;

                case GameState.Editor:
                    LevelEditor.Update();
                    break;

                case GameState.Transition:
                    bool finished = currentTransition.Update();
                    if(finished) {
                        switch(currentTransition.EndState) {
                            case GameState.Cutscene:
                                gameState = GameState.Cutscene;
                                CurrentCutscene = currentTransition.EndCutscene;
                                currentTransition = null;
                                break;

                            case GameState.Game:
                                gameState = GameState.Game;
                                CurrentLevel = currentTransition.EndLevel;
                                currentTransition = null;
                                break;

                            case GameState.Menu:
                                gameState = GameState.Menu;
                                CurrentMenu = currentTransition.EndMenu;
                                currentTransition = null;
                                break;
                        }
                    }
                    break;

                case GameState.Cutscene:
                    CurrentCutscene.Update();
                    if(CurrentCutscene.Finished || Input.JustPressed(Inputs.Pause)) {
                        Music.Play(Songs.None); // stop music
                        if(CurrentCutscene.IsIntro) {
                            // go to first level
                            gameState = GameState.Transition;
                            currentTransition = new Transition(CurrentCutscene, CurrentLevel);
                            Music.Queue(Songs.Game);
                        } else {
                            // go to main menu
                            gameState = GameState.Transition;
                            currentTransition = new Transition(CurrentCutscene, mainMenu);
                            Music.Queue(Songs.Menu);
                        }
                    }
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(OffBlack);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, transforms);
            
            switch(gameState) {
                case GameState.Menu:
                    CurrentMenu.Draw(spriteBatch);
                    break;

                case GameState.Game: 
                    CurrentLevel.Draw(spriteBatch);
                    player.Draw(spriteBatch);
                    break;

                case GameState.Editor:
                    LevelEditor.Draw(spriteBatch);
                    break;

                case GameState.Transition:
                    currentTransition.Draw(spriteBatch);
                    break;

                case GameState.Cutscene:
                    CurrentCutscene.Draw(spriteBatch);
                    break;
            }

            spriteBatch.End();

            // add black bars
            spriteBatch.Begin(); // reset scaling and everything
            if(xOffset > 0) {
                spriteBatch.Draw(Pixel, new Rectangle(0, 0, xOffset, GraphicsDevice.Viewport.Height), Color.Black); // left
                spriteBatch.Draw(Pixel, new Rectangle(GraphicsDevice.Viewport.Width - xOffset, 0, xOffset, GraphicsDevice.Viewport.Height), Color.Black); // right
            }
            else if(yOffset > 0) {
                spriteBatch.Draw(Pixel, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, yOffset), Color.Black); // top
                spriteBatch.Draw(Pixel, new Rectangle(0, GraphicsDevice.Viewport.Height - yOffset, GraphicsDevice.Viewport.Width, yOffset), Color.Black); // bottom
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void OnResize(Object sender, EventArgs e) {
            xOffset = 0;
            yOffset = 0;
            if(GraphicsDevice.Viewport.Width > GraphicsDevice.Viewport.Height * 4 / 3f) { // determine which dimension is the limit
                // bars on left and right because wider than tall
                float scale = (float)GraphicsDevice.Viewport.Height / StartScreenHeight;
                gameDims = new Vector2(GraphicsDevice.Viewport.Height * 4 / 3f, GraphicsDevice.Viewport.Height);
                transforms = Matrix.CreateScale(scale, scale, 1);
                xOffset = (int) ((GraphicsDevice.Viewport.Width - GraphicsDevice.Viewport.Height * 4 / 3f) / 2f);
                transforms = transforms * Matrix.CreateTranslation(xOffset, 0, 0);
            } else {
                // bars above and below because taller than wide
                float scale = (float)GraphicsDevice.Viewport.Width / StartScreenWidth;
                gameDims = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Width * 3 / 4f);
                transforms = Matrix.CreateScale(scale, scale, 1);
                yOffset = (int) ((GraphicsDevice.Viewport.Height - GraphicsDevice.Viewport.Width * 3 / 4f) / 2f);
                transforms = transforms * Matrix.CreateTranslation(0, yOffset, 0);
            }
        }

        private void MakeMenus() {
            // main menu
            mainMenu = new Menu(MainMenu);
            int buttonWidth = 200;
            int buttonHeight = 60;
            mainMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 400, buttonWidth, buttonHeight, "Play", () => { 
                SetLevel(0);
                // transition to cutscene instead of game
                Music.Queue(Songs.Intro);
                currentTransition = new Transition(CurrentMenu, new Cutscene(true));
            }));
            mainMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 480, buttonWidth, buttonHeight, "Options", () => { TransitionToMenu(optionsMenu); }));
            mainMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 560, buttonWidth, buttonHeight, "Level Select", () => { TransitionToMenu(levelSelect); }));
            mainMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 640, buttonWidth, buttonHeight, "Level Editor", () => { TransitionToMenu(levelEditorSelect); playtesting = true; }));
            mainMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 720, buttonWidth, buttonHeight, "Quit", Exit));
            mainMenu.ConnectVertically();

            // level editor select
            levelEditorSelect = new Menu(GearMenu);
            levelEditorSelect.BackMenu = mainMenu;
            levelEditorSelect.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 370 - buttonHeight / 2, buttonWidth, buttonHeight, "New", () => {
                Level newLevel = new Level();
                CurrentLevel = newLevel;
                Edit();
            }));
            levelEditorSelect.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 450 - buttonHeight / 2, buttonWidth, buttonHeight, "Load", () => {
                // reset load menu each time
                loadMenu.ClearButtons();

                String[] fileNames = Directory.GetFiles($"Content{GetOsDelimeter()}Levels{GetOsDelimeter()}Custom", "*.lvl"); 
                int startY = StartScreenHeight / 2 - 30 * fileNames.Length;
                for(int i = 0; i < fileNames.Length; i++) {
                    String fileName = fileNames[i];
                    String levelName = fileName.Substring(fileName.LastIndexOf(GetOsDelimeter()) + 1, fileName.LastIndexOf(".") - fileName.LastIndexOf(GetOsDelimeter()) - 1);
                    loadMenu.AddButton(new Button((StartScreenWidth - 2 * buttonWidth) / 2, startY + i * (60), 2 * buttonWidth, 50, levelName, () => { CurrentLevel = new Level(levelName, true); Edit(); }));
                }
                loadMenu.ConnectVertically();

                TransitionToMenu(loadMenu);
            }));
            levelEditorSelect.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 530 - buttonHeight / 2, buttonWidth, buttonHeight, "Back", () => { TransitionToMenu(mainMenu); }));
            levelEditorSelect.ConnectVertically();

            // load menu
            loadMenu = new Menu(GearMenu); // buttons created by level editor select
            loadMenu.BackMenu = levelEditorSelect;

            // pause menu
            pauseMenu = new Menu(VialMenu); // buttons created by game state

            // level select
            levelSelect = new Menu(VialMenu); // buttons created by main menu
            levelSelect.BackMenu = mainMenu;
            levelSelect.AddButton(new Button(StartScreenWidth / 2 - buttonWidth, 150, 2 * buttonWidth, buttonHeight, "Explosion", () => { TransitionToMenu(chapters[0]); } ));
            levelSelect.AddButton(new Button(StartScreenWidth / 2 - buttonWidth, 230, 2 * buttonWidth, buttonHeight, "Mutation", () => { TransitionToMenu(chapters[1]); }));
            levelSelect.AddButton(new Button(StartScreenWidth / 2 - buttonWidth, 310, 2 * buttonWidth, buttonHeight, "Adhesion", () => { TransitionToMenu(chapters[2]); }));
            levelSelect.AddButton(new Button(StartScreenWidth / 2 - buttonWidth, 390, 2 * buttonWidth, buttonHeight, "Confusion", () => { TransitionToMenu(chapters[3]); }));
            levelSelect.AddButton(new Button(StartScreenWidth / 2 - buttonWidth, 470, 2 * buttonWidth, buttonHeight, "Liberation", () => { TransitionToMenu(chapters[4]); }));
            levelSelect.AddButton(new Button(StartScreenWidth / 2 - buttonWidth, StartScreenHeight - buttonHeight - 230, 2 * buttonWidth, buttonHeight, "Custom Levels", () => { 
                // create custom level select buttons
                playtesting = false;

                // reset load menu each time
                customLevelSelect.ClearButtons();

                String[] fileNames = Directory.GetFiles($"Content{GetOsDelimeter()}Levels{GetOsDelimeter()}Custom", "*.lvl");
                int startY = StartScreenHeight / 2 - 30 * fileNames.Length;
                for (int i = 0; i < fileNames.Length; i++)
                {
                    String fileName = fileNames[i];
                    String levelName = fileName.Substring(fileName.LastIndexOf(GetOsDelimeter()) + 1, fileName.LastIndexOf(".") - fileName.LastIndexOf(GetOsDelimeter()) - 1);
                    customLevelSelect.AddButton(new Button((StartScreenWidth - 2 * buttonWidth) / 2, startY + i * (60), 2 * buttonWidth, 50, levelName, () => { SetLevel(levelName); }));
                }
                customLevelSelect.ConnectVertically();

                TransitionToMenu(customLevelSelect);
            }));
            levelSelect.AddButton(new Button(StartScreenWidth / 2 - buttonWidth, StartScreenHeight - buttonHeight - 150, 2 * buttonWidth, buttonHeight, "Back", () => { TransitionToMenu(mainMenu); })); 
            levelSelect.ConnectVertically();

            // custom level select
            customLevelSelect = new Menu(VialMenu); // buttons created by level select
            customLevelSelect.BackMenu = levelSelect;

            // options menu
            optionsMenu = new Menu(GearMenu);
            optionsMenu.BackMenu = mainMenu;
            optionsMenu.AddButton(new Button((int)(StartScreenWidth - 1.5 * buttonWidth) / 2, 330 - buttonHeight / 2, (int)(1.5 * buttonWidth), buttonHeight, "Volume +", () => { Music.ChangeVolume(0.1f); }));
            optionsMenu.AddButton(new Button((int)(StartScreenWidth - 1.5 * buttonWidth) / 2, 410 - buttonHeight / 2, (int)(1.5 * buttonWidth), buttonHeight, "Volume -", () => { Music.ChangeVolume(-0.1f); }));
            optionsMenu.AddButton(new Button((int)(StartScreenWidth - 1.5 * buttonWidth) / 2, 490 - buttonHeight / 2, (int)(1.5 * buttonWidth), buttonHeight, "Colorblind: Off", "Colorblind: On", () => {Colorblind = true;}, () => {Colorblind = false;}));
            optionsMenu.AddButton(new Button((int)(StartScreenWidth - 1.5 * buttonWidth) / 2, 570 - buttonHeight / 2, (int)(1.5 * buttonWidth), buttonHeight, "Fullscreen", "Windowed", 
                () => { graphics.IsFullScreen = true; graphics.ApplyChanges(); }, // become fullscreen
                () => { // become windowed
                    graphics.IsFullScreen = false;
                    graphics.PreferredBackBufferWidth = StartScreenWidth;
                    graphics.PreferredBackBufferHeight = StartScreenHeight;
                    graphics.ApplyChanges(); 
                }
            ));
            optionsMenu.AddButton(new Button((int)(StartScreenWidth - 1.5 * buttonWidth) / 2, 650 - buttonHeight / 2, (int)(1.5 * buttonWidth), buttonHeight, "Back", () => { TransitionToMenu(mainMenu); }));
            optionsMenu.ConnectVertically();

            // chapters
            int menuButtons = 7; // the number of buttons on each chapter menu
            int BUTTON_X = (StartScreenWidth - 2 * buttonWidth) / 2;
            int BUTTON_Y_START = (StartScreenHeight - menuButtons * (buttonHeight + 20) - 20) / 2;
            chapters = new Menu[5];
            chapters[0] = new Menu(VialMenu);
            chapters[0].BackMenu = levelSelect;

            // note: cannot use for loop because it passes in to function literals
            chapters[0].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 0 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[0], () => { SetLevel(0); }));
            chapters[0].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 1 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[1], () => { SetLevel(1); }));
            chapters[0].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 2 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[2], () => { SetLevel(2); }));
            chapters[0].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 3 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[3], () => { SetLevel(3); }));
            chapters[0].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 4 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[4], () => { SetLevel(4); }));
            chapters[0].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 5 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[5], () => { SetLevel(5); }));
            chapters[0].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 6 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, "Back", () => { TransitionToMenu(levelSelect); }));
            chapters[0].ConnectVertically();

            menuButtons = 9;
            BUTTON_Y_START = (StartScreenHeight - menuButtons * (buttonHeight + 20) - 20) / 2;
            chapters[1] = new Menu(VialMenu);
            chapters[1].BackMenu = levelSelect;
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 0 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[6], () => { SetLevel(6); }));
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 1 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[7], () => { SetLevel(7); }));
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 2 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[8], () => { SetLevel(8); }));
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 3 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[9], () => { SetLevel(9); }));
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 4 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[10], () => { SetLevel(10); }));
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 5 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[11], () => { SetLevel(11); }));
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 6 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[12], () => { SetLevel(12); }));
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 7 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[13], () => { SetLevel(13); }));
            chapters[1].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 8 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, "Back", () => { TransitionToMenu(levelSelect); }));
            chapters[1].ConnectVertically();

            menuButtons = 7;
            BUTTON_Y_START = (StartScreenHeight - menuButtons * (buttonHeight + 20) - 20) / 2;
            chapters[2] = new Menu(VialMenu);
            chapters[2].BackMenu = levelSelect;
            chapters[2].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 0 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[14], () => { SetLevel(14); }));
            chapters[2].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 1 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[15], () => { SetLevel(15); }));
            chapters[2].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 2 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[16], () => { SetLevel(16); }));
            chapters[2].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 3 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[17], () => { SetLevel(17); }));
            chapters[2].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 4 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[18], () => { SetLevel(18); }));
            chapters[2].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 5 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[19], () => { SetLevel(19); }));
            chapters[2].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 6 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, "Back", () => { TransitionToMenu(levelSelect); }));
            chapters[2].ConnectVertically();

            menuButtons = 7;
            BUTTON_Y_START = (StartScreenHeight - menuButtons * (buttonHeight + 20) - 20) / 2;
            chapters[3] = new Menu(VialMenu);
            chapters[3].BackMenu = levelSelect;
            chapters[3].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 0 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[20], () => { SetLevel(20); }));
            chapters[3].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 1 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[21], () => { SetLevel(21); }));
            chapters[3].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 2 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[22], () => { SetLevel(22); }));
            chapters[3].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 3 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[23], () => { SetLevel(23); }));
            chapters[3].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 4 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[24], () => { SetLevel(24); }));
            chapters[3].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 5 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[25], () => { SetLevel(25); }));
            chapters[3].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 6 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, "Back", () => { TransitionToMenu(levelSelect); }));
            chapters[3].ConnectVertically();

            menuButtons = 9;
            BUTTON_Y_START = (StartScreenHeight - menuButtons * (buttonHeight + 20) - 20) / 2;
            chapters[4] = new Menu(VialMenu);
            chapters[4].BackMenu = levelSelect;
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 0 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[26], () => { SetLevel(26); }));
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 1 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[27], () => { SetLevel(27); }));
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 2 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[28], () => { SetLevel(28); }));
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 3 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[29], () => { SetLevel(29); }));
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 4 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[30], () => { SetLevel(30); }));
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 5 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[31], () => { SetLevel(31); }));
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 6 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[32], () => { SetLevel(32); }));
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 7 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, levels[34], () => { SetLevel(33); }));
            chapters[4].AddButton(new Button(BUTTON_X, BUTTON_Y_START + 8 * (buttonHeight + 20), 2 * buttonWidth, buttonHeight, "Back", () => { TransitionToMenu(levelSelect); }));
            chapters[4].ConnectVertically();
        }

        private void Pause() {
            // generate pause menu based on whether or not the level is custom
            int buttonWidth = 200;
            int buttonHeight = 60;
            pauseMenu.ClearButtons();

            if(CurrentLevel.IsCustom) {
                pauseMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 340, buttonWidth, buttonHeight, "Resume", () => { currentTransition = new Transition(CurrentMenu, CurrentLevel, 10); gameState = GameState.Transition; }));
                pauseMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 420, buttonWidth, buttonHeight, "Restart", () => {
                    CurrentLevel.Restart();
                    currentTransition = new Transition(CurrentMenu, CurrentLevel, 10);
                    gameState = GameState.Transition;
                }));
                if(playtesting) {
                    pauseMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 500, buttonWidth, buttonHeight, "Edit", Edit));
                } else {
                    pauseMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 500, buttonWidth, buttonHeight, "Exit", () => { 
                        Music.Queue(Songs.Menu);
                        currentTransition = new Transition(CurrentMenu, customLevelSelect, 120);
                        gameState = GameState.Transition;
                    }));
                }
            } else { // game level
                pauseMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 300, buttonWidth, buttonHeight, "Resume", () => { currentTransition = new Transition(CurrentMenu, CurrentLevel, 10); gameState = GameState.Transition; }));
                pauseMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 380, buttonWidth, buttonHeight, "Restart", () => {
                    CurrentLevel.Restart();
                    currentTransition = new Transition(CurrentMenu, CurrentLevel, 10);
                    gameState = GameState.Transition;
                }));
                pauseMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 460, buttonWidth, buttonHeight, "Skip", () => { 
                    gameState = GameState.Game; // make sure to load the decorations
                    if(levelNumber > levels.Length - 2) {
                        // skipping last level: go to main menu
                        Music.Queue(Songs.Menu); 
                        gameState = GameState.Transition; 
                        currentTransition = new Transition(CurrentMenu, mainMenu, 120);
                    } else {
                        NextLevel(); 
                        currentTransition = new Transition(CurrentMenu, CurrentLevel, 10); 
                        gameState = GameState.Transition; 
                    }
                }));
                pauseMenu.AddButton(new Button((StartScreenWidth - buttonWidth) / 2, 540, buttonWidth, buttonHeight, "Exit", () => { 
                    Music.Queue(Songs.Menu); 
                    gameState = GameState.Transition; 

                    // go to the level select that has the current level in it
                    int chapterNum = 0;
                    if(levelNumber >= 26) { // 34 max, 9 levels in last chapter
                        chapterNum = 4;
                        if(levelNumber >= 34) {
                            // avoid index out of range error since there is a level not shown in this menu
                            chapters[4].Select(7);
                        } else {
                            chapters[4].Select(levelNumber - 26);
                        }
                    }
                    else if(levelNumber >= 20) { // 6 levels in chapter 4
                        chapterNum = 3;
                        chapters[3].Select(levelNumber - 20);
                    }
                    else if(levelNumber >= 14) { // 6 levels in chapter 3
                        chapterNum = 2;
                        chapters[2].Select(levelNumber - 14);
                    }
                    else if(levelNumber >= 6) { // 8 levels in chapter 2
                        chapterNum = 1;
                        chapters[1].Select(levelNumber - 6);
                    }
                    else {
                        chapters[0].Select(levelNumber);
                    }
                    currentTransition = new Transition(CurrentMenu, chapters[chapterNum], 120);
                }));
            }
            pauseMenu.ConnectVertically();

            // switch state
            TransitionToMenu(pauseMenu);
        }

        private void Edit() {
            CurrentLevel.Restart();
            gameState = GameState.Editor;
            LevelEditor.Level = CurrentLevel;
            Camera.Position = new Vector2(-400, 0);
            Music.Play(Songs.None);
        }

        private void SetLevel(int levelNum) {
            gameState = GameState.Game; // tells the level to add decorations
            CurrentLevel = new Level(levels[levelNum], false);
            CurrentLevel.Restart();
            levelNumber = levelNum;
            if(levelNum == 0) {
                Player.X = 225; 
            }
            Camera.Update(CurrentLevel); // set camera to beginning of level
            Music.Queue(Songs.Game);
            if(levelNum == levels.Length - 2) { // don't play music on the second to last level
                Music.Queue(Songs.None);
            }

            currentTransition = new Transition(CurrentMenu, CurrentLevel);
            gameState = GameState.Transition;
        }

        private void SetLevel(String name) {
            CurrentLevel = new Level(name, true);
            CurrentLevel.Restart();
            Camera.Update(CurrentLevel); // set camera to beginning of level
            Music.Queue(Songs.Game);

            currentTransition = new Transition(CurrentMenu, CurrentLevel);
            gameState = GameState.Transition;
        }

        private void TransitionToMenu(Menu nextMenu) {
            if(gameState == GameState.Menu) {
                currentTransition = new Transition(CurrentMenu, nextMenu);
            }
            else if(gameState == GameState.Game) {
                currentTransition = new Transition(CurrentLevel, nextMenu);
            }
            else {
                return;
            }

            gameState = GameState.Transition;
        }

        public static char GetOsDelimeter() {
            switch(Environment.OSVersion.Platform) {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    return '/';
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return '\\';
                // OSs other than Mac/Windows are not supported
                default:
                    throw new SystemException("Unknown Operating System");
            }
        }

        public static Direction Opposite(Direction forward) {
            switch(forward) {
                case Direction.Left:
                    return Direction.Right;

                case Direction.Right:
                    return Direction.Left;

                case Direction.Up:
                    return Direction.Down;

                case Direction.Down:
                    return Direction.Up;
            }

            return Direction.None;
        }
    }
}