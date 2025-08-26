using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class GameManager : SerializedMonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [OdinSerialize] PersistentDataManager persistentDataManager;
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

    private void Start() {
        persistentDataManager = PersistentDataManager.Instance;
    }

    public void OnPuzzleComplete()
    {
        bool donePuzzle1 = persistentDataManager.puzzles[PuzzleType.CrystalCrush];
        bool donePuzzle2 = persistentDataManager.puzzles[PuzzleType.OmegaSolution];
        if (donePuzzle1 && donePuzzle2)
        {
            NarrativeManager.Instance.StartCutscene("12_02");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Destroy(player);
        }
    }
}
