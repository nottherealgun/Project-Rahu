using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using PrimeTween;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
public class ScenesManager : SerializedMonoBehaviour
{
    public static ScenesManager Instance { get; private set; }
    Scene currentScene;
    [HideInInspector] public Action onSceneLoaded;
    float minimumLoadingTime = 3f;
    [HideInInspector] public AsyncOperation loadOperation;
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

    public void LoadScene(string sceneName)
    {
        currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene("Loading Screen", LoadSceneMode.Additive);
        StartCoroutine(LoadLevelAsync(sceneName));
    }

    IEnumerator LoadLevelAsync(string sceneName)
    {
        float startTime = Time.realtimeSinceStartup;

        loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone && UIManager.Instance.ManualFadeIn().isAlive)
        {
            yield return null;
        }

        float actualLoadDuration = Time.realtimeSinceStartup - startTime;

        if (actualLoadDuration < minimumLoadingTime)
        {
            float remainingDelay = minimumLoadingTime - actualLoadDuration;
            yield return new WaitForSecondsRealtime(remainingDelay);
        }

        loadOperation.allowSceneActivation = true;

        while (!loadOperation.isDone)
        {
            yield return null;
        }


        Debug.Log("Loaded scene: " + sceneName);
        SceneManager.UnloadSceneAsync(currentScene.buildIndex);
        SceneManager.UnloadSceneAsync("Loading Screen");

        EnvironmentalAudioManager.Instance.StopMusic();

        yield return UIManager.Instance.ManualFadeOut();

        onSceneLoaded?.Invoke();
        onSceneLoaded = null;
    }

    public void LoadPuzzleScene(string sceneName)
    {
        StartCoroutine(LoadPuzzleLevelAsync(sceneName));
    }

    IEnumerator LoadPuzzleLevelAsync(string sceneName)
    {
        float startTime = Time.realtimeSinceStartup;

        loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOperation.isDone)
        {
            yield return null;
        }
        onSceneLoaded?.Invoke();
        onSceneLoaded = null;
    }

    public void UnloadPuzzleScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
        onSceneLoaded = null;
    }

    public bool IsLoadingComplete()
    {
        return loadOperation == null || loadOperation.isDone;
    }
}
