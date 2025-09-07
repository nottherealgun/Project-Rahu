using System.Collections.Generic;
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
    [OdinSerialize, ReadOnly] string enteredCode = "";
    int currentDigit = 0;
    void Start()
    {
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

        UIManager.OnTransitioned += PuzzleSetup;
    }

    async void PuzzleSetup()
    {
        ScenesManager.Instance.LoadPuzzleScene(puzzleSceneName);
        await ScenesManager.Instance.LoadOperation;
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

        UIManager.OnTransitioned += () => ScenesManager.Instance.UnloadPuzzleScene(puzzleSceneName);
        PersistentDataManager.Instance.puzzles[puzzle] = true;
        currentPuzzleManager = null;
        completionEmitter = null;
        onPuzzleCompleted?.Invoke();
    }

    [ShowIf("puzzle", PuzzleType.CrystalCrush), Title("Crystal Crush")]
    [ShowIf("puzzle", PuzzleType.CrystalCrush), OdinSerialize, SceneObjectsOnly] Transform crystalSpawnPos;
    [ShowIf("puzzle", PuzzleType.CrystalCrush), OdinSerialize, AssetsOnly] GameObject[] crystalProps = new GameObject[4];
    [ShowIf("puzzle", PuzzleType.CrystalCrush), Button(ButtonSizes.Medium)]
    public void SpawnCrystalProp()
    {
        GameObject newCrystalProp = Instantiate(crystalProps[Random.Range(0, crystalProps.Length)], crystalSpawnPos.position, Quaternion.identity);
    }

    [ShowIf("puzzle", PuzzleType.Platinum), Title("Passcode Numbers")]
    [ShowIf("puzzle", PuzzleType.Platinum), OdinSerialize, SceneObjectsOnly]
    Dictionary<string, List<Sprite>> numbers = new Dictionary<string, List<Sprite>>()
    {
        {"Purple", new List<Sprite>{} },
        {"Green", new List<Sprite>{} },
        {"Yellow", new List<Sprite>{} },
        {"Red", new List<Sprite>{} }
    };
    List<string> colorOrder = new List<string>() { "Purple", "Green", "Yellow", "Red" };

    [ShowIf("puzzle", PuzzleType.Platinum), OdinSerialize, SceneObjectsOnly] List<Image> numberImages = new List<Image>();

    public void OnPanelButtonPressed(int number)
    {
        if (number == -1)
        {
            if (enteredCode.Length > 0) enteredCode = enteredCode.Substring(0, enteredCode.Length - 1);
            if (currentDigit > 0) currentDigit--;
            numberImages[currentDigit].sprite = null;
            return;
        }
        else if (enteredCode.Length == 4)
        {
            enteredCode = "";
            currentDigit = 0;
        }

        enteredCode += number.ToString();
        numberImages[currentDigit].sprite = numbers[colorOrder[currentDigit]][number];
        currentDigit++;

        PasscodeCheck();
    }

    public void PasscodeCheck()
    {
        if (enteredCode == "1584")
        {
            print("Passcode correct! Unlocking door...");
            completionEmitter.onPuzzleCompleted?.Invoke();
            UIManager.SetCursorState(true);
        }
    }
}
