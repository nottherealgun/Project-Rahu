using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.Events;
using System.Threading.Tasks;

public class ProjectRahu
{
    public enum Character
    {
        FASAI,
        NATE
    }

    public enum VoicelineType
    {
        EventBased,
        RandomBased
    }
}

[Serializable]
public class NarrativeManager : SerializedMonoBehaviour
{
    const string AnimaticsPath = "Animatics/";
    const string AnimationsPath = "Animation/";

    public static NarrativeManager Instance { get; private set; }
    [OdinSerialize] AudioSource cutsceneAudioSource;
    [OdinSerialize] CutsceneStore cutsceneStore;
    [OdinSerialize] Transform cutsceneContainer;
    [OdinSerialize] int currentVoicelineID = 1;
    [OdinSerialize] string currentVoiceline = "";
    [OdinSerialize] string currentShotID = "";
    [OdinSerialize, AssetsOnly] GameObject cutscenePrefab;
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
    public static SceneData currentScene;
    public struct SceneData
    {
        public string name;
        public List<string> characters;
        public SceneData(string name, List<string> characters)
        {
            this.name = name;
            this.characters = characters;
        }
    }
    bool testSetup { get { return GameManager.Instance.StartsAsTest; } }
    [OdinSerialize] Transform choicePromptContainer;
    [OdinSerialize] Transform qtePromptContainer;
    [OdinSerialize, AssetsOnly] GameObject choicePromptPrefab;
    [OdinSerialize, AssetsOnly] GameObject qtePromptPrefab;
    UnityAction choiceChosen;
    UnityAction qteCompleted;
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
        if (testSetup)
        {
            SetupScene(GameManager.Instance.testSceneID, GameManager.Instance.testShotID);
        }
    }

    public async void StartNewGame()
    {
        // This function runs when player starts new game

        ScenesManager.Instance.ShowLoadingScreen();
        await ScenesManager.Instance.LoadScene("CH02_SC12");
        ScenesManager.Instance.HideLoadingScreen();

        PersistentDataManager.Instance.ResetPuzzleData();
        await PlayCutsceneSequence(true);

        ScenesManager.Instance.ShowLoadingScreen();
        ScenesManager.Instance.LoadPuzzleScene("Puzzle02Wirebox");
        await ScenesManager.Instance.LoadOperation;
        WireboxGameManager wireboxManager = FindFirstObjectByType<WireboxGameManager>();
        wireboxManager.onPuzzleCompleted.AddListener(async () =>
        {
            ScenesManager.Instance.ShowLoadingScreen();
            await ScenesManager.Instance.UnloadPuzzleScene("Puzzle02Wirebox");
            ScenesManager.Instance.HideLoadingScreen();

            await PlayCutsceneSequence(true);
        });

        SetupScene("10", "06");

        ScenesManager.Instance.HideLoadingScreen();
        await UIManager.Instance.ManualFadeOut();

        PersistentDataManager.Instance.FindPlayer();
        await LateSetup();
    }

    public void SetupScene(string setupSceneName, string setupStartingShotID = "01")
    {
        currentScene = new SceneData(setupSceneName, new List<string>());
        currentShotID = $"{setupSceneName}_{setupStartingShotID}";

        // Before entering a scene, call this function to setup the scene data and prepare the cutscene sequence
        PrepareCutsceneSequenceFrom($"{setupSceneName}_{setupStartingShotID}");
        VoicelineManager.Instance.Setup();
        VoicelineManager.Instance.OnVoicelinePackDictEmpty += () =>
        {
            StopAllCoroutines();
        };
    }

    public async UniTask LateSetup()
    {
        // Everything here happens after the scene is loaded and objects in the scene are active

        // Force someone to randomly speak (removeOnUse = true)
        await VoicelineManager.Instance.CharacterSpeak(ProjectRahu.VoicelineType.RandomBased, "random");

        StartCoroutine(SpeakRandomLineAfterInterval());
    }

    IEnumerator SpeakRandomLineAfterInterval()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(30f, 60f));
            yield return VoicelineManager.Instance.CharacterSpeak(ProjectRahu.VoicelineType.RandomBased, "random");
        }
    }

    public async void CharacterSpeak(string lineName)
    {
        await VoicelineManager.Instance.CharacterSpeak(ProjectRahu.VoicelineType.EventBased, lineName);
    }

    GameObject CreateBlankCutscene()
    {
        GameObject newCutscene = Instantiate(cutscenePrefab, cutsceneContainer);
        VideoPlayer cutscenePlayer = newCutscene.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
        cutscenePlayer.SetTargetAudioSource(0, EnvironmentalAudioManager.Instance.cutsceneSource);
        return newCutscene;
    }

    public void PrepareCutsceneSequence()
    {
        Queue<string> shotsToPrepare = new Queue<string>();

        EnqueueCutsceneNode(currentShotID, ref shotsToPrepare);

        string nextShotID;
        while (shotsToPrepare.Count > 0)
        {
            // enqueue loop until the shot is an EVENT shot, nextPlayer final shot or nextShot doesn't exist
            nextShotID = shotsToPrepare.Dequeue();
            string newName = $"Cutscene_{nextShotID}";
            CutsceneStore.Shot currentShot = cutsceneStore.GetShot(nextShotID);
            if (cutsceneContainer.Find(newName) != null)
            {
                // already prepared
                shotDict[nextShotID] = new Cutscene(
                    nextShotID,
                    currentShot,
                    cutsceneContainer.Find(newName).gameObject
                );
                continue;
            }
            
            GameObject newCutscene = CreateBlankCutscene();
            shotDict[nextShotID] = new Cutscene(nextShotID, currentShot, newCutscene);
            newCutscene.name = newName;
            VideoPlayer cutscenePlayer = newCutscene.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();
            
            PrepareShot(currentShot, cutscenePlayer);
            
            if (currentShot.shotType == ShotType.CHOICE)
            {
                cutscenePlayer.isLooping = true;
                CreateChoicePrompt(nextShotID);
            }
            else if (currentShot.shotType == ShotType.QTE)
            {
                CreateQTEPrompt(nextShotID);
            }
        }
    }

    void EnqueueCutsceneNode(string rootShotID, ref Queue<string> shotQueue)
    {
        if (rootShotID == "") { return; }
        if (rootShotID.Contains(","))
        {
            string[] idSplit = rootShotID.Split(",");
            foreach (string id in idSplit) EnqueueCutsceneNode(id, ref shotQueue);
        }
        else
        {
            CutsceneStore.Shot currentShot = cutsceneStore.GetShot(rootShotID);

            shotQueue.Enqueue(rootShotID);
            EnqueueCutsceneNode(currentShot.nextShotID, ref shotQueue);
        }
    }

    [Button(ButtonSizes.Large)]
    public async UniTask PlayCutsceneSequence(bool withFadeIn = false)
    {
        // Show cutscenes
        ShowCutsceneContainer();

        // Retrieve fields
        CutsceneStore.Shot currentShot = shotDict[currentShotID].shotData;
        GameObject cutsceneObj = shotDict[currentShotID].cutsceneObj;
        VideoPlayer cutscenePlayer = cutsceneObj.transform.Find("CutscenePlayer").GetComponent<VideoPlayer>();

        // Prepare (if needed) & start playing cutscene
        cutscenePlayer.Play();
        while (!cutscenePlayer.isPlaying) { await UniTask.Yield(); }

        print($"Playing cutscene shot ID: {currentShotID}");

        string[] shotIDSplit = new string[2];

        if (currentShot.nextShotID.Contains(","))
        {
            shotIDSplit = currentShot.nextShotID.Split(",");
        }

        string choiceID1 = shotIDSplit[0];
        string choiceID2 = shotIDSplit[1];

        // On start playing, enable relevant prompts
        switch (currentShot.shotType)
        {
            case ShotType.CHOICE:
                if (choiceID1 == choiceID2)
                {
                    print("Invalid choice shot setup: both choice IDs are the same.");
                }
                await EnableChoicePrompt(currentShot, cutscenePlayer, (choiceID1, choiceID2));
                break;
            case ShotType.QTE:
                await EnableQTEPrompt(currentShot, cutscenePlayer, (choiceID1, choiceID2));
                break;
            default:
                // Otherwise, simply set next shot ID
                break;
        }

        while (cutscenePlayer.isPlaying) { await UniTask.Yield(); }

        if (currentShot.shotType == ShotType.LINEAR)
        {
            currentShotID = currentShot.nextShotID;
        }

        // On cutscene finishes, deactivate cutscene object
        cutsceneObj.SetActive(false);

        if (currentShot.shotType == ShotType.EVENT || currentShot.isFinalShot)
        {
            // If next shot is an Event OR is final shot, fade in and hide cutscenes
            if (withFadeIn)
                await UIManager.Instance.ManualFadeIn();
            HideCutsceneContainer();
        }
        else
        {
            // if shotQueue is not empty, play next shot
            await PlayCutsceneSequence();
        }
        ;
    }

    async UniTask EnableChoicePrompt(CutsceneStore.Shot currentShot, VideoPlayer cutscenePlayer, (string id1, string id2) shotTuple)
    {
        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.CHOICE);
        ShowChoicePrompt(currentShotID);
        ChoicePrompt choicePrompt = GetChoicePrompt(currentShotID);

        bool isChoiceChosen = false;
        choiceChosen += cutscenePlayer.Stop;
        choiceChosen += UIManager.Instance.DisableInteractionHUD;
        choiceChosen += () =>
        {
            isChoiceChosen = true;

            string[] shotIDSplit = currentShotID.Split("_");
            SetupScene(shotIDSplit[0], shotIDSplit[1]);
            
        };

        choicePrompt.onLeftChosen.AddListener(() => currentShotID = shotTuple.id1);
        choicePrompt.onLeftChosen.AddListener(() => choiceChosen());

        choicePrompt.onRightChosen.AddListener(() => currentShotID = shotTuple.id2);
        choicePrompt.onRightChosen.AddListener(() => choiceChosen());

        await UniTask.WaitUntil(() => isChoiceChosen = true);
    }

    async UniTask EnableQTEPrompt(CutsceneStore.Shot currentShot, VideoPlayer cutscenePlayer, (string id1, string id2) shotTuple)
    {
        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.QTE);
        ShowQTEPrompt(currentShotID);
        QTEPrompt qtePrompt = GetQTEPrompt(currentShotID);

        qteCompleted += cutscenePlayer.Stop;
        qteCompleted += UIManager.Instance.DisableInteractionHUD;

        qtePrompt.onQTEFailed.AddListener(() => currentShotID = shotTuple.id1);
        qtePrompt.onQTESucceed.AddListener(() => currentShotID = shotTuple.id2);
        qtePrompt.onQTECompleted.AddListener(() => qteCompleted());
        qtePrompt.Show();

        await qtePrompt.onQTECompleted;
    }

    public async UniTask PlaySequenceFrom(string shotID)
    {
        currentShotID = shotID;
        await PlayCutsceneSequence();
    }

    public void PrepareCutsceneSequenceFrom(string startingShotID)
    {
        currentShotID = startingShotID;
        PrepareCutsceneSequence();
    }

    AsyncOperationHandle<VideoClip> PrepareShot(CutsceneStore.Shot nextShot, VideoPlayer vp)
    {
        string nextfilePath = nextShot.fileName;
        // AsyncOperationHandle<VideoClip> handle = Addressables.LoadAssetAsync<VideoClip>($"{AnimationsPath}{nextfilePath}.mp4");
        string targetPath = $"{AnimationsPath}{nextfilePath}.mp4";
        bool needFallback = false;

        try
        {
            AsyncOperationHandle<IList<IResourceLocation>> validateAddress = Addressables.LoadResourceLocationsAsync(targetPath);
            if (validateAddress.Status != AsyncOperationStatus.Succeeded || validateAddress.Result == null || validateAddress.Result.Count == 0)
            {
                // fallback 1
                Debug.Log($"Failed to find clip at {targetPath}. Trying fallback path...");
                needFallback = true;
            }
        }
        catch(Exception e)
        {
            Debug.Log($"Failed to find clip at {targetPath} with error {e}. Trying fallback path...");
            needFallback = true;
        }

        if (needFallback)
        {
            // try fallback path
            targetPath = $"{AnimaticsPath}{nextfilePath}.mp4";
        }
        
        AsyncOperationHandle<VideoClip> handle;
        try
        {
            handle = Addressables.LoadAssetAsync<VideoClip>(targetPath);
            handle.Completed += (op) =>
            {
                vp.clip = op.Result;
                vp.Prepare();
            };

            return handle;
        }
        catch (Exception e)
        {
            Debug.Log($"Animation failed to load at: {AnimationsPath}{nextfilePath}.mp4; Error: {e.Message}");
            throw e;
        }
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

    GameObject CreateChoicePrompt(string shotID)
    {
        GameObject newChoicePrompt = Instantiate(choicePromptPrefab, choicePromptContainer);
        newChoicePrompt.SetActive(false);
        newChoicePrompt.name = shotID;

        CutsceneStore.ChoicePromptData choicePromptData = cutsceneStore.GetChoicePromptData(shotID);

        GetChoicePrompt(shotID).SetupTexts(
            choicePromptData.leftHeader,
            choicePromptData.leftBody,
            choicePromptData.rightHeader,
            choicePromptData.rightBody
        );

        return newChoicePrompt;
    }

    ChoicePrompt GetChoicePrompt(string shotID)
    {
        return choicePromptContainer.Find(shotID).GetComponent<ChoicePrompt>();
    }

    void ShowChoicePrompt(string shotID)
    {
        choicePromptContainer.Find(shotID).gameObject.SetActive(true);
    }

    GameObject CreateQTEPrompt(string shotID)
    {
        GameObject newQTEPrompt = Instantiate(qtePromptPrefab, qtePromptContainer);
        newQTEPrompt.SetActive(false);
        newQTEPrompt.name = shotID;

        CutsceneStore.QTEPromptData qtePromptData = cutsceneStore.GetQTEPromptData(shotID);

        GetQTEPrompt(shotID).Setup(qtePromptData.onScreenPosition, qtePromptData.keyPrompt);

        return newQTEPrompt;
    }

    QTEPrompt GetQTEPrompt(string shotID)
    {
        return qtePromptContainer.Find(shotID).GetComponent<QTEPrompt>();
    }

    void ShowQTEPrompt(string shotID)
    {
        qtePromptContainer.Find(shotID).gameObject.SetActive(true);
    }
}