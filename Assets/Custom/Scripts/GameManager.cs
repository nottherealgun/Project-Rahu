using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : SerializedMonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [OdinSerialize] PersistentDataManager persistentDataManager;
    UnityAction OnPuzzleCompleted;
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
        OnPuzzleCompleted += StartFinalCutscene;
    }

    public void OnPuzzleComplete()
    {
        bool donePuzzle1 = persistentDataManager.puzzles[PuzzleType.CrystalCrush];
        bool donePuzzle2 = persistentDataManager.puzzles[PuzzleType.OmegaSolution];
        if (donePuzzle1 && donePuzzle2)
        {
            UIManager.OnTransitioned += StartFinalCutscene;
        }
    }

    async void StartFinalCutscene()
    {
        UIManager.SetCursorState(false);
        await NarrativeManager.Instance.StartCutscene("12_02");
        ScenesManager.Instance.ShowLoadingScreen();
        await ScenesManager.Instance.LoadScene("MainMenu");
        ScenesManager.Instance.HideLoadingScreen();
        ScenesManager.Instance.ShowScene();
        PersistentDataManager.Instance.FindPlayer();
    }
}
