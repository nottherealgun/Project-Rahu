using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Newtonsoft.Json;

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

    public class VoicelineData
    {
        public string speaker { get; set; }
        public string dialogueText { get; set; }
        public string audioClipPath { get; set; }
    }

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
        // print(currentVoicelinePack.randomBased["random"][0].dialogueText);
    }

    async Task InitializeVoicelinePackOf(string sceneName)
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

        await loadOp.Task;
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

    public async Task<AudioClip> LoadAudio(string fileName)
    {
        string address = VoicelinesPath + $"SC{NarrativeManager.currentScene.name}/" + fileName;
        print(address);
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
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
    public async Task TestSpeak()
    {
        await CharacterSpeak(ProjectRahu.VoicelineType.RandomBased, "random");
    }

    public async Task CharacterSpeak(ProjectRahu.VoicelineType voicelineType, string voicelineKey)
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
                    if (voicelines.Count == 0) return;

                    int randomIndex = Random.Range(0, voicelines.Count);
                    data = voicelines[randomIndex];
                }
                break;
        }

        await CharacterSpeak(data);
    }

    async Task CharacterSpeak(VoicelineData data)
    {
        // Find speaker
        string speakerName = data.speaker;
        GameObject speaker = GameObject.Find(speakerName);
        if (speaker == null) return;
        SpeakerComponent speakerComponent = speaker.GetComponent<SpeakerComponent>();

        // Load audio
        AudioClip clip = await LoadAudio(data.audioClipPath);

        // Play audio and dialogue
        speakerComponent.Speak(data, clip);
    }
}
