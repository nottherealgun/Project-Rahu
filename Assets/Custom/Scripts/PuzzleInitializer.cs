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
    public void TogglePuzzle(bool toggleOn)
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

        if (toggleOn == false)
        {
            currentPuzzleManager = null;
            completionEmitter = null;
            ScenesManager.Instance.UnloadPuzzleScene(puzzleSceneName);
            PersistentDataManager.Player.GetComponent<PlayerController>().SetIsInteracting(false);
            return;
        }

        ScenesManager.Instance.onSceneLoaded += () =>
        {
            currentPuzzleManager = GameObject.Find("PuzzleManager");
            completionEmitter = currentPuzzleManager.GetComponent<PuzzleCompletionEmitter>();
            completionEmitter.onPuzzleCompleted.AddListener(() =>
            {
                ScenesManager.Instance.UnloadPuzzleScene(puzzleSceneName);
                currentPuzzleManager = null;
                completionEmitter = null;
                PersistentDataManager.Player.GetComponent<PlayerController>().StopInteracting();
            });
        };
        ScenesManager.Instance.LoadPuzzleScene(puzzleSceneName);
        
    }
}
