using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabGame
{
    public enum AnimationType {
        None,
        Oscillate,
        Loop,
        End,
        Hold,
        Rebound
    }

    public class Animation
    {
        private Texture2D[] sprites;
        private AnimationType type;
        private int currentSprite;
        private bool backwards;
        private int waitFrames;
        private int waitFrame;
        private bool complete;

        public int CurrentFrame { get { return currentSprite; } }
        public bool Complete { get { return complete; } }
        public bool Backwards { set { backwards = value; } }
        public Texture2D this[int index] { get { return sprites[index]; } }
        public int Frames { get { return sprites.Length; } }

        public Animation(Texture2D[] sprites, AnimationType type, int waitFrames) {
            this.sprites = sprites;
            this.type = type;
            this.waitFrames = waitFrames;
            Restart();
            if(type == AnimationType.Rebound) {
                backwards = true; // start disabled
            }
        }


        public Texture2D GetNext() {
            if(Game1.Game.GameState == GameState.Editor) {
                return sprites[0];
            }

            if(complete) {
                return null;
            }

            if(waitFrame > 0) {
                waitFrame--;
                if(currentSprite < 0) {
                    return null;
                } else {
                    return sprites[currentSprite];
                }
            } else {
                waitFrame = waitFrames;
            }

            switch(type) {
                case AnimationType.Oscillate:
                    if(currentSprite == 0) {
                        backwards = false;
                        currentSprite++;
                    }
                    else if(currentSprite == sprites.Length - 1) {
                        backwards = true;
                        currentSprite--;
                    }
                    else if(backwards) {
                        currentSprite--;
                    }
                    else { // move forwards
                        currentSprite++;
                    }
                    return sprites[currentSprite];

                case AnimationType.Rebound:
                    if(currentSprite < 0 && backwards) {
                        return null;
                    }
                    else if(currentSprite == sprites.Length - 1) {
                        backwards = true;
                        currentSprite--;
                    }
                    else if(backwards) {
                        currentSprite--;
                        if(currentSprite < 0) {
                            return null;
                        }
                    }
                    else { // move forwards
                        currentSprite++;
                    }
                    return sprites[currentSprite];

                case AnimationType.End:  
                    if(currentSprite < sprites.Length) { // end frame is one past the array size
                        currentSprite++;

                        if(currentSprite >= sprites.Length) {
                            complete = true;
                            return null;
                        } else {
                            return sprites[currentSprite];
                        }
                    }
                    return null;

                case AnimationType.Hold:
                    if(backwards) {
                        currentSprite--;
                        if(currentSprite > 0) {
                            return sprites[currentSprite];
                        } else {
                            return null;
                        }
                    } else {
                        if(currentSprite < sprites.Length - 1) {
                            currentSprite++;
                        }
                        return sprites[currentSprite];
                    }

                case AnimationType.Loop:
                    currentSprite++;
                    if(currentSprite >= sprites.Length) {
                        currentSprite = 0;
                    }
                    return sprites[currentSprite];
            }
            
            return null;
        }

        public void Restart() {
            currentSprite = -1;
            backwards = false;
            complete = false;
            waitFrame = 0;
        }
    }
}
