using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace LabGame
{
    public enum Songs {
        None,
        Menu,
        Game,
        Boss,
        Intro,
        End
    }

    public static class Music
    {
        public static SoundEffectInstance MenuMusic { get; set; }
        public static SoundEffectInstance GameMusic { get; set; }
        public static SoundEffectInstance BossMusic { get; set; }
        public static SoundEffect IntroSong { get; set; }
        public static SoundEffect EndSong { get; set; }

        private static float fadeSpeed = .015f; // percent of fading each frame
        private static float maxVolume = 0.5f; // between 0 and 1
        private static float percentVolume = 1f; // percentage of max volume
        private static int waitFrames = 0; // frames to wait between one song ending and the next one starting
        private static SoundEffectInstance currentSong = null;
        private static SoundEffectInstance nextSong = null;
        private static bool fadeOut = false; // allows fading out to no music

        // handles transitions and song endings
        public static void Update() {
            // handle fading
            if(waitFrames > 0) {
                // pause between songs
                waitFrames--;
                if(waitFrames <= 0) {
                    // change to next song
                    Pause();
                    if(nextSong != null) {
                        nextSong.Play();
                        currentSong = nextSong;
                        nextSong = null;
                    }
                }
            }
            else if(nextSong != null || fadeOut) {
                // fade out
                percentVolume -= fadeSpeed;
                if(percentVolume < 0) {
                    percentVolume = 0;
                    waitFrames = 60; // wait between fade out / in
                    fadeOut = false;
                }
                ChangeVolume();
            }
            else if(percentVolume < 1f) {
                // fade in
                percentVolume += fadeSpeed / 4f;
                if(percentVolume > 1f) {
                    percentVolume = 1f;
                }
                ChangeVolume();
            }
        }

        // fades out of a song and into a new one
        public static void Queue(Songs song) {
            // if it was mid-fade, make sure timing is the same everytime
            percentVolume = 1.0f; 
            fadeOut = false;
            waitFrames = 0;

            nextSong = EnumToInstance(song);
            if(nextSong == null) {
                fadeOut = true;
            }
        }

        public static void ChangeVolume(float amount) {
            maxVolume = maxVolume + amount;
            if(maxVolume > 1f) {
                maxVolume = 1f;
            }
            else if(maxVolume < 0f) {
                maxVolume = 0f;
            }

            ChangeVolume();
        }

        public static void ToggleMute() {
            if(maxVolume > 0) {
                maxVolume = 0;
            } else {
                maxVolume = 0.5f;
            }

            ChangeVolume();
        }

        // Starts playing the input song. If the input song is already playing, does nothing
        public static void Play(Songs song) {
            SoundEffectInstance newSong = EnumToInstance(song);

            // already playing the chosen song
            if(newSong != null && newSong == currentSong) {
                return;
            }

            Pause();

            currentSong = newSong;
            if(newSong != null) {
                newSong.Play();
            }
        }

        private static SoundEffectInstance EnumToInstance(Songs song) {
            switch(song) {
                case Songs.Menu:
                    return MenuMusic;

                case Songs.Game:
                    return GameMusic;

                case Songs.Boss:
                    return BossMusic;

                case Songs.Intro:
                    return IntroSong.CreateInstance();

                case Songs.End:
                    return EndSong.CreateInstance();

                default:
                    return null;
            }
        }

        // makes sure every song is paused
        private static void Pause() {
            if(currentSong != null) {
                currentSong.Stop();
            }
            MenuMusic.Pause();
            GameMusic.Pause();
            BossMusic.Pause();
        }

        // updates the volume to match the new percentage / max volume
        private static void ChangeVolume() {
            float volume = maxVolume * percentVolume;
            if(volume > 1f) {
                volume = 1f;
            }
            else if(volume < 0f) {
                volume = 0f;
            }
            SoundEffect.MasterVolume = volume;
        }
    }
}
