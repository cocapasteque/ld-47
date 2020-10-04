using UnityEngine;

namespace Utils
{
    public class AudioManager : Singleton<AudioManager>
    {
        private AudioSource _source;

        void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = false;
        }

        public void PlaySound(AudioClip clip)
        {
            _source.PlayOneShot(clip);
        }
    }
}