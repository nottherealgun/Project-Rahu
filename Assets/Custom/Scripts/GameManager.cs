using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
public class GameManager : SerializedMonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [OdinSerialize] PersistentDataManager persistentDataManager;
    public UnityAction onPasscodePanelUnlocked;
    public bool StartsAsTest = false;
    [ShowIf("@StartsAsTest == true")] public string testSceneID = "12";
    [ShowIf("@StartsAsTest == true")] public string testShotID = "01";

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

    public bool OnePuzzleIsDone()
    {
        bool donePuzzle1 = persistentDataManager.puzzles[PuzzleType.CrystalCrush];
        bool donePuzzle2 = persistentDataManager.puzzles[PuzzleType.OmegaSolution];
        return donePuzzle1 || donePuzzle2;
    }

    public bool TwoPuzzlesAreDone()
    {
        bool donePuzzle1 = persistentDataManager.puzzles[PuzzleType.CrystalCrush];
        bool donePuzzle2 = persistentDataManager.puzzles[PuzzleType.OmegaSolution];
        return donePuzzle1 && donePuzzle2;
    }

    public async UniTask OnPuzzleStart()
    {
        if (OnePuzzleIsDone())
        {
            await UniTask.Delay(10000);
            if (PersistentDataManager.Instance.HasEventPassed("foundNewFuse")) return;
            NarrativeManager.Instance.CharacterSpeak("SC12_Nate_DuringSecondPuzzle_FoundFuse");
            PersistentDataManager.Instance.MarkEventAsPassed("foundNewFuse");
        }
    }

    public void OnPuzzleComplete()
    {
        bool donePuzzle1 = persistentDataManager.puzzles[PuzzleType.CrystalCrush];
        bool donePuzzle2 = persistentDataManager.puzzles[PuzzleType.OmegaSolution];
        bool donePuzzle3 = persistentDataManager.puzzles[PuzzleType.Platinum];
        if (donePuzzle1 && donePuzzle2)
        {
            onPasscodePanelUnlocked?.Invoke();
            onPasscodePanelUnlocked = null;
        }
        if (donePuzzle3)
        {
            NarrativeManager.Instance.SetupScene("12", "02");
            UIManager.OnTransitioned += StartFinalCutscene;
            UIManager.Instance.ToggleTransitionPanel();
            // StartFinalCutscene();
        }
    }

    async void StartFinalCutscene()
    {
        await NarrativeManager.Instance.PlayCutsceneSequence();
        ScenesManager.Instance.ShowLoadingScreen();
        await ScenesManager.Instance.LoadScene("MainMenu");
        ScenesManager.Instance.HideLoadingScreen();
        ScenesManager.Instance.ShowScene();
        PersistentDataManager.Instance.FindPlayer();
    }
}
