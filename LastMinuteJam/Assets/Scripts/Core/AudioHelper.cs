using UnityEngine;

public class AudioHelper
{
    public struct OneShot
    {
        public AudioSource source;
        public AudioClip clip;
        public OneShot(AudioSource source, AudioClip clip)
        {
            this.source = source;
            this.clip = clip;
        }
    }
}
