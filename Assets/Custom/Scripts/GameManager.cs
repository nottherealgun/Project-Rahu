using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : SerializedMonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [OdinSerialize] PersistentDataManager persistentDataManager;
    public UnityAction onPasscodePanelUnlocked;

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
        persistentDataManager = PersistentDataManager.Instance;
    }

    public void OnPuzzleComplete()
    {
        bool donePuzzle1 = persistentDataManager.puzzles[PuzzleType.CrystalCrush];
        bool donePuzzle2 = persistentDataManager.puzzles[PuzzleType.OmegaSolution];
        bool donePuzzle3 = persistentDataManager.puzzles[PuzzleType.Platinum];
        if(donePuzzle1 && donePuzzle2)
        {
            onPasscodePanelUnlocked?.Invoke();
            onPasscodePanelUnlocked = null;
        }
        if (donePuzzle3)
        {
            // UIManager.OnTransitioned += StartFinalCutscene;
            StartFinalCutscene();
        }
    }

    async void StartFinalCutscene()
    {
        UIManager.LockCursor(false);
        await NarrativeManager.Instance.PlayCutsceneSequence();
        ScenesManager.Instance.ShowLoadingScreen();
        await ScenesManager.Instance.LoadScene("MainMenu");
        ScenesManager.Instance.HideLoadingScreen();
        ScenesManager.Instance.ShowScene();
        PersistentDataManager.Instance.FindPlayer();
    }
}
