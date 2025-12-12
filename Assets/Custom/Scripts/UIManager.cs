using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Users;
using System;
using System.Linq;

public class UIManager : SerializedMonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public static event UnityAction OnTransitioned;
    [OdinSerialize] GameObject _transitionPanel;
    [OdinSerialize] Image _transitionPanelImage;
    [OdinSerialize] GameObject pauseMenu;
    PlayerController playerController;
    static bool _isGamePaused = false;
    public static bool isGamePaused { get { return _isGamePaused; } }
    public static bool lastCursorState = true;

    [Title("PDA Menu")]
    [OdinSerialize] GameObject pdaMenu;
    [OdinSerialize] TMP_Text pdaHeader;
    [OdinSerialize] TMP_Text pdaBody;
    [OdinSerialize] Image pdaImage;

    [Title("Settings")]
    [OdinSerialize] GameObject settingsMenu;

    [Title("Subtitles")]
    private bool _subtitlesOn = true;
    [ReadOnly] public bool subtitlesOn { get { return _subtitlesOn; } set { SetSubtitles(value); } }
    public static bool isTransitioning = false;
    void SetSubtitles(bool value)
    {
        _subtitlesOn = value;
        if (!value) subtitleContainer.SetActive(false);
        else subtitleContainer.SetActive(true);
    }
    [OdinSerialize] TMP_Text subtitleText;
    [OdinSerialize] CanvasGroup subtitleCanvasGroup;
    [OdinSerialize] GameObject subtitleContainer;

    [Title("Interaction Prompt HUD")]
    [OdinSerialize] GameObject interactionPromptHUD;

    [Title("Quest-Task HUD")]
    [OdinSerialize] GameObject questHUD;

    Stack uiLayers = new Stack();
    Sequence? sequence;

    public enum InteractionHUDPreset { DEFAULT, INTERACTABLE, ROTATABLE, CHOICE, QTE, PUZZLE, CUSTOM }

    [ReadOnly, OdinSerialize] string controlScheme = "Keyboard&Mouse";
    public UnityAction onOpenSettings;
    public UnityAction onCloseSettings;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(this.gameObject);

        _transitionPanelImage = _transitionPanel.GetComponent<Image>();
    }

    void Start()
    {
        if (GameManager.Instance.StartsAsTest)
        {
            EnableQuestHUD();
        }
        PersistentDataManager.Instance.OnPlayerFound += OnPlayerSearchStatus;
        settingsMenu.GetComponent<SettingsMenu>().Initialize();
    }

    [Button(ButtonSizes.Large)]
    public static void LockCursor(bool newState)
    {
        // true = locked, false = unlocked
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !newState;
    }

    public static void RevertCursorState()
    {
        LockCursor(lastCursorState);
    }

    void OnPlayerSearchStatus(bool found)
    {
        if (found) playerController = PersistentDataManager.Player.GetComponent<PlayerController>();
    }

    public void ToggleTransitionPanel(bool isInteracting = false)
    {
        isTransitioning = true;
        _transitionPanelImage.raycastTarget = true;
        Sequence.Create(cycles: 1, CycleMode.Restart)
            .Chain(Tween.Custom(Color.clear, Color.black, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal))
            .ChainCallback(() => RunOnTransitioned())
            .Chain(Tween.Custom(Color.black, Color.clear, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal))
            .OnComplete(() =>
            {
                _transitionPanelImage.raycastTarget = false;
                isTransitioning = false;
            });
    }

    public void RunOnTransitioned()
    {
        OnTransitioned?.Invoke();
        OnTransitioned = null;
    }

    public async UniTask ManualFadeIn()
    {
        isTransitioning = true;
        _transitionPanelImage.raycastTarget = true;
        await Tween.Custom(Color.clear, Color.black, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal);
    }

    public async UniTask ManualFadeOut()
    {
        RunOnTransitioned();
        Tween tween = Tween.Custom(Color.black, Color.clear, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal);
        await tween;
        _transitionPanelImage.raycastTarget = false;
        isTransitioning = false;
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        _isGamePaused = true;
        Time.timeScale = 0f;

        uiLayers.Push("PauseMenu");
    }

    public void ResumeGame()
    {
        LockCursor(lastCursorState);
        pauseMenu.SetActive(false);
        _isGamePaused = false;
        Time.timeScale = 1f;
        PersistentDataManager.Player.GetComponent<PlayerInput>().enabled = true;
    }

    public void EnableQuestHUD()
    {
        if (sequence != null && sequence.Value.isAlive) sequence.Value.Stop();
        sequence = Sequence.Create();
        sequence.Value
            .Group(Tween.PositionX(questHUD.GetComponent<RectTransform>(), 0f, 1f))
            .Group(Tween.Alpha(questHUD.GetComponent<CanvasGroup>(), 1f, 1f));
    }
    public void DisableQuestHUD()
    {
        if (sequence != null && sequence.Value.isAlive) sequence.Value.Stop();
        sequence = Sequence.Create();
        sequence.Value
            .Group(Tween.PositionX(questHUD.GetComponent<RectTransform>(), -150f, 0.5f))
            .Group(Tween.Alpha(questHUD.GetComponent<CanvasGroup>(), 0f, 0.5f));
    }

    public void SetupPDA(string header, string body, Sprite image)
    {
        pdaHeader.text = header;
        pdaBody.text = body;
        pdaImage.sprite = image;
    }

    public void EnablePDA()
    {
        pdaMenu.SetActive(true);
        LockCursor(false);
        if (playerController != null) playerController.enabled = false;
    }

    public void DisablePDA()
    {
        pdaMenu.SetActive(false);
        LockCursor(lastCursorState);
        if (playerController != null) playerController.enabled = true;
    }

    public void HideAllPrompts()
    {
        GameObject.Find("BillboardCamera").GetComponent<Camera>().enabled = false;
    }

    public void ShowAllPrompts()
    {
        GameObject.Find("BillboardCamera").GetComponent<Camera>().enabled = true;
    }

    Sequence? subtitleSeq;
    public void DisplaySubtitle(string text)
    {
        subtitleText.text = text;
        subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, 1f);
        subtitleCanvasGroup.alpha = 1f;

        // Tween out the text opacity after 5 seconds
        subtitleSeq?.Stop();
        subtitleSeq = Sequence.Create()
            .ChainDelay(5f)
            .Chain(Tween.Alpha(subtitleCanvasGroup, 0f, duration: 5f))
            .OnComplete(() =>
            {
                subtitleText.text = "";
            });
    }

    public void OpenSettingsMenu()
    {
        onOpenSettings?.Invoke();

        settingsMenu.SetActive(true);
        LockCursor(false);

        uiLayers.Push("Settings");
        
        EnableInteractionHUD(InteractionHUDPreset.CUSTOM, "next,back,adjust");
    }

    public void CloseSettingsMenu()
    {
        onCloseSettings?.Invoke();

        settingsMenu.SetActive(false);
        LockCursor(false);
        DisableInteractionHUD();
    }

    public void EnableInteractionHUD()
    {
        interactionPromptHUD.SetActive(true);
    }

    public void DisableInteractionHUD()
    {
        interactionPromptHUD.GetComponent<InteractionPromptHUD>().DisablePrompts();
        interactionPromptHUD.SetActive(false);
    }
    public void EnableInteractionHUD(InteractionHUDPreset preset, string customInteractionString = "")
    {
        InteractionPromptHUD interactionPromptHUDScript = interactionPromptHUD.GetComponent<InteractionPromptHUD>();

        interactionPromptHUDScript.EnablePrompts(preset, customInteractionString);

        EnableInteractionHUD();
    }

    public void CloseMenu()
    {
        if (uiLayers.Count == 0) return;

        string currentLayer = (string)uiLayers.Pop();
        switch (currentLayer)
        {
            case "PauseMenu":
                ResumeGame();
                break;
            case "Settings":
                CloseSettingsMenu();
                if (pauseMenu.activeSelf)
                {
                    pauseMenu.GetComponent<PauseMenu>().ReselectFirst();
                }
                break;
        }
    }

    public string PeekUILayer()
    {
        if (uiLayers.Count == 0) return "No layers.";
        return (string)uiLayers.Peek();
    }

    public void AddUILayer(string uiLayerName)
    {
        uiLayers.Push(uiLayerName);
    }

    [Button(ButtonSizes.Large)]
    void ListLayers()
    {
        Stack duplicate = (Stack)uiLayers.Clone();
        int layerIdx = 0;
        while (duplicate.Count > 0)
        {
            string layer = (string)duplicate.Pop();
            Debug.Log($"{layerIdx}: {layer}");
            layerIdx++;
        }
    }
}
