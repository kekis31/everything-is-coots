using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum SoundType
    {
        SFX,
        Music,
        Dialogue
    }

    public static SoundManager instance;

    [SerializeField]
    private List<SFXPlayer> sfxPlayers = new List<SFXPlayer>();

    private Dictionary<string, AudioClip> sfx = new Dictionary<string, AudioClip>();

    // Parameters
    [SerializeField, Tooltip("What path all the SFX are located in (Has to be in Assets/Resources)")]
    private string sfxPath;
    [SerializeField, Tooltip("What path all the Music clips are located in (Has to be in Assets/Resources)")]
    private string musicPath;
    [SerializeField, Tooltip("What path all the Dialogue clips are located in (Has to be in Assets/Resources)")]
    private string dialoguePath;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // Pre-load all SFX
        UnityEngine.Object[] clips = Resources.LoadAll(sfxPath, typeof(AudioClip));
        UnityEngine.Object[] musicClips = Resources.LoadAll(musicPath, typeof(AudioClip));
        UnityEngine.Object[] dialogueClips = Resources.LoadAll(dialoguePath, typeof(AudioClip));

        LoadClips(clips);
        LoadClips(musicClips);
        LoadClips(dialogueClips);
    }

    public void LoadClips(UnityEngine.Object[] clips)
    {
        foreach (UnityEngine.Object c in clips)
        {
            sfx.Add(c.name, (AudioClip)c);
            Debug.Log($"Loaded clip ({c.name})");
        }
    }

    void Update()
    {
        // Remove / loop empty players
        foreach (SFXPlayer player in CheckForEmptyPlayers())
        {
            Destroy(player.source);
            sfxPlayers.Remove(player);
        }
    }

    /// <summary>
    /// Check for SFXPlayers that are not paused and not playing anything.
    /// </summary>
    private SFXPlayer[] CheckForEmptyPlayers()
    {
        List<SFXPlayer> emptyPlayers = new List<SFXPlayer>();

        foreach (SFXPlayer s in sfxPlayers)
        {
            if (!s.paused && !s.source.isPlaying)
            {
                emptyPlayers.Add(s);
            }
        }

        return emptyPlayers.ToArray();
    }

    /// <summary>
    /// Play the specified sound effect.
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <param name="loop"></param>
    public void PlaySound(string clipName, float volume, float pitch, bool loop, SoundType type)
    {
        // Try to get clip
        AudioClip clip;

        if (sfx.TryGetValue(clipName, out clip) == false)
        {
            Debug.LogWarning($"Could not play SFX {clipName}. (Not found in path {sfxPath})");
            return;
        }

        // Create new audio source
        AudioSource source = gameObject.AddComponent<AudioSource>();

        float volumeModifier = 1;
        switch (type)
        {
            case SoundType.SFX:
                //volumeModifier = PlayerPrefs.GetFloat("Volume");
                break;
            case SoundType.Music:
                //volumeModifier = PlayerPrefs.GetFloat("MusicVolume");
                break;
            case SoundType.Dialogue:
                //volumeModifier = PlayerPrefs.GetFloat("DialogueVolume");
                break;
        }
        volumeModifier *= PlayerPrefs.GetFloat("MasterVolume");

        source.clip = clip;
        source.volume = volume * volumeModifier;
        source.pitch = pitch;
        source.loop = loop;
        source.Play();

        // Create new SFX player
        sfxPlayers.Add(new SFXPlayer(source, volume, type));

        Debug.Log($"Played SFX ({clipName})");
    }

    public void PlaySound(string clipName, float volume, float pitch, bool loop) { PlaySound(clipName, volume, pitch, loop, SoundType.SFX); }
    public void PlaySound(string clipName, float volume, float pitch) { PlaySound(clipName, volume, pitch, false); }
    public void PlaySound(string clipName, float volume) { PlaySound(clipName, volume, 1); }
    public void PlaySound(string clipName) { PlaySound(clipName, 1); }

    /// <summary>
    /// Changes the volume of the source in relation to the base volume.
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="value"></param>
    public void ChangeVolume(string clipName, float value)
    {
        if (GetSFXPlayerWithClip(clipName) == null)
            return;

        GetSFXPlayerWithClip(clipName).ChangeVolume(value);
    }

    public void UpdateVolume()
    {
        for (int i = 0; i < sfxPlayers.Count; i++)
        {
            float volumeModifier = 1;
            switch (sfxPlayers[i].type)
            {
                case SoundType.SFX:
                    //volumeModifier = PlayerPrefs.GetFloat("Volume");
                    break;
                case SoundType.Music:
                    //volumeModifier = PlayerPrefs.GetFloat("MusicVolume");
                    break;
                case SoundType.Dialogue:
                    //volumeModifier = PlayerPrefs.GetFloat("DialogueVolume");
                    break;
            }
            volumeModifier *= PlayerPrefs.GetFloat("MasterVolume");

            sfxPlayers[i].source.volume = sfxPlayers[i].volume * volumeModifier;
        }

    }

    /// <summary>
    /// Stops the sound with the specified clip playing.
    /// </summary>
    /// <param name="clipName"></param>
    public void StopSoundWithClip(string clipName)
    {
        SFXPlayer player = GetSFXPlayerWithClip(clipName);
        if (player == null)
            return;

        Destroy(player.source);
        sfxPlayers.Remove(player);
    }

    private SFXPlayer GetSFXPlayerWithClip(string clipName)
    {
        for (int i = 0; i < sfxPlayers.Count; i++)
        {
            if (sfxPlayers[i].source.clip.name == clipName)
            {
                return sfxPlayers[i];
            }
        }
        //Debug.LogWarning($"Could not find SFX {clipName}. (Not in sfx players)");
        return null;
    }

    public void StopAllSounds()
    {
        for (int i = 0; i < sfxPlayers.Count; i++)
        {
            sfxPlayers[i].source.Stop();
        }
    }

    public void PauseAllSounds()
    {
        for (int i = 0; i < sfxPlayers.Count; i++)
        {
            sfxPlayers[i].paused = true;
            sfxPlayers[i].source.Pause();
        }
    }

    public void UnpauseAllSounds()
    {
        for (int i = 0; i < sfxPlayers.Count; i++)
        {
            sfxPlayers[i].paused = false;
            sfxPlayers[i].source.UnPause();
        }
    }

    public AudioClip GetSound(string clipName)
    {
        for (int i = 0; i < sfxPlayers.Count; i++)
        {
            if (sfxPlayers[i].source.clip.name == clipName)
            {
                return sfxPlayers[i].source.clip;
            }
        }

        return null;
    }

    [Serializable]
    public class SFXPlayer
    {
        public AudioSource source;
        public float volume;
        public bool paused;
        public SoundType type;

        public SFXPlayer(AudioSource s, float v, SoundType t)
        {
            source = s;
            volume = v;
            type = t;
            paused = false;
        }

        public void ChangeVolume(float value)
        {
            source.volume = volume * value;
        }
    }

}