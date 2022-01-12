using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    public class Transition
    {
        private int timer = 0;
        private int duration;

        private GameState startState;
        private GameState endState;

        private Menu startMenu = null;
        private Menu endMenu = null;
        private Level startLevel = null;
        private Level endLevel = null;
        private Cutscene cutscene = null; // cutscene never transitions into cutscene

        public GameState EndState { get { return endState; } }
        public Menu EndMenu { get { return endMenu; } }
        public Level EndLevel { get { return endLevel; } }
        public Cutscene EndCutscene { get { return cutscene; } }

        // transition from a menu to a menu
        public Transition(Menu startMenu, Menu endMenu, int duration = 20) {
            startState = GameState.Menu;
            endState = GameState.Menu;
            this.startMenu = startMenu;
            this.endMenu = endMenu;
            this.duration = duration;
        }

        // transition from a menu to a level
        public Transition(Menu startMenu, Level endLevel, int duration = 120) {
            startState = GameState.Menu;
            endState = GameState.Game;
            this.startMenu = startMenu;
            this.endLevel = endLevel;
            this.duration = duration;
        }

        // transition from a level to a menu
        public Transition(Level startLevel, Menu endMenu, int duration = 10) {
            startState = GameState.Game;
            endState = GameState.Menu;
            this.startLevel = startLevel;
            this.endMenu = endMenu;
            this.duration = duration;
        }

        // transition from a cutscene to a level
        public Transition(Cutscene startCutscene, Level endLevel, int duration = 120) {
            startState = GameState.Cutscene;
            endState = GameState.Game;
            this.cutscene = startCutscene;
            this.endLevel = endLevel;
            this.duration = duration;
        }

        // transition from a menu to a cutscene
        public Transition(Menu startMenu, Cutscene endCutscene, int duration = 240) {
            startState = GameState.Menu;
            endState = GameState.Cutscene;
            this.startMenu = startMenu;
            this.cutscene = endCutscene;
            this.duration = duration;
        }

        // transition from a cutscene to a menu
        public Transition(Cutscene startCutscene, Menu endMenu, int duration = 300) {
            startState = GameState.Cutscene;
            endState = GameState.Menu;
            this.cutscene = startCutscene;
            this.endMenu = endMenu;
            this.duration = duration;
        }

        // transition from a level to a cutscene
        public Transition(Level startLevel, Cutscene endCutscene, int duration = 360) {
            startState = GameState.Game;
            endState = GameState.Cutscene;
            this.startLevel = startLevel;
            this.cutscene = endCutscene;
            this.duration = duration;
        }

        // advances the transition one frame, then tells whether or not the transition is done
        public bool Update() {
            timer++;
            return timer > duration;
        }

        // draws the current visible menu or level with an amount of black fade
        public void Draw(SpriteBatch sb) {
            float alpha =  timer;

            if(timer <= duration / 2) {
                // draw first half
                switch(startState) {
                    case GameState.Cutscene:
                        cutscene.DrawLastFrame(sb);
                        break;

                    case GameState.Game:
                        startLevel.Draw(sb);
                        Game1.Game.Player.Draw(sb);
                        break;

                    case GameState.Menu:
                        startMenu.Draw(sb);
                        break;
                }
            } else {
                // draw second half
                switch(endState) {
                    case GameState.Cutscene:
                        cutscene.DrawFirstFrame(sb);
                        break;

                    case GameState.Game:
                        endLevel.Draw(sb);
                        Game1.Game.Player.Draw(sb);
                        break;

                    case GameState.Menu:
                        endMenu.Draw(sb);
                        break;
                }

                // T/2 - T
                alpha -= duration; // -T/2 - 0
                alpha *= -1; // T/2 - 0
            }

            // draw black fade
            alpha /= duration / 2.0f;
            if(alpha < 0) {
                alpha = 0;
            }
            sb.Draw(Game1.Pixel, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), new Color(Color.Black, alpha));
        }
    }
}
