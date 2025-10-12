using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections;

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
    void SetSubtitles(bool value)
    {
        _subtitlesOn = value;
        if (!value) subtitleText.gameObject.SetActive(false);
        else subtitleText.gameObject.SetActive(true);
    }
    [OdinSerialize] TMP_Text subtitleText;

    Stack uiLayers = new Stack();

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
        PersistentDataManager.Instance.OnPlayerFound += OnPlayerSearchStatus;
        settingsMenu.GetComponent<SettingsMenu>().Initialize();
    }

    [Button(ButtonSizes.Large)]
    public static void LockCursor(bool newState)
    {
        // true = locked, false = unlocked
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void OnPlayerSearchStatus(bool found)
    {
        if (found) playerController = PersistentDataManager.Player.GetComponent<PlayerController>();
    }

    public void ToggleTransitionPanel(bool isInteracting = false)
    {
        _transitionPanelImage.raycastTarget = true;
        Sequence.Create(cycles: 1, CycleMode.Restart)
            .Chain(Tween.Custom(Color.clear, Color.black, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal))
            .ChainCallback(() => RunOnTransitioned())
            .Chain(Tween.Custom(Color.black, Color.clear, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal))
            .OnComplete(() =>
            {
                _transitionPanelImage.raycastTarget = false;
                // _HUD.SetActive(isInteracting);
            });
    }

    public async UniTask<Tween> ManualFadeOut()
    {
        Tween tween = Tween.Custom(Color.black, Color.clear, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal);
        await tween;
        RunOnTransitioned();
        return tween;
    }

    public void RunOnTransitioned()
    {
        OnTransitioned?.Invoke();
        OnTransitioned = null;
    }

    public Tween ManualFadeIn()
    {
        return Tween.Custom(Color.clear, Color.black, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal);
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
    }

    public void SetupPDA(string header, string body, Sprite image)
    {
        pdaHeader.text = header;
        pdaBody.text = body;
        pdaImage.sprite = image;
    }

    public void OpenPDA()
    {
        pdaMenu.SetActive(true);
        LockCursor(false);
        if (playerController != null) playerController.enabled = false;

        uiLayers.Push("PDA");
    }

    public void ClosePDA()
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

        // Tween out the text opacity after 5 seconds
        subtitleSeq?.Stop();
        subtitleSeq = Sequence.Create()
            .ChainDelay(5f)
            .Chain(Tween.Custom(1f, 0f, duration: 5f, onValueChange: newVal => subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, newVal)))
            .OnComplete(() =>
            {
                subtitleText.text = "";
                subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, 1f);
            });
    }

    public void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true);
        LockCursor(false);

        uiLayers.Push("Settings");
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
        LockCursor(false);
    }

    public void CloseMenu()
    {
        if (uiLayers.Count == 0) return;

        string currentLayer = (string)uiLayers.Pop();
        switch (currentLayer)
        {
            case "PDA":
                ClosePDA();
                break;
            case "PauseMenu":
                ResumeGame();
                break;
            case "Settings":
                CloseSettingsMenu();
                break;
        }
    }

    public string PeekUILayer()
    {
        return (string) uiLayers.Peek();
    }
    
    public void AddUILayer(string uiLayerName)
    {
        uiLayers.Push(uiLayerName);
    }
}
