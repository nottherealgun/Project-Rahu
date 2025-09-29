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
using Unity.VisualScripting;
[Serializable]
public class NarrativeManager : SerializedMonoBehaviour
{
    const string AnimaticsPath = "Animatics/";
    const string AnimationsPath = "Animations/";
    public static NarrativeManager Instance { get; private set; }
    [OdinSerialize] AudioDataStore audioDataStore;
    [OdinSerialize] AudioSource cutsceneAudioSource;
    [OdinSerialize] CutsceneStore cutsceneStore;
    [OdinSerialize] Transform cutsceneContainer;
    [OdinSerialize] int currentVoicelineID = 1;
    [OdinSerialize] string currentVoiceline = "";
    [OdinSerialize] string currentShotID = "";
    [OdinSerialize, AssetsOnly] GameObject cutscenePrefab;
    // [OdinSerialize, ReadOnly] Queue<(string, CutsceneStore.Shot)> shotQueue = new Queue<(string, CutsceneStore.Shot)>();
    struct Cutscene
    {
        public string shotID;
        public CutsceneStore.Shot shotData;
        public GameObject cutsceneObj;
        public Cutscene(string id, CutsceneStore.Shot data, GameObject obj)
        {
            shotID = id;
            shotData = data;
            cutsceneObj = obj;
        }
    }
    [OdinSerialize, ReadOnly] Dictionary<string, Cutscene> shotDict = new Dictionary<string, Cutscene>();
    // [OdinSerialize, ReadOnly] Queue<GameObject> cutsceneObjQueue = new Queue<GameObject>();
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

    GameObject CreateBlankCutscene()
    {
        GameObject newCutscene = Instantiate(cutscenePrefab, cutsceneContainer);
        VideoPlayer cutscenePlayer = newCutscene.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
        cutscenePlayer.SetTargetAudioSource(0, EnvironmentalAudioManager.Instance.cutsceneSource);
        // cutsceneObjQueue.Enqueue(newCutscene);
        return newCutscene;
    }

    public void PrepareCutsceneSequence()
    {
        string nextShotID = currentShotID;
        while (true)
        {
            // enqueue loop until the shot is an EVENT shot, nextPlayer final shot or nextShot doesn't exist
            CutsceneStore.Shot currentShot = cutsceneStore.GetShot(nextShotID);
            // shotQueue.Enqueue((nextShotID, currentShot));
            GameObject newCutscene = CreateBlankCutscene();
            shotDict[nextShotID] = new Cutscene(nextShotID, currentShot, newCutscene);
            newCutscene.name = $"Cutscene_{nextShotID}";
            VideoPlayer cutscenePlayer = newCutscene.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
            PrepareShot(currentShot, cutscenePlayer);

            if (currentShot.isFinalShot || currentShot.nextShotID == "")
            {
                break;
            }

            nextShotID = currentShot.nextShotID;
        }
    }

    public async Task PlayCutsceneSequence()
    {
        ShowCutsceneContainer();

        CutsceneStore.Shot currentShot = shotDict[currentShotID].shotData;
        GameObject cutsceneObj = shotDict[currentShotID].cutsceneObj;
        VideoPlayer cutscenePlayer = cutsceneObj.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();

        cutscenePlayer.Play();

        while (!cutscenePlayer.isPlaying) await Task.Yield();
        while (cutscenePlayer.isPlaying) await Task.Yield();

        // Destroy(cutsceneObj);
        cutsceneObj.SetActive(false);

        currentShotID = currentShot.nextShotID;
        
        // if shotQueue is not empty, play next shot
        if (currentShot.shotType == ShotType.EVENT || currentShot.isFinalShot) HideCutsceneContainer();
        else
        {
            await PlayCutsceneSequence();
        }
        ;
    }

    public async Task PlaySequenceFrom(string shotID)
    {
        currentShotID = shotID;
        await PlayCutsceneSequence();
    }

    public void PrepareCutsceneSequenceFrom(string startingShotID)
    {
        currentShotID = startingShotID;
        PrepareCutsceneSequence();
    }

    public async Task StartCutscene(string shotID)
    {
        PrepareCutsceneSequenceFrom(shotID);
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

    void ShowCutsceneContainer()
    {
        cutsceneContainer.gameObject.TryGetComponent<CanvasGroup>(out CanvasGroup _cg);
        _cg.alpha = 1;
    }

    void HideCutsceneContainer()
    {
        cutsceneContainer.gameObject.TryGetComponent<CanvasGroup>(out CanvasGroup _cg);
        _cg.alpha = 0;
    }
}