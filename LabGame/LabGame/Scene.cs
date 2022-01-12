using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    // one portion of a cutscene
    class Scene
    {
        protected Texture2D background;
        private Animation animation;
        protected int animationDuration; // frames to play the animation, be sure to use AnimationType.Hold if holding the last frame
        protected int currentFrame; // frames left holding the first frame
        protected Texture2D specialImage; // for fading scenes, or scrolling scenes
        private float fadeLevel; // for fading scenes only

        public bool Finished { get { return currentFrame >= animationDuration; } }

        // -startFrames -> 0: hold first frame, 0 -> animationDuration: animate
        public Scene(Texture2D background, Animation animation, int startFrames, int animationDuration) {
            currentFrame = -startFrames - 1; 
            this.background = background;
            this.animation = animation;
            this.animationDuration = animationDuration;
        }

        public Scene(Texture2D background, int animationDuration) {
            // no animated portion
            currentFrame = -1; 
            this.background = background;
            this.animation = null;
            this.animationDuration = animationDuration;
        }

        // create a scene that fades an image in instead of animating
        public Scene(Texture2D background, Texture2D fadeImage, int startFrames, int animationDuration) {
            currentFrame = -startFrames - 1;
            this.background = background;
            this.specialImage = fadeImage;
            fadeLevel = 0.0f;
            this.animationDuration = animationDuration;
        }

        public virtual void DrawNextFrame(SpriteBatch sb) {
            currentFrame++;
            if(currentFrame < 0) {
                // hold first frame
                DrawFirstFrame(sb);
            } else {
                // play animation
                if(background != null) {
                    sb.Draw(background, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
                }

                if(specialImage != null) {
                    sb.Draw(specialImage, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White * fadeLevel);
                    if(fadeLevel < 1f) {
                        fadeLevel += 0.01f; // 100 frames total
                    }
                } else if(animation != null) {
                    Texture2D sprite = animation.GetNext();
                    if(sprite != null) {
                        sb.Draw(sprite, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
                    }
                }
            }
        }

        // transition helpers
        public void DrawFirstFrame(SpriteBatch sb) {
            if(background != null) {
                sb.Draw(background, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
            }
            if(animation != null && animation[0] != null) {
                sb.Draw(animation[0], new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
            }
        }

        public void DrawLastFrame(SpriteBatch sb) {
            if(background != null) {
                sb.Draw(background, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
            }
            if(animation != null && animation[animation.Frames - 1] != null) {
                sb.Draw(animation[animation.Frames - 1], new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
            }
            if(specialImage != null) {
                sb.Draw(specialImage, new Rectangle(0, 0, Game1.StartScreenWidth, Game1.StartScreenHeight), Color.White);
            }
        }
    }
}
