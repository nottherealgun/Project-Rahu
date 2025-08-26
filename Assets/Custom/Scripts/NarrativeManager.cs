using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using System.Threading.Tasks;
using PrimeTween;
using Palmmedia.ReportGenerator.Core;

[Serializable]
public class NarrativeManager : SerializedMonoBehaviour
{
    public static NarrativeManager Instance { get; private set; }
    [OdinSerialize] AudioDataStore audioDataStore;
    [OdinSerialize] AudioSource cutsceneAudioSource;
    [OdinSerialize] CutsceneStore cutsceneStore;
    [OdinSerialize] int currentVoicelineID = 1;
    [OdinSerialize] string currentVoiceline = "";
    [OdinSerialize] string currentShotID = "12_01";
    [OdinSerialize, AssetsOnly] GameObject cutscenePrefab;
    const string AnimaticsPath = "Animatics/";
    const string AnimationsPath = "Animations/";
    [OdinSerialize, ReadOnly] Queue<(string, CutsceneStore.Shot)> shotQueue = new Queue<(string, CutsceneStore.Shot)>();
    [OdinSerialize, ReadOnly] Queue<GameObject> cutsceneObjQueue = new Queue<GameObject>();
    GameObject player;
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

    private void Start()
    {
        SetupCutsceneSequence();
    }

    public async void StartNewGame()
    {
        // VoicelineStore currentVoicelineStore = AudioDataStore.Instance.scenes.scenes[currentScene - 1];
        // AudioDataStore.DialogueLine dialogueLine = currentVoicelineStore.cutsceneVoicelines[currentVoicelineID - 1];
        // currentVoiceline = dialogueLine.text;
        player = GameObject.FindGameObjectWithTag("Player");
        await PlayCutsceneSequence();
    }

    GameObject CreateBlankCutscene()
    {
        GameObject newCutscene = Instantiate(cutscenePrefab, transform);
        VideoPlayer cutscenePlayer = newCutscene.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
        cutscenePlayer.SetTargetAudioSource(0, EnvironmentalAudioManager.Instance.cutsceneSource);
        newCutscene.SetActive(false);
        cutsceneObjQueue.Enqueue(newCutscene);
        return newCutscene;
    }

    void SetupCutsceneSequence()
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

    async Task PlayCutsceneSequence()
    {
        player.SetActive(false);
        currentShotID = shotQueue.Dequeue().Item1;
        GameObject cutsceneObj = cutsceneObjQueue.Dequeue();
        VideoPlayer cutscenePlayer = cutsceneObj.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
        cutsceneObj.SetActive(true);
        await WaitForVideoEnd(cutscenePlayer);
        Destroy(cutsceneObj);
        if (PlayedFinalCutscene())
        {
            Cursor.lockState = CursorLockMode.None;
            ScenesManager.Instance.LoadScene("MainMenu");
        }

        // if shotQueue is not empty, play next shot
        if (cutsceneObjQueue.Count > 0) await PlayCutsceneSequence();
        else { player.SetActive(true); }
    }

    bool PlayedFinalCutscene()
    {
        CutsceneStore.Shot currentShot = cutsceneStore.GetShot(currentShotID);
        return currentShot.isFinalShot;
    }

    public void StartCutscene(string shotID)
    {
        currentShotID = shotID;
        SetupCutsceneSequence();
        PlayCutsceneSequence();
    }

    private Task WaitForVideoEnd(VideoPlayer vp)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Called when video reaches the end
        void OnVideoEnd(VideoPlayer source)
        {
            vp.loopPointReached -= OnVideoEnd;
            tcs.TrySetResult(true);
        }

        vp.loopPointReached += OnVideoEnd;

        return tcs.Task;
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
        vp.prepareCompleted += (op) => { print($"Prepare completed for {vp.gameObject.name}"); };
        return handle;
    }
}