using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentDataManager : MonoBehaviour
{
    public static PersistentDataManager Instance { get; private set; }
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

    public int currentChapter = 2;
    public int currentScene = 12;
    public int currentVoicelineID = 1;
    public string currentVoiceline = "";
    public static GameObject Player = null;
    public static bool playerExists { get { return Player == null; }}
    public Action<bool> onPlayerSearchStatus;
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Player == null) Player = GameObject.FindWithTag("Player");
        onPlayerSearchStatus.Invoke(Player != null);
    }
}
