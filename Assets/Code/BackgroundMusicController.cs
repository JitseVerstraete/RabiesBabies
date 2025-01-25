using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    
    public AudioSource audioSource;

    private readonly float _speedUpRate = 0.01f;
    private readonly float _maxPitch = 1.5f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayMusic();
    }
    
    public void PlayMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            audioSource.loop = true;
        }
    }
  
    public void PauseMusic()
    {
        if (!audioSource.isPlaying) return;
        audioSource.Pause();
    }

    public void UnPauseMusic()
    {
        if (audioSource.isPlaying) return;
        audioSource.UnPause();
    }

    public void SpeedUpMusic()
    {
        if (audioSource.pitch > _maxPitch) return;
        audioSource.pitch += _speedUpRate;
    }

    public void ResetSpeed()
    {
        audioSource.pitch = 1f;
    }
}
