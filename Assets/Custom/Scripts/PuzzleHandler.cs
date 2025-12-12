using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

public enum PuzzleType
{
    MaraInvasion,
    Wirebox,
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
    [OdinSerialize, ReadOnly] bool puzzleLoaded = false;
    [ReadOnly] public bool puzzleCompleted = false;
    public UnityAction onEnterPuzzle;
    public UnityAction onLeavePuzzle;
    void Start()
    {
        onPuzzleCompleted.AddListener(UnloadPuzzle);
        onPuzzleCompleted.AddListener(() => Debug.Log("PUZZLE COMPLETED."));
        onPuzzleCompleted.AddListener(GameManager.Instance.OnPuzzleComplete);

        switch (puzzle)
        {
            case PuzzleType.MaraInvasion:
                puzzleSceneName = "Puzzle01MaraInvasion";
                break;
            case PuzzleType.Wirebox:
                puzzleSceneName = "Puzzle02Wirebox";
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
    }

    public void EnterPuzzle()
    {
        PersistentDataManager.Player.GetComponent<PlayerInput>().enabled = false;
        if (puzzleLoaded)
        {
            UIManager.OnTransitioned += ShowPuzzle;
        }
        else
        {
            UIManager.OnTransitioned += SetupPuzzle;
        }

        switch (puzzle)
        {
            case PuzzleType.CrystalCrush:
                EnvironmentalAudioManager.Instance.PlayMusic("crystal_puzzle_bgm");
                break;
            case PuzzleType.OmegaSolution:
                EnvironmentalAudioManager.Instance.PlayMusic("chemical_puzzle_bgm");
                EnvironmentalAudioManager.Instance.PlayPersistingAmbience("chemical_stirring");
                break;
        }
    }

    public void LeavePuzzle()
    {
        UIManager.OnTransitioned += HidePuzzle;
        UIManager.OnTransitioned += EnvironmentalAudioManager.Instance.StopMusic;
        if (!puzzleCompleted)
            UIManager.OnTransitioned += gameObject.GetComponent<InteractableObject>().ActivatePrompt;
        
        switch (puzzle)
        {
            case PuzzleType.OmegaSolution:
                EnvironmentalAudioManager.Instance.StopPersistingAmbience("chemical_stirring");
                break;
        }

        PersistentDataManager.Player.GetComponent<PlayerInput>().enabled = true;
        UIManager.Instance.DisableInteractionHUD();
    }

    async void SetupPuzzle()
    {
        PlayerController playerController = PersistentDataManager.Player.GetComponent<PlayerController>();

        ScenesManager.Instance.LoadPuzzleScene(puzzleSceneName);
        await ScenesManager.Instance.LoadOperation;
        puzzleLoaded = true;

        currentPuzzleManager = GameObject.Find($"{puzzleSceneName}Manager");
        completionEmitter = currentPuzzleManager.GetComponent<PuzzleCompletionEmitter>();
        completionEmitter.onPuzzleCompleted.AddListener(() =>
        {
            puzzleCompleted = true;
            playerController.ForceExitInteraction();
            playerController.DisconnectFromInteractingObject();
            PersistentDataManager.Instance.puzzles[puzzle] = true;
            onPuzzleCompleted?.Invoke();
        });

        switch (puzzle)
        {
            case PuzzleType.CrystalCrush:
                CrystalCrushGameManager script = currentPuzzleManager.GetComponent<CrystalCrushGameManager>();
                script.onKeyCrystalCollected.AddListener(SpawnCrystalProp);
                NarrativeManager.Instance.CharacterSpeak("SC12_Nate_CandyCrush_DumbQuestion");
                await GameManager.Instance.OnPuzzleStart();
                break;
            case PuzzleType.OmegaSolution:
                NarrativeManager.Instance.CharacterSpeak("SC12_Nate_Chemical_Joke");
                await GameManager.Instance.OnPuzzleStart();
                break;
        }
    }

    async void ShowPuzzle()
    {
        onEnterPuzzle?.Invoke();
        ScenesManager.ShowSceneObjects(puzzleSceneName);
    }

    async void HidePuzzle()
    {
        onLeavePuzzle?.Invoke();
        ScenesManager.HideSceneObjects(puzzleSceneName);
    }

    public void UnloadPuzzle()
    {
        switch (puzzle)
        {
            case PuzzleType.CrystalCrush:
                CrystalCrushGameManager script = currentPuzzleManager.GetComponent<CrystalCrushGameManager>();
                script.onKeyCrystalCollected.RemoveAllListeners();
                break;
            case PuzzleType.OmegaSolution:
                EnvironmentalAudioManager.Instance.StopPersistingAmbience("chemical_stirring");
                break;
        }

        EnvironmentalAudioManager.Instance.PlaySFX("puzzle_complete");

        UIManager.OnTransitioned += () => ScenesManager.Instance.UnloadPuzzleScene(puzzleSceneName);
        puzzleLoaded = false;
        PersistentDataManager.Instance.puzzles[puzzle] = true;
        currentPuzzleManager = null;
        completionEmitter = null;

        if (GameManager.Instance.TwoPuzzlesAreDone())
        {
            if (PersistentDataManager.Instance.HasEventPassed("replacedFuse")) return;
            NarrativeManager.Instance.CharacterSpeak("SC12_Fasai_After2ndPuzzle_Foundfuse");
        }
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
