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

    // The mixer to use
    [SerializeField] private AudioMixer mixer;

    //
    // Music
    //

    private double goalTime = 0.0;
    private int musicToggle = 0;

    // Event for when the music changes
    public static System.Action<string> MusicChanged;

    // The AudioSource that music plays from
    private AudioSource[] musicSources;

    // The current music
    private Music currentMusic;

    private AudioClip currentClip;
    private AudioClip loopClip;

    // Dictionary containing all music resources
    private Dictionary<string, Music> musicDict;

    // Songs
    [SerializeField] private AudioClip testMusic;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip mainMenuMusicLoop;
    [SerializeField] private AudioClip game1Music;
    [SerializeField] private AudioClip game1MusicLoop;

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
            {"Test", new Music("Test", testMusic)},
            {"MainMenu", new Music("Main Menu", mainMenuMusic, mainMenuMusicLoop) },
            {"Game1", new Music("Game1", game1Music, game1MusicLoop) },
        };



        foreach (AudioSource source in sourceDict.Values)
        {
            source.transform.parent = transform;
        }

        GameObject coinSourceObject = new GameObject();
        collectableSource = coinSourceObject.AddComponent<AudioSource>();

        coinSourceObject.transform.parent = transform;
    }

    void Start()
    {
        musicSources = Camera.main.GetComponents<AudioSource>();

        currentMusic = new Music("none", testMusic);
        PlayMusic("MainMenu");
    }

    private void Update()
    {
        if(goalTime > 0.0 && AudioSettings.dspTime > goalTime - 1.0f)
        {
            PlayScheduledMusic();
        }
    }

    #endregion

    #region Music

    // Music system is currently suitable for songs with an intro, no outro
    // Currently has a weird bug where the looped section plays slightly before its meant to after the first loop. TODO

    // Play new music or continue paused music
    public void PlayMusic(string newMusicName)
    {
        // Get the music
        Music newMusic = musicDict[newMusicName];

        // Reset looping
        musicSources[0].loop = false;
        musicSources[1].loop = false;

        // Set the music
        currentMusic = newMusic;
        currentClip = newMusic.clip;
        goalTime = AudioSettings.dspTime + 0.01;
        PlayScheduledMusic();
        if (newMusic.loopClip != null)
        {
            goalTime = AudioSettings.dspTime + currentClip.length;
            currentClip = newMusic.loopClip;
        }

        // Broadcast event
        MusicChanged?.Invoke(newMusicName);
    }
    
    // Play the next music clip
    private void PlayScheduledMusic()
    {
        // If this is the looping clip then set looping state appropriately
        if (currentMusic.loopClip == musicSources[1 - musicToggle].clip)
        {
            goalTime = -1.0; 
            musicSources[1 - musicToggle].loop = true;
            return;
        }
        // Play the music
        musicSources[musicToggle].clip = currentClip;
        musicSources[musicToggle].PlayScheduled(goalTime);

        goalTime = goalTime + (double)(currentClip.samples / currentClip.frequency);

        musicToggle = 1 - musicToggle;
    }

    // Pause the music
    public void PauseMusic()
    {
        musicSources[1 - musicToggle].Pause();
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

public class Music
{
    public string name { get; private set; }
    public AudioClip clip { get; private set; }
    public AudioClip loopClip { get; private set; }


    public Music(string _name, AudioClip _clip)
    {
        name = _name;
        clip = _clip;
    }

    public Music(string _name, AudioClip _clip, AudioClip _loopClip)
    {
        name = _name;
        clip = _clip;
        loopClip = _loopClip;
    } 
}