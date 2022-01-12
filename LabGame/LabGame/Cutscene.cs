using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    public class Cutscene
    {
        private List<Scene> scenes;

        private int currentScene;
        private bool finished;
        private bool isIntro;
        private int endFrames; // frames to pause at end of cutscene

        public bool Finished { get { return finished; } }
        public bool IsIntro { get { return isIntro; } }

        // creates either the intro or closing cutscene
        public Cutscene(bool isIntro) {
            scenes = new List<Scene>();
            currentScene = 0;
            finished = false;
            this.isIntro = isIntro;

            if(isIntro) {
                Scene explosionScene = new Scene(Game1.LabWide, new Animation(Game1.LabExplosion, AnimationType.Hold, 2), 600, 500 + Game1.LabExplosion.Length * 3);
                Scene wakeUpScene = new Scene(Game1.LabInterior, new Animation(Game1.WakeUp, AnimationType.Hold, 4), 240, 90);
                Scene standUpScene = new Scene(Game1.LabInterior, new Animation(Game1.StandUp, AnimationType.Hold, 3), 0, 270);

                scenes.Add(explosionScene);
                scenes.Add(wakeUpScene);
                scenes.Add(standUpScene);
            } else {
                // closing cutscne
                Scene leaveScene = new Scene(Game1.LabExit, Game1.Leaving, 120, 100 + 270); // fade scene is 100 frames
                Scene twitchScene = new Scene(Game1.LabDebris, new Animation(Game1.Twitching, AnimationType.Oscillate, 4), 0, 680);
                ScrollScene walkAwayScene = new ScrollScene(600); // special case: singleton subclass
                Scene stopScene = new Scene(Game1.WalkAway[0], new Animation(Game1.TurnStart, AnimationType.Hold, 10), 0, 60);
                Scene turnBackScene = new Scene(Game1.WalkAway[0], new Animation(Game1.TurnEnd, AnimationType.Hold, 10), 0, 90);
                Scene wideViewScene = new Scene(Game1.WideView, 250);
                Scene slideDownScene = new Scene(Game1.Pixel, new Animation(Game1.SlideDown, AnimationType.Loop, 5), 0, 500);
                Scene riverStartScene = new Scene(Game1.Pixel, new Animation(Game1.RiverEnd, AnimationType.Loop, 5), 0, 120);
                Scene riverWalkScene = new Scene(Game1.Pixel, new Animation(Game1.RiverWalk, AnimationType.Hold, 5), 0, 132); // 6 * 22 frames
                Scene riverStareScene = new Scene(Game1.Pixel, new Animation(Game1.RiverStare, AnimationType.Loop, 5), 0, 300);
                Scene dissipateScene = new Scene(Game1.Pixel, new Animation(Game1.Dissipate, AnimationType.Hold, 2), 0, 78); // 3 * 26
                Scene riverEndScene = new Scene(Game1.Pixel, new Animation(Game1.RiverEnd, AnimationType.Loop, 5), 0, 1300);

                scenes.Add(leaveScene);
                scenes.Add(twitchScene);
                scenes.Add(walkAwayScene);
                scenes.Add(stopScene);
                scenes.Add(turnBackScene);
                scenes.Add(wideViewScene);
                scenes.Add(slideDownScene);
                scenes.Add(riverStartScene);
                scenes.Add(riverWalkScene);
                scenes.Add(riverStareScene);
                scenes.Add(dissipateScene);
                scenes.Add(riverEndScene);
            }
        }

        public void Update() {
            if(scenes[currentScene].Finished) {
                if(currentScene < scenes.Count - 1) {
                    // next cutscene
                    currentScene++;
                } else {
                    // finished cutscene
                    finished = true;
                }
            }
        }

        public void Draw(SpriteBatch sb) {
            scenes[currentScene].DrawNextFrame(sb);
        }

        // transition helpers
        public void DrawFirstFrame(SpriteBatch sb) {
            scenes[0].DrawFirstFrame(sb);
        }

        public void DrawLastFrame(SpriteBatch sb) {
            scenes[scenes.Count - 1].DrawLastFrame(sb);
        }
    }
}
