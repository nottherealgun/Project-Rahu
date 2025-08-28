using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using UnityEngine.Events;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _HUD;
    [SerializeField] GameObject _transitionPanel;
    [SerializeField] Image _transitionPanelImage;
    private PlayerController playerController;
    public static UIManager Instance { get; private set; }
    // public UnityEvent transitioned;
    public static event UnityAction OnTransitioned;
    private void Awake()
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

    private void Start()
    {
        PersistentDataManager.Instance.OnPlayerFound += OnPlayerSearchStatus;
    }

    public static void SetCursorState(bool newState)
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
}
