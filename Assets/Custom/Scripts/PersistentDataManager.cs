using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

public class PersistentDataManager : SerializedMonoBehaviour
{
    public static PersistentDataManager Instance { get; private set; }
    public static bool GameOver = false;
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

    [OdinSerialize, ReadOnly, TabGroup("tab1", "Game Data")]
    [DictionaryDrawerSettings(KeyLabel = "Puzzle Type", ValueLabel = "Is Completed?")]
    public Dictionary<PuzzleType, bool> puzzles = new Dictionary<PuzzleType, bool>
    {
        { PuzzleType.MaraInvasion    , false },
        { PuzzleType.Wirebox           , false },
        { PuzzleType.CrystalCrush    , false },
        { PuzzleType.OmegaSolution   , false },
        { PuzzleType.Platinum        , false }
    };
    [ShowInInspector, ReadOnly] public static GameObject Player = null;
    [ShowInInspector, ReadOnly] public static bool isPlayerActive = false;
    [HideInInspector] public UnityAction<bool> OnPlayerFound;
    DialogueDatabase dialogueDatabase;

    void Start()
    {
        dialogueDatabase = DialogueManager.masterDatabase;
        FindPlayer();
    }

    public GameObject FindPlayer()
    {
        Player = GameObject.FindWithTag("Player");
        isPlayerActive = Player != null && Player.activeInHierarchy;
        OnPlayerFound?.Invoke(isPlayerActive);
        if (isPlayerActive)
        {
            return Player;
        }
        return null;
    }

    public void ResetPuzzleData()
    {
        puzzles = new Dictionary<PuzzleType, bool>
        {
            { PuzzleType.MaraInvasion    , false },
            { PuzzleType.Wirebox           , false },
            { PuzzleType.CrystalCrush    , false },
            { PuzzleType.OmegaSolution   , false },
            { PuzzleType.Platinum        , false }
        };
    }

    public bool HasEventPassed(string eventName)
    {
        return DialogueLua.GetVariable(eventName).AsBool;
    }

    public void MarkEventAsPassed(string eventName)
    {
        DialogueLua.SetVariable(eventName,true);
    }
}
