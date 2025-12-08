using System.Collections.Generic;
using Unity.VisualScripting;
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

    // Dictionary containing all music resources
    private Dictionary<string, Music> musicDict;        // Key-value list of musics
    [SerializeField] private List<Music> musicList;     // Serialized list of music assets

    public int startSong = 0;                           // Element in musicList to start with

    private Coroutine lastMusicCoroutine;

    //
    // SFX
    //

    public enum Source
    {
        Generic,
        Collectable,
        Combo,
        Crate,
    }
    private Dictionary<Source, AudioSource> sourceDict;

    private AudioSource genericSource;      // Handy for uncommon sounds that won't suffer from two playing at the same time
    private AudioSource collectableSource;  // For coins and powerups
    private AudioSource comboSource;        // For the combo SFX
    private AudioSource crateSource;        // For the crate breaking SFX

    private Coroutine lastDamageCoroutine;

    #region Overhead

    void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);

        genericSource = new GameObject("Generic Audio Source").AddComponent<AudioSource>();
        collectableSource = new GameObject("Collectable Audio Source").AddComponent<AudioSource>();
        comboSource = new GameObject("Combo Audio Source").AddComponent<AudioSource>();
        crateSource = new GameObject("Crate Audio Source").AddComponent<AudioSource>();

        // Populate the dictionaries
        sourceDict = new()
        {
            { Source.Generic, genericSource },
            { Source.Collectable, collectableSource },
            { Source.Combo, comboSource },
            { Source.Crate, crateSource },
        };

        foreach (AudioSource source in sourceDict.Values)
        {
            source.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
        }

        musicDict = new();

        foreach (Music music in musicList)
            musicDict.Add(music.musicName, music);


        foreach (AudioSource source in sourceDict.Values)
        {
            source.transform.parent = transform;
        }
    }

    void Start()
    {
        musicSources = Camera.main.GetComponents<AudioSource>();

        PlayMusic(musicList[startSong]);
    }

    private void Update()
    {
        if(goalTime > 0.0 && AudioSettings.dspTime > goalTime - 1.0f)
        {
            PlayScheduledMusic();
        }
    }

    public void GameManagerReady()
    {
        GameManager.Instance.stateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        GameManager.Instance.stateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameManager.State newState)
    {
        switch (newState)
        {
            case GameManager.State.MainMenu:
                PlayMusic("MainMenu");
                break;
            case GameManager.State.Playing:
                PlayMusic("Game1");
                break;
            case GameManager.State.Paused:
                break;
            case GameManager.State.Dead:
                break;
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

        if (lastMusicCoroutine != null)
            StopCoroutine(lastMusicCoroutine);
        lastMusicCoroutine = StartCoroutine(FadeMuteMusic(2.0f));

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

    public void PlayMusic(Music newMusic)
    {
        // Reset looping
        musicSources[0].loop = false;
        musicSources[1].loop = false;

        if (lastMusicCoroutine != null)
            StopCoroutine(lastMusicCoroutine);
        lastMusicCoroutine = StartCoroutine(FadeMuteMusic(2.0f));

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
        MusicChanged?.Invoke(newMusic.musicName);
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
        musicSources[musicToggle].volume = currentMusic.volume;
        musicSources[musicToggle].PlayScheduled(goalTime);

        goalTime += (double)(currentClip.samples / currentClip.frequency);

        musicToggle = 1 - musicToggle;
    }

    // Pause the music
    public void PauseMusic()
    {
        musicSources[1 - musicToggle].Pause();
    }

    private IEnumerator<WaitForEndOfFrame> FadeMuteMusic(float time)
    {
        float curTime = 0.0f;
        AudioSource source = musicSources[1 - musicToggle];

        while (curTime < time)
        {
            yield return new WaitForEndOfFrame();
            curTime += Time.deltaTime;
            source.volume = Mathf.Lerp(currentMusic.volume, 0.0f, curTime/time);
        }

        source.volume = 0.0f;
        source.Stop();
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

    public void PlaySFXAtPoint(Source sourceToUse, List<AudioClip> clips, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        AudioSource source = sourceDict[sourceToUse];

        source.transform.parent = transform;
        source.transform.position = position;
        source.pitch = pitch;
        source.PlayOneShot(clips[Random.Range(0, clips.Count)], volume);
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
        if (lastDamageCoroutine != null)
            StopCoroutine(lastDamageCoroutine);
        lastDamageCoroutine = StartCoroutine(DamageCoroutine());
    }

    // Coroutine to ramp the frequency from 1000 back to 22000
    IEnumerator<WaitForSeconds> DamageCoroutine()
    {
        SetLowPass(1000.0f);
        yield return new WaitForSeconds(0.5f);
        for (int i = 1; i <= 22; i++)
        {
            SetLowPass(1000.0f * i);
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion
}

