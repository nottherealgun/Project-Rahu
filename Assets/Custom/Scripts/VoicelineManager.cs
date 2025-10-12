using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using UnityEngine.Events;
using System.Collections;

public class VoicelineManager : SerializedMonoBehaviour
{
    public static VoicelineManager Instance { get; private set; }

    const string VoicelinesPath = "Voicelines/";
    string currentRawVoicepackData;
    [OdinSerialize] VoicelinePack currentVoicelinePack;
    public class VoicelinePack
    {
        public string sceneName;
        public List<string> characters;
        public Dictionary<string, VoicelineData> eventBased;
        public Dictionary<string, List<VoicelineData>> randomBased;
    }

    public UnityAction OnVoicelinePackDictEmpty;

    public class VoicelineData
    {
        public string speaker { get; set; }
        public string dialogueText { get; set; }
        public string audioClipPath { get; set; }
        public string respondingVoicelineKey { get; set; }
        public float? responseDelay { get; set; }
    }

    Queue voicelineQueue = new Queue();
    bool playingDialogue = false;

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

    public async void Setup()
    {
        await InitializeVoicelinePackOf(NarrativeManager.currentScene.name);
        NarrativeManager.currentScene.characters = currentVoicelinePack.characters;
    }

    async UniTask InitializeVoicelinePackOf(string sceneName)
    {
        TextAsset jsonFile;

        AsyncOperationHandle<TextAsset> loadOp = Addressables.LoadAssetAsync<TextAsset>("SC" + sceneName + "VoicelineStore");

        loadOp.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                jsonFile = loadOp.Result;
                currentRawVoicepackData = jsonFile.text;
                // currentVoicelinePack = JsonUtility.FromJson<VoicelinePack>(currentRawVoicepackData);
                currentVoicelinePack = JsonConvert.DeserializeObject<VoicelinePack>(currentRawVoicepackData);
            }
        };

        await loadOp;
    }

    bool ValidateAllVoicelinePacks()
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync("Voicelines/SC12", typeof(Object));

        locationsHandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var loc in handle.Result)
                {
                    // Load each asset from its location
                    Addressables.LoadAssetAsync<Object>(loc).Completed += assetOp =>
                    {
                        if (assetOp.Status == AsyncOperationStatus.Succeeded)
                        {
                            Debug.Log("Loaded: " + assetOp.Result.name);
                        }
                        else
                        {
                            Debug.LogWarning("Failed to load: " + loc.PrimaryKey);
                        }
                    };
                }
            }
            else
            {
                Debug.LogError("Could not find any assets at Voicelines/SC12");
            }
        };
        return true;
    }

    public async UniTask<AudioClip> LoadAudio(string fileName)
    {
        string address = VoicelinesPath + $"SC{NarrativeManager.currentScene.name}/" + fileName;
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(address);
        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            print("Playing audio: " + fileName);
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load audio at {fileName}");
            Addressables.Release(handle);
            return null;
        }
    }

    [Button(ButtonSizes.Large)]
    public async UniTask TestSpeak()
    {
        await CharacterSpeak(ProjectRahu.VoicelineType.RandomBased, "random");
    }

    public async UniTask CharacterSpeak(ProjectRahu.VoicelineType voicelineType, string voicelineKey, bool removeOnUse = true)
    {
        VoicelineData data = null;
        switch (voicelineType)
        {
            case ProjectRahu.VoicelineType.EventBased:
                if (currentVoicelinePack.eventBased.ContainsKey(voicelineKey))
                {
                    data = currentVoicelinePack.eventBased[voicelineKey];
                }
                break;
            case ProjectRahu.VoicelineType.RandomBased:
                if (currentVoicelinePack.randomBased.ContainsKey(voicelineKey))
                {
                    List<VoicelineData> voicelines = currentVoicelinePack.randomBased[voicelineKey];

                    if (voicelines.Count == 0)
                    {
                        OnVoicelinePackDictEmpty?.Invoke();
                        return;
                    }
                    

                    int randomIndex = Random.Range(0, voicelines.Count);
                    data = voicelines[randomIndex];

                    if (removeOnUse) currentVoicelinePack.randomBased[voicelineKey].RemoveAt(randomIndex);
                }
                break;
        }

        // Load audio
        AudioClip clip = await LoadAudio(data.audioClipPath);
        voicelineQueue.Enqueue((data, clip, voicelineType));

        // Play dialogue
        StartCoroutine(PlayDialogueQueue());
    }

    IEnumerator PlayDialogueQueue()
    {
        if (playingDialogue) yield break;

        playingDialogue = true;

        while (voicelineQueue.Count > 0)
        {
            var currentTuple = ((VoicelineData, AudioClip, ProjectRahu.VoicelineType))voicelineQueue.Dequeue();
            VoicelineData voicelineData = currentTuple.Item1;

            // Find speaker
            string speakerName = voicelineData.speaker;
            GameObject speaker = GameObject.Find(speakerName);

            if (speaker == null) break;

            SpeakerComponent speakerComponent = speaker.GetComponent<SpeakerComponent>();
            speakerComponent.voiceSource.clip = currentTuple.Item2;
            speakerComponent.currentDialogueLine = voicelineData.dialogueText;

            if (voicelineData.respondingVoicelineKey != null && voicelineData.respondingVoicelineKey != "")
            {
                ProjectRahu.VoicelineType voicelineType = currentTuple.Item3;
                CharacterSpeak(voicelineType, voicelineData.respondingVoicelineKey, false);
            }
            
            if (voicelineData.responseDelay != null)
                yield return new WaitForSeconds((float)voicelineData.responseDelay);
            
            UIManager.Instance.DisplaySubtitle($"{voicelineData.speaker}: {speakerComponent.currentDialogueLine}");
            yield return speakerComponent.PlayAndCheckDialogueCompletion();
        }
        
        playingDialogue = false;
    }
}
