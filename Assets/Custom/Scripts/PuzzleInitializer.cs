using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public enum PuzzleType
{
    MaraInvasion,
    Pipes,
    CrystalCrush,
    OmegaSolution,
    Platinum   
}

public class PuzzleInitializer : SerializedMonoBehaviour
{
    [OdinSerialize] PuzzleType puzzle;
    string puzzleSceneName;
    GameObject currentPuzzleManager;
    PuzzleCompletionEmitter completionEmitter;
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

        ScenesManager.Instance.onSceneLoaded += () =>
        {
            currentPuzzleManager = GameObject.Find("PuzzleManager");
            completionEmitter = currentPuzzleManager.GetComponent<PuzzleCompletionEmitter>();
            completionEmitter.onPuzzleCompleted.AddListener(() =>
            {
                PersistentDataManager.Player.GetComponent<PlayerController>().SetIsInteracting(false);
                PersistentDataManager.Instance.puzzles[puzzle] = true;
            });
        };
        ScenesManager.Instance.LoadPuzzleScene(puzzleSceneName);
    }

    public void EndPuzzle()
    {
        ScenesManager.Instance.UnloadPuzzleScene(puzzleSceneName);
        PersistentDataManager.Instance.puzzles[puzzle] = true;
        currentPuzzleManager = null;
        completionEmitter = null;
    }
}
