using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        public bool loop;
        [Range(0f, 1f)] public float volume = 1.0f;
    }

    public List<Sound> sounds = new List<Sound>();
    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

    private readonly float _speedUpRate = 0.01f;
    private readonly float _maxPitch = 1.5f;
    
    private void Start()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create an AudioSource for each sound
        foreach (var sound in sounds)
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.loop = sound.loop;
            audioSource.volume = sound.volume;

            audioSources[sound.name] = audioSource;
        }
        
        PlaySound("backgroundMusic");
    }
    
    private AudioSource GetAudioSource(string soundName)
    {
        if (audioSources.TryGetValue(soundName, out AudioSource audioSource))
        {
            return audioSource;
        }
        Debug.LogWarning($"Sound '{soundName}' not found!");
        return null;
    }
    
    public void PlaySound(string soundName)
    {
        var audioSource = GetAudioSource(soundName);
        if (audioSource != null) audioSource.Play();
    }

    public void StopSound(string soundName)
    {
        var audioSource = GetAudioSource(soundName);
        if (audioSource != null) audioSource.Stop();
    }

    public void SetVolume(string soundName, float volume)
    {
        var audioSource = GetAudioSource(soundName);
        if (audioSource != null) audioSource.volume = Mathf.Clamp01(volume);
    }

    public void StopAllSounds()
    {
        foreach (var audioSource in audioSources.Values)
        {
            audioSource.Stop();
        }
    }

    public void PauseSound(string soundName)
    {
        var audioSource = GetAudioSource(soundName);
        if (audioSource != null) audioSource.Pause();
    }

    public void ResumeSound(string soundName)
    {
        var audioSource = GetAudioSource(soundName);
        if (audioSource != null) audioSource.UnPause();
    }
    
    public void SpeedUpMusic(string soundName)
    {
        var audioSource = GetAudioSource(soundName);
        if (audioSource.pitch > _maxPitch) return;
        audioSource.pitch += _speedUpRate;
    }

    public void ResetSpeed(string soundName)
    {
        var audioSource = GetAudioSource(soundName);
        audioSource.pitch = 1f;
    }

    public bool IsSoundPlaying(string soundName)
    {
        var audioSource = GetAudioSource(soundName);
        return audioSource != null && audioSource.isPlaying;
    }
}
