using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using PrimeTween;
public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance { get; private set; }
    public Scene currentScene;
    [SerializeField] private float minimumLoadingTime = 3f;

    [SerializeField]
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadScene(SceneSO sceneSO)
    {
        currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene("Loading Screen", LoadSceneMode.Additive);
        StartCoroutine(LoadLevelAsync(sceneSO.sceneName));
    }

    IEnumerator LoadLevelAsync(string sceneName)
    {
        float startTime = Time.realtimeSinceStartup;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone)
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
    }
}
