using UnityEngine;

namespace Manager
{
    public class MusicManager:Singleton<MusicManager>
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] musicTracks;
        private int currentTrackIndex;
        private void Awake()
        {
            currentTrackIndex = 0;
            LoadVolume();
        }

        private void Start()
        {
            if (musicTracks.Length > 0)
            {
                PlayMusic(0);
            }
        }

        public void PlayMusic(int index)
        {
            if (index < 0 || index >= musicTracks.Length) return;
            currentTrackIndex = index;
            audioSource.clip = musicTracks[currentTrackIndex];
            audioSource.Play();
        }

        public void StopMusic()
        {
            audioSource.Stop();
        }

        public void SetVolume(float volume)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }

        public void NextTrack()
        {
            var nextIndex = (currentTrackIndex + 1) % musicTracks.Length;
            PlayMusic(nextIndex);
        }

        private void LoadVolume()
        {
            var savedVolume = PlayerPrefs.GetInt("Volume", 100) / 100f;
            SetVolume(savedVolume);
        }

    }
}