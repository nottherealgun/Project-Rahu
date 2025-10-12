using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using Sirenix.Serialization;
public class ScenesManager : SerializedMonoBehaviour
{
    public static ScenesManager Instance { get; private set; }
    public static UnityAction OnSceneLoaded;
    [HideInInspector] public AsyncOperation LoadOperation;
    Scene currentScene;
    // Scene loadingScreen;
    [OdinSerialize] GameObject loadingScreen;
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
        // SceneManager.LoadScene("Loading Screen", LoadSceneMode.Additive);
        // loadingScreen = SceneManager.GetSceneByName("Loading Screen");
    }

    public async UniTask LoadScene(string sceneName)
    {
        // 1. Load loading screen
        // 2. Load & disable scene
        // 3. Show cutscene
        // 4. Stop loading screen
        // 5. Run cutscene
        // 6. Enable scene
        // 7. Hide cutscene
        currentScene = SceneManager.GetActiveScene();

        float startTime = Time.realtimeSinceStartup;

        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        print("Loaded scene: " + sceneName);

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        foreach (GameObject o in newScene.GetRootGameObjects())
            o.SetActive(false);

        await SceneManager.UnloadSceneAsync(currentScene.buildIndex);

        currentScene = newScene;

        EnvironmentalAudioManager.Instance.StopMusic();

        await UIManager.Instance.ManualFadeOut();

        OnSceneLoaded?.Invoke();
    }

    public void ShowLoadingScreen()
    {
        loadingScreen.SetActive(true);
        // foreach (GameObject o in loadingScreen.GetRootGameObjects())
        // {
        //     o.SetActive(true);
        // }
    }

    public void HideLoadingScreen()
    {
        loadingScreen.SetActive(false);
        // foreach (GameObject o in loadingScreen.GetRootGameObjects())
        // {
        //     o.SetActive(false);
        // }
    }

    public void ShowScene()
    {
        foreach (GameObject o in currentScene.GetRootGameObjects())
        {
            if (o.name == "Enable When Test") continue;
            o.SetActive(true);
        }
    }

    public void LoadPuzzleScene(string sceneName)
    {
        StartCoroutine(LoadPuzzleLevelAsync(sceneName));
    }

    IEnumerator LoadPuzzleLevelAsync(string sceneName)
    {
        float startTime = Time.realtimeSinceStartup;

        LoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!LoadOperation.isDone)
        {
            yield return null;
        }
        OnSceneLoaded?.Invoke();
    }

    public void UnloadPuzzleScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    public bool IsLoadingComplete()
    {
        return LoadOperation == null || LoadOperation.isDone;
    }

    public static void HideSceneObjects(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            Debug.LogWarning($"Scene '{sceneName}' is not loaded.");
            return;
        }

        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            if (rootObj.name == "Enable When Test") continue;
            rootObj.SetActive(false);
        }
    }

    public static void ShowSceneObjects(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            Debug.LogWarning($"Scene '{sceneName}' is not loaded.");
            return;
        }

        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            if (rootObj.name == "Enable When Test") continue;
            rootObj.SetActive(true);
        }
    }
}
