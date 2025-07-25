using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using UnityEngine.Events;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _HUD;
    [SerializeField] GameObject _transitionPanel;
    [SerializeField] Image _transitionPanelImage;
    private PlayerController _playerController;
    public PlayerController playerController
    {
        get { return _playerController; }
        set
        {
            _playerController = value;
            _playerController.onInteracted += ToggleTransitionPanel;
        }
    }
    public static UIManager Instance { get; private set; }
    // public UnityEvent transitioned;
    public event Action onTransitioned;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(this.gameObject);

        _transitionPanelImage = _transitionPanel.GetComponent<Image>();
    }

    public void ToggleTransitionPanel(bool isInteracting = false)
    {
        _transitionPanelImage.raycastTarget = true;
        Sequence.Create(cycles: 1, CycleMode.Restart)
            .Chain(Tween.Custom(Color.clear, Color.black, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal))
            .ChainCallback(() => onTransitioned.Invoke())
            .Chain(Tween.Custom(Color.black, Color.clear, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal))
                .OnComplete(() =>
                {
                    _transitionPanelImage.raycastTarget = false;
                    _HUD.SetActive(isInteracting);
                });
    }
}
