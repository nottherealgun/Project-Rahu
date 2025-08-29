using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
[Serializable]
public class NarrativeManager : SerializedMonoBehaviour
{
    const string AnimaticsPath = "Animatics/";
    const string AnimationsPath = "Animations/";
    public static NarrativeManager Instance { get; private set; }
    [OdinSerialize] AudioDataStore audioDataStore;
    [OdinSerialize] AudioSource cutsceneAudioSource;
    [OdinSerialize] CutsceneStore cutsceneStore;
    [OdinSerialize] int currentVoicelineID = 1;
    [OdinSerialize] string currentVoiceline = "";
    [OdinSerialize] string currentShotID = "12_01";
    [OdinSerialize, AssetsOnly] GameObject cutscenePrefab;
    [OdinSerialize, ReadOnly] Queue<(string, CutsceneStore.Shot)> shotQueue = new Queue<(string, CutsceneStore.Shot)>();
    [OdinSerialize, ReadOnly] Queue<GameObject> cutsceneObjQueue = new Queue<GameObject>();
    GameObject player;
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

    void Start()
    {
        SetupCutsceneSequence();
    }

    // public void StartNewGame()
    // {
        // PlayCutsceneSequence();
        // VoicelineStore currentVoicelineStore = AudioDataStore.Instance.scenes.scenes[currentScene - 1];
        // AudioDataStore.DialogueLine dialogueLine = currentVoicelineStore.cutsceneVoicelines[currentVoicelineID - 1];
        // currentVoiceline = dialogueLine.text;
    // }

    GameObject CreateBlankCutscene()
    {
        GameObject newCutscene = Instantiate(cutscenePrefab, transform);
        VideoPlayer cutscenePlayer = newCutscene.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
        cutscenePlayer.SetTargetAudioSource(0, EnvironmentalAudioManager.Instance.cutsceneSource);
        cutsceneObjQueue.Enqueue(newCutscene);
        return newCutscene;
    }

    public void SetupCutsceneSequence()
    {
        string nextShotID = currentShotID;
        while (true)
        {
            // enqueue loop until the shot is an EVENT shot, nextPlayer final shot or nextShot doesn't exist
            CutsceneStore.Shot currentShot = cutsceneStore.GetShot(nextShotID);
            shotQueue.Enqueue((nextShotID, currentShot));
            GameObject newCutscene = CreateBlankCutscene();
            VideoPlayer cutscenePlayer = newCutscene.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
            PrepareShot(currentShot, cutscenePlayer);

            if (currentShot.isFinalShot || currentShot.shotType != ShotType.LINEAR || currentShot.nextShotID == "")
            {
                break;
            }

            nextShotID = currentShot.nextShotID;
        }
    }

    public async Task PlayCutsceneSequence()
    {
        currentShotID = shotQueue.Dequeue().Item1;
        GameObject cutsceneObj = cutsceneObjQueue.Dequeue();

        VideoPlayer cutscenePlayer = cutsceneObj.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
        cutscenePlayer.Play();
        while (!cutscenePlayer.isPlaying) await Task.Yield();
        while (cutscenePlayer.isPlaying) await Task.Yield();

        Destroy(cutsceneObj);
        // // if shotQueue is not empty, play next shot
        if (cutsceneObjQueue.Count > 0) await PlayCutsceneSequence();
    }

    bool AtFinalCutscene()
    {
        CutsceneStore.Shot currentShot = cutsceneStore.GetShot(currentShotID);
        return currentShot.isFinalShot;
    }

    public async Task StartCutscene(string shotID)
    {
        currentShotID = shotID;
        SetupCutsceneSequence();
        await PlayCutsceneSequence();
    }

    AsyncOperationHandle<VideoClip> PrepareShot(CutsceneStore.Shot nextShot, VideoPlayer vp)
    {
        string nextfilePath = nextShot.fileName;
        AsyncOperationHandle<VideoClip> handle = Addressables.LoadAssetAsync<VideoClip>($"{AnimaticsPath}{nextfilePath}.mp4");
        handle.Completed += (op) =>
        {
            vp.clip = op.Result;
            vp.Prepare();
        };
        return handle;
    }
}