using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

public class VoicelineManager : MonoBehaviour
{
    public static VoicelineManager Instance { get; private set; }

    const string VoicelinesPath = "Voicelines/";
    string currentScene = "SC12";
    string currentRawVoicepackData;
    VoicelinePack currentVoicelinePack;

    public class VoicelinePack
    {
        public string sceneName;
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> eventBased;
        public Dictionary<string, Dictionary<string, string>> randomBased;
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

    async void Start()
    {
        if (ValidateAllVoicelinePacks() == false) return;
        await InitializeVoicelinePackOf(currentScene);
        if (currentVoicelinePack == null) return;
        print(currentVoicelinePack.sceneName);
    }

    async Task InitializeVoicelinePackOf(string sceneName)
    {
        TextAsset jsonFile;

        AsyncOperationHandle<TextAsset> loadOp = Addressables.LoadAssetAsync<TextAsset>(sceneName + "VoicelineStore");

        loadOp.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                jsonFile = loadOp.Result;
                currentRawVoicepackData = jsonFile.text;
                currentVoicelinePack = JsonUtility.FromJson<VoicelinePack>(currentRawVoicepackData);
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

    public async Task<AudioClip> LoadAudio(string address)
    {
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load audio at {address}");
            return null;
        }
    }
}
