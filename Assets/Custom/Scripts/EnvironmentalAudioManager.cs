using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class EnvironmentalAudioManager : SerializedMonoBehaviour
{
    public static EnvironmentalAudioManager Instance { get; private set; }
    private void Awake()
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
    [OdinSerialize] GameObject sfxSource;
    [TabGroup("Sources")]
    [OdinSerialize] GameObject nsSfxSource;
    [TabGroup("Sources")]
    [OdinSerialize] AudioSource musicSource;
    [TabGroup("Sources")]
    public AudioSource cutsceneSource;
    [TabGroup("Sources")]
    [OdinSerialize] AudioSource ambienceSource;

    [TabGroup("Data Store")]
    [OdinSerialize] AudioStore tracks;
    AudioListener audioListener;

    private void Start()
    {
        TryGetComponent<AudioListener>(out audioListener);
        PersistentDataManager.Instance.OnPlayerFound += OnPlayerSearchStatus;
    }

    void OnPlayerSearchStatus(bool found)
    {
        audioListener.enabled = !found;
    }

    public void PlaySFX(string trackName, Transform transform = null, int variant = 0) // transform, variant
    {
        AudioClip track = tracks.sfx[trackName][variant];
        if (transform == null)
        {
            PlaySFX(trackName, false);
            return;
        }
        GameObject newSFXSource = Instantiate(sfxSource);
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
        GameObject newSFXSource = Instantiate(sfxSource);
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
        GameObject newSFXSource = Instantiate(nsSfxSource);
        newSFXSource.GetComponent<AudioSource>().clip = track;
        newSFXSource.GetComponent<AudioSource>().Play();
        newSFXSource.GetComponent<DestroyOnAudioFinish>().CheckAudioFinish();
    }

    public void PlaySFX(string trackName, int variant) // variant, no random
    {
        AudioClip track = tracks.sfx[trackName][variant];
        GameObject newSFXSource = Instantiate(nsSfxSource);
        newSFXSource.GetComponent<AudioSource>().clip = track;
        newSFXSource.GetComponent<AudioSource>().Play();
        newSFXSource.GetComponent<DestroyOnAudioFinish>().CheckAudioFinish();
    }

    public void PlaySFX(string trackName)
    {
        AudioClip track = tracks.sfx[trackName][0]; // Assuming the first variant is the UI sound
        GameObject newSFXSource = Instantiate(nsSfxSource);
        newSFXSource.GetComponent<AudioSource>().clip = track;
        newSFXSource.GetComponent<AudioSource>().Play();
        newSFXSource.GetComponent<DestroyOnAudioFinish>().CheckAudioFinish();
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

    public void StopAmbience()
    {
        ambienceSource.Stop();
    }
}
