using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentDataManager : SerializedMonoBehaviour
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

    [OdinSerialize, TabGroup("tab1", "Cinematics", SdfIconType.CameraReelsFill)] public int currentChapter = 2;
    [OdinSerialize, TabGroup("tab1", "Cinematics")] public int currentScene         = 12;
    [OdinSerialize, TabGroup("tab1", "Cinematics")] public int currentVoicelineID   = 1;
    [OdinSerialize, TabGroup("tab1", "Cinematics")] public string currentVoiceline  = "";
    [OdinSerialize, ReadOnly, TabGroup("tab1", "Game Data")]
    [DictionaryDrawerSettings(KeyLabel = "Puzzle Type", ValueLabel = "Is Completed?")]
    public Dictionary<PuzzleType, bool> puzzles = new Dictionary<PuzzleType, bool>
    {
        { PuzzleType.MaraInvasion    , false },
        { PuzzleType.Pipes           , false },
        { PuzzleType.CrystalCrush    , false },
        { PuzzleType.OmegaSolution   , false },
        { PuzzleType.Platinum        , false }
    };
    public static GameObject Player = null;
    public static bool playerExists { get { return Player == null; }}
    [HideInInspector] public Action<bool> onPlayerSearchStatus;
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
