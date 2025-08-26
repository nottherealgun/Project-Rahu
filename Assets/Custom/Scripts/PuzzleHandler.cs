using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public enum PuzzleType
{
    MaraInvasion,
    Pipes,
    CrystalCrush,
    OmegaSolution,
    Platinum   
}

public class PuzzleHandler : SerializedMonoBehaviour
{
    [OdinSerialize] PuzzleType puzzle;
    string puzzleSceneName;
    GameObject currentPuzzleManager;
    PuzzleCompletionEmitter completionEmitter;
    [OdinSerialize] UnityEvent onPuzzleCompleted;
    private void Start() {
        onPuzzleCompleted.AddListener(GameManager.Instance.OnPuzzleComplete);
    }
    public void StartPuzzle()
    {
        switch (puzzle)
        {
            case PuzzleType.MaraInvasion:
                puzzleSceneName = "Puzzle01MaraInvasion";
                break;
            case PuzzleType.Pipes:
                puzzleSceneName = "Puzzle02Pipes";
                break;
            case PuzzleType.CrystalCrush:
                puzzleSceneName = "Puzzle03CrystalCrush";
                break;
            case PuzzleType.OmegaSolution:
                puzzleSceneName = "Puzzle03OmegaSolution";
                break;
            case PuzzleType.Platinum:
                puzzleSceneName = "Puzzle03Platinum";
                break;
        }

        UIManager.onActionTransition += PuzzleSetup;
    }

    async void PuzzleSetup()
    {
        ScenesManager.Instance.LoadPuzzleScene(puzzleSceneName);
        await ScenesManager.Instance.loadOperation;
        currentPuzzleManager = GameObject.Find("PuzzleManager");
        completionEmitter = currentPuzzleManager.GetComponent<PuzzleCompletionEmitter>();
        completionEmitter.onPuzzleCompleted.AddListener(() =>
        {
            PersistentDataManager.Player.GetComponent<PlayerController>().SetIsInteracting(false);
            PersistentDataManager.Instance.puzzles[puzzle] = true;
        });

        switch (puzzle)
        {
            case PuzzleType.MaraInvasion:
                break;
            case PuzzleType.Pipes:
                break;
            case PuzzleType.CrystalCrush:
                CrystalCrushGameManager script = currentPuzzleManager.GetComponent<CrystalCrushGameManager>();
                script.onKeyCrystalCollected.AddListener(SpawnCrystalProp);
                break;
            case PuzzleType.OmegaSolution:
                break;
            case PuzzleType.Platinum:
                break;
        }
    }

    public void EndPuzzle()
    {
        UIManager.onActionTransition -= PuzzleSetup;
        switch (puzzle)
        {
            case PuzzleType.MaraInvasion:
                break;
            case PuzzleType.Pipes:
                break;
            case PuzzleType.CrystalCrush:
                CrystalCrushGameManager script = currentPuzzleManager.GetComponent<CrystalCrushGameManager>();
                script.onKeyCrystalCollected.RemoveAllListeners();
                break;
            case PuzzleType.OmegaSolution:
                break;
            case PuzzleType.Platinum:
                break;
        }

        ScenesManager.Instance.UnloadPuzzleScene(puzzleSceneName);
        PersistentDataManager.Instance.puzzles[puzzle] = true;
        currentPuzzleManager = null;
        completionEmitter = null;
        onPuzzleCompleted?.Invoke();
    }

    [EnableIf("puzzle", PuzzleType.CrystalCrush), Title("Crystal Crush")]
    [EnableIf("puzzle", PuzzleType.CrystalCrush), OdinSerialize, SceneObjectsOnly] Transform crystalSpawnPos;
    [EnableIf("puzzle", PuzzleType.CrystalCrush), OdinSerialize, AssetsOnly] GameObject[] crystalProps = new GameObject[4];
    [EnableIf("puzzle", PuzzleType.CrystalCrush), Button(ButtonSizes.Medium)]
    public void SpawnCrystalProp()
    {
        GameObject newCrystalProp = Instantiate(crystalProps[Random.Range(0, crystalProps.Length)], crystalSpawnPos.position, Quaternion.identity);
    }
}
