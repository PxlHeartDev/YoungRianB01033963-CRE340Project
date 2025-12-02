using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicAsset", menuName = "Scriptable Objects/MusicAsset")]
[Serializable]
public class Music : ScriptableObject
{
    public string musicName;
    public AudioClip clip;
    public AudioClip loopClip;
    public float volume;


    public Music(string _name, AudioClip _clip, float _volume)
    {
        musicName = _name;
        clip = _clip;
        loopClip = null;
        float volume = _volume;
    }

    public Music(string _name, AudioClip _clip, AudioClip _loopClip, float _volume)
    {
        musicName = _name;
        clip = _clip;
        loopClip = _loopClip;
        float volume = _volume;
    }
}