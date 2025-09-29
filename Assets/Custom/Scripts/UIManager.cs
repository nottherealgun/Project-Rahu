using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using UnityEngine.Events;
using System.Threading.Tasks;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

public class UIManager : SerializedMonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public static event UnityAction OnTransitioned;
    [OdinSerialize] GameObject _HUD;
    [OdinSerialize] GameObject _transitionPanel;
    [OdinSerialize] Image _transitionPanelImage;
    [OdinSerialize] GameObject pauseMenu;
    PlayerController playerController;
    static bool _isGamePaused = false;
    public static bool isGamePaused { get { return _isGamePaused; } }
    public static bool lastCursorState = true;

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

    public async Task<Tween> ManualFadeOut()
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
    }

    public void ResumeGame()
    {
        LockCursor(lastCursorState);
        pauseMenu.SetActive(false);
        _isGamePaused = false;
        Time.timeScale = 1f;
    }

    public void ShowInteractionPrompt()
    {
        if (_HUD.gameObject.activeInHierarchy) return;
        _HUD.gameObject.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (_HUD.gameObject.activeInHierarchy == false) return;
        _HUD.gameObject.SetActive(false);
    }
}
