using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class EnvironmentalAudioManager : SerializedMonoBehaviour
{
    public static EnvironmentalAudioManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }
    [TabGroup("Sources")]
    [OdinSerialize] GameObject sfxPrefab;
    [TabGroup("Sources")]
    [OdinSerialize] GameObject nonSpatialSfxPrefab;
    [TabGroup("Sources")]
    [OdinSerialize] AudioSource musicSource;
    [TabGroup("Sources")]
    public AudioSource cutsceneSource;
    [TabGroup("Sources")]
    [OdinSerialize] AudioSource ambienceSource;

    [TabGroup("Data Store")]
    [OdinSerialize] AudioStore tracks;
    AudioListener audioListener;

    void Start()
    {
        TryGetComponent(out audioListener);
        PersistentDataManager.Instance.OnPlayerFound += (bool isPlayerActive) => SetPersistentListener(!isPlayerActive);
    }

    void SetPersistentListener(bool val)
    {
        audioListener.enabled = val;
    }

    public void PlaySFX(string trackName, Transform transform = null, int variant = 0) // transform, variant
    {
        AudioClip track = tracks.sfx[trackName][variant];
        if (transform == null)
        {
            PlaySFX(trackName, false);
            return;
        }
        GameObject newSFXSource = Instantiate(sfxPrefab);
        newSFXSource.name = "SFX_" + trackName;
        newSFXSource.transform.position = transform.position;
        newSFXSource.GetComponent<AudioSource>().clip = track;
        newSFXSource.GetComponent<AudioSource>().Play();
        newSFXSource.GetComponent<DestroyOnAudioFinish>().CheckAudioFinish();
    }

    public void PlaySFX(string trackName, Transform transform = null, bool random = false) // transform, random
    {
        int variant = UnityEngine.Random.Range(0, tracks.sfx[trackName].Count);
        AudioClip track = tracks.sfx[trackName][variant];
        if (transform == null)
        {
            PlaySFX(trackName, random);
            return;
        }
        GameObject newSFXSource = Instantiate(sfxPrefab);
        newSFXSource.name = "SFX_" + trackName;
        newSFXSource.transform.position = transform.position;
        newSFXSource.GetComponent<AudioSource>().clip = track;
        newSFXSource.GetComponent<AudioSource>().Play();
        newSFXSource.GetComponent<DestroyOnAudioFinish>().CheckAudioFinish();
    }

    public void PlaySFX(string trackName, bool random = false) // no variant, random
    {
        int variant = 0;
        if (random)
            variant = UnityEngine.Random.Range(0, tracks.sfx[trackName].Count);
        AudioClip track = tracks.sfx[trackName][variant];
        GameObject newSFXSource = Instantiate(nonSpatialSfxPrefab);
        newSFXSource.name = "SFX_" + trackName;
        newSFXSource.GetComponent<AudioSource>().clip = track;
        newSFXSource.GetComponent<AudioSource>().Play();
        newSFXSource.GetComponent<DestroyOnAudioFinish>().CheckAudioFinish();
    }

    public void PlaySFX(string trackName, int variant) // variant, no random
    {
        AudioClip track = tracks.sfx[trackName][variant];
        GameObject newSFXSource = Instantiate(nonSpatialSfxPrefab);
        newSFXSource.name = "SFX_" + trackName;
        newSFXSource.GetComponent<AudioSource>().clip = track;
        newSFXSource.GetComponent<AudioSource>().Play();
        newSFXSource.GetComponent<DestroyOnAudioFinish>().CheckAudioFinish();
    }

    public void PlaySFX(string trackName)
    {
        AudioClip track = tracks.sfx[trackName][0]; // Assuming the first variant is the UI sound
        GameObject newSFXSource = Instantiate(nonSpatialSfxPrefab);
        newSFXSource.name = "SFX_" + trackName;
        newSFXSource.GetComponent<AudioSource>().clip = track;
        newSFXSource.GetComponent<AudioSource>().Play();
        newSFXSource.GetComponent<DestroyOnAudioFinish>().CheckAudioFinish();
    }

    public void PlayLoopingSFX(string trackName)
    {
        if (LoopingSFXExists(trackName)) return;
        AudioClip track = tracks.sfx[trackName][0]; // Assuming the first variant is the looping sound
        GameObject newSFXSource = Instantiate(sfxPrefab, transform);
        newSFXSource.name = "SFX_" + trackName;
        newSFXSource.transform.position = transform.position;
        AudioSource source = newSFXSource.GetComponent<AudioSource>();
        source.clip = track;
        source.loop = true;
        source.Play();
    }

    public void StopLoopingSFX(string trackName)
    {
        if(!LoopingSFXExists(trackName)) return;
        GameObject newSource = transform.Find("SFX_" + trackName).gameObject;
        if (newSource != null)
        {
            newSource.GetComponent<AudioSource>().Stop();
            Destroy(newSource);
        }
    }

    bool LoopingSFXExists(string trackName)
    {
        return transform.Find("SFX_" + trackName) != null;
    }

    public void PlayMusic(string trackName)
    {
        musicSource.clip = tracks.music[trackName];
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlayAmbience(string trackName)
    {
        ambienceSource.clip = tracks.ambience[trackName];
        ambienceSource.Play();
    }

    public void PlayPersistingAmbience(string trackName)
    {
        if (PersistingAmbienceExists(trackName)) return;
        AudioSource newSource = Instantiate(ambienceSource, this.transform);
        newSource.name = "AMB_" + trackName;
        newSource.clip = tracks.ambience[trackName];
        newSource.Play();
    }

    public void StopAmbience()
    {
        ambienceSource.Stop();
    }

    public void StopPersistingAmbience(string trackName)
    {
        if (!PersistingAmbienceExists(trackName)) return;
        GameObject newSource = transform.Find("AMB_" + trackName).gameObject;
        newSource.GetComponent<AudioSource>().Stop();
        Destroy(newSource);
    }
    
    bool PersistingAmbienceExists(string trackName)
    {
        return transform.Find("AMB_" + trackName) != null;
    }
}
