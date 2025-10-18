using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // Singleton
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager instance is null");
            }

            return _instance;
        }
    }

    // Event for when the music changes
    public delegate void MusicEventHandler(string newMusicName);

    public event MusicEventHandler MusicChanged;

    // The mixer to use
    [SerializeField] private AudioMixer mixer;

    // The AudioSource that music plays from
    private AudioSource musicSource;

    // The string name of the current music
    private string currentMusic;

    // Dictionary containing all music resources
    private Dictionary<string, AudioResource> musicDict;
    
    void Awake()
    {
        // Populate the dictionary
        musicDict = new()
        {
            {"Test", GetAudio("Music/TestMusic")},
        };

        _instance = this;
    }

    // Get an AudioResource by path
    private AudioResource GetAudio(string path)
    {
        return Resources.Load(path) as AudioResource;
    }

    // Set the source of the music
    public void SetMusicSource(AudioSource audioSource)
    {
        musicSource = audioSource;

    }

    // Play new music or continue paused music
    public void PlayMusic(string newMusicName = "")
    {
        if (newMusicName == "" || currentMusic == newMusicName)
        {
            musicSource.Play();
            return;
        }
        currentMusic = newMusicName;

        // Stop the music, set the new music, and then play it
        musicSource.Stop();
        musicSource.resource = musicDict[currentMusic];
        musicSource.Play();
    }

    // Pause the music
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    // Set the frequency threshold of the low pass filter
    public void SetLowPass(float freq = 22000.0f)
    {
        mixer.SetFloat("LowpassFreq", freq);
    }

    // Audio effect for when the player takes damage
    public void PlayerTookDamage()
    {
        StartCoroutine(DamageCoroutine());
    }

    // Coroutine to ramp the frequency from 1000 back to 22000
    IEnumerator<WaitForSeconds> DamageCoroutine()
    {
        SetLowPass(1000.0f);
        yield return new WaitForSeconds(0.5f);
        for (int i = 1; i <= 22; i++)
        {
            SetLowPass(1000.0f * i);
            yield return new WaitForSeconds(0.2f);
        }
    }

}
