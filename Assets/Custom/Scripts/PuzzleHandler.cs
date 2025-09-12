using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    void Start()
    {
        onPuzzleCompleted.AddListener(GameManager.Instance.OnPuzzleComplete);
    }
    public void PreparePuzzle()
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

        UIManager.OnTransitioned += PuzzleSetup;
    }

    async void PuzzleSetup()
    {
        PlayerController playerController = PersistentDataManager.Player.GetComponent<PlayerController>();

        ScenesManager.Instance.LoadPuzzleScene(puzzleSceneName);
        await ScenesManager.Instance.LoadOperation;
        currentPuzzleManager = GameObject.Find("PuzzleManager");
        completionEmitter = currentPuzzleManager.GetComponent<PuzzleCompletionEmitter>();
        completionEmitter.onPuzzleCompleted.AddListener(() =>
        {
            playerController.ForceExitInteraction();
            playerController.DisconnectFromInteractingObject();
            PersistentDataManager.Instance.puzzles[puzzle] = true;
            onPuzzleCompleted?.Invoke();
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
                EnvironmentalAudioManager.Instance.PlayPersistingAmbience("chemical_stirring");
                break;
            case PuzzleType.Platinum:
                break;
        }
    }

    public void UnloadPuzzle()
    {
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
                EnvironmentalAudioManager.Instance.StopPersistingAmbience("chemical_stirring");
                break;
        }

        UIManager.OnTransitioned += () => ScenesManager.Instance.UnloadPuzzleScene(puzzleSceneName);
        PersistentDataManager.Instance.puzzles[puzzle] = true;
        currentPuzzleManager = null;
        completionEmitter = null;
    }

    [ShowIf("puzzle", PuzzleType.CrystalCrush), Title("Crystal Crush")]
    [ShowIf("puzzle", PuzzleType.CrystalCrush), OdinSerialize, SceneObjectsOnly] Transform crystalSpawnPos;
    [ShowIf("puzzle", PuzzleType.CrystalCrush), OdinSerialize, AssetsOnly] GameObject[] crystalProps = new GameObject[4];
    [ShowIf("puzzle", PuzzleType.CrystalCrush), Button(ButtonSizes.Medium)]
    public void SpawnCrystalProp()
    {
        GameObject newCrystalProp = Instantiate(crystalProps[Random.Range(0, crystalProps.Length)], crystalSpawnPos.position, Quaternion.identity);
    }
}
