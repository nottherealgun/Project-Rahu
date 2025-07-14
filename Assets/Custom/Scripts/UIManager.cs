using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _transitionPanel;
    [SerializeField] Image _transitionPanelImage;
    public static UIManager Instance { get; private set; }
    public UnityEvent transitioned;
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

    public void ToggleTransitionPanel()
    {
        Sequence.Create(cycles: 1, CycleMode.Restart)
            .Chain(Tween.Custom(Color.clear, Color.black, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal))
            .ChainCallback(() => transitioned.Invoke())
            .Chain(Tween.Custom(Color.black, Color.clear, duration: 0.5f, onValueChange: newVal => _transitionPanelImage.color = newVal));
    }
}
