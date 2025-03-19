using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Poltergeist
{
    public static class AudioManager
    {
        /**
         * Code for this class is shamelessly stolen from the New Horizons mod for Outer Wilds
         */

        private static AudioClip[] clips;
        public static AudioClip defaultClip = null;

        /**
         * Retrieve the clip at the specified index
         */
        public static AudioClip GetClip(int index)
        {
            if(clips.Length == 0)
                return defaultClip;

            return clips[index % clips.Length];
        }

        /**
         * Load all of the audio clips from the folder
         */
        public static void LoadClips(string folderPath)
        {
            String[] filePaths = Directory.GetFiles(folderPath);
            List<AudioClip> loadedClips = new List<AudioClip>();

            //Find every file in the folder
            foreach (String path in filePaths)
            {
                //Try and load it
                try
                {
                    Task<AudioClip> task = Task.Run(async () => await GetAudioClip(path));
                    task.Wait();
                    loadedClips.Add(task.Result);
                }

                //If we can't, throw an error and move on to the next
                catch (Exception e)
                {
                    continue;
                }
            }

            //Put everything in the actual array
            clips = loadedClips.ToArray();
        }

        private static async Task<AudioClip> GetAudioClip(string path)
        {
            string extension = Path.GetExtension(path);
            AudioType audioType;

            //Determine the type of audio, erroring if it's invalid
            switch (extension)
            {
                case ".wav":
                    audioType = AudioType.WAV;
                    break;
                case ".ogg":
                    audioType = AudioType.OGGVORBIS;
                    break;
                case ".mp3":
                    audioType = AudioType.MPEG;
                    break;
                default:
                    return null;
            }

            path = $"file:///{path.Replace("+", "%2B")}";

            //If it's an MP3, need to load it a specific way
            if (audioType == AudioType.MPEG)
            {
                //Set up the thing to "download" the audio
                DownloadHandlerAudioClip dh = new DownloadHandlerAudioClip(path, AudioType.MPEG);
                dh.compressed = true;

                //Make a web request that doesn't actually go to the web
                using (UnityWebRequest www = new UnityWebRequest(path, "GET", dh, null))
                {
                    var result = www.SendWebRequest();

                    while (!result.isDone) await Task.Yield();

                    //Make sure that it actually loaded
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        return null;
                    }

                    //Everything worked, return the MP3 clip
                    else
                    {
                        var audioClip = dh.audioClip;
                        audioClip.name = Path.GetFileNameWithoutExtension(path);
                        return audioClip;
                    }
                }
            }

            //It's not an MP3, we can use this method
            else
            {
                //Make the web request
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
                {
                    var result = www.SendWebRequest();

                    while (!result.isDone) await Task.Yield();

                    //Make sure it actually loaded
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        return null;
                    }

                    //Otherwise, make the clip
                    else
                    {
                        var audioClip = DownloadHandlerAudioClip.GetContent(www);
                        audioClip.name = Path.GetFileNameWithoutExtension(path);
                        return audioClip;
                    }
                }
            }
        }
    }
}
