using System.Collections.Generic;
using System.Linq;
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

    // The mixer to use
    [SerializeField] private AudioMixer mixer;

    //
    // Music
    //

    // Event for when the music changes
    public static System.Action<string> MusicChanged;

    // The AudioSource that music plays from
    private AudioSource musicSource;

    // The string name of the current music
    private string currentMusic;
    private AudioClip currentMusicClip;

    // Dictionary containing all music resources
    private Dictionary<string, AudioClip> musicDict;

    // Songs
    [SerializeField] private AudioClip testMusic;
    [SerializeField] private AudioClip introMusic;

    //
    // SFX
    //

    public enum Source
    {
        Collectable,
        Combo,
    }
    private Dictionary<Source, AudioSource> sourceDict;

    private AudioSource collectableSource;
    private AudioSource comboSource;

    #region Overhead

    void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);

        collectableSource = new GameObject().AddComponent<AudioSource>();
        comboSource = new GameObject().AddComponent<AudioSource>();

        // Populate the dictionaries
        sourceDict = new()
        {
            { Source.Collectable, collectableSource },
            { Source.Combo, comboSource },
        };

        musicDict = new()
        {
            {"Test", testMusic},
            {"Intro", introMusic },
        };



        foreach (AudioSource source in sourceDict.Values)
        {
            source.transform.parent = transform;
        }


        GameObject coinSourceObject = new GameObject();
        collectableSource = coinSourceObject.AddComponent<AudioSource>();

        coinSourceObject.transform.parent = transform;
    }

    #endregion

    #region Music

    // Set the source of the music
    public void SetMusicSource(AudioSource audioSource)
    {
        musicSource = audioSource;

    }

    // Play new music or continue paused music
    public void PlayMusic(string newMusicName = "")
    {
        // Don't do anything and just continue playing if the new music is the same as the current one
        if (newMusicName == "" || currentMusic == newMusicName)
        {
            musicSource.Play();
            return;
        }
        // Get the AudioClip
        AudioClip musicClip = musicDict[newMusicName];

        // Update tracker
        currentMusic = newMusicName;

        MusicChanged?.Invoke(newMusicName);

        // Play using the other function
        PlayMusic(musicClip, true);
    }
    
    // Play a generic AudioClip as music
    private void PlayMusic(AudioClip newMusicClip, bool fromString = false)
    {
        // Don't do anything and just continue playing if the new music is the same as the current one
        if (currentMusicClip == newMusicClip)
        {
            musicSource.Play();
            return;
        }

        // Update tracker
        currentMusicClip = newMusicClip;

        // If it was a generic AudioClip, set the currentMusic to an empty string
        if (!fromString) currentMusic = "";

        // Stop the music, set the new music, and then play it
        musicSource.Stop();
        musicSource.resource = newMusicClip;
        musicSource.Play();
    }

    // Pause the music
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    #endregion

    #region SFX
    public void PlaySFXAtPoint(Source sourceToUse, AudioClip clip, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        AudioSource source = sourceDict[sourceToUse];

        source.transform.parent = transform;
        source.transform.position = position;
        source.pitch = pitch;
        source.PlayOneShot(clip, volume);
    }

    public void PlaySFXNonPositional(Source sourceToUse, AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        AudioSource source = sourceDict[sourceToUse];

        source.transform.parent = Camera.main.transform;
        source.pitch = pitch;
        source.PlayOneShot(clip, volume);
    }
    #endregion

    #region Effects

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
    #endregion
}
