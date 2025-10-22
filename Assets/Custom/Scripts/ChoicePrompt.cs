using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ChoicePrompt : SerializedMonoBehaviour
{
    public static ChoicePrompt Instance { get; private set; }
    enum ChoiceType { LEFT, RIGHT, NONE }
    [OdinSerialize, ReadOnly] ChoiceType choice = ChoiceType.NONE;
    bool choiceSelected { get { return choice != ChoiceType.NONE; } }
    [OdinSerialize] CinemachineCamera choiceCamera;
    [Title("Left Choice")]
    public UnityEvent onLeftChosen;
    [OdinSerialize] GameObject leftChoosingCircle;
    [OdinSerialize] TMP_Text leftChoiceHeader;
    [OdinSerialize] TMP_Text leftChoiceBody;
    [OdinSerialize, ShowIf("@leftChoiceHeader != null")] string leftChoiceTextHeader;
    [OdinSerialize, ShowIf("@leftChoiceBody != null")] string leftChoiceTextBody;
    [Title("Right Choice")]
    public UnityEvent onRightChosen;
    [OdinSerialize] GameObject rightChoosingCircle;
    [OdinSerialize] TMP_Text rightChoiceHeader;
    [OdinSerialize] TMP_Text rightChoiceBody;
    [OdinSerialize, ShowIf("@rightChoiceHeader != null")] string rightChoiceTextHeader;
    [OdinSerialize, ShowIf("@rightChoiceBody != null")] string rightChoiceTextBody;
    bool leftPressed = false;
    bool rightPressed = false;
    Tween? leftTween;
    Tween? rightTween;
    float scalingSpeed = 0.03f;
    Vector3 min = Vector3.zero;
    Vector3 max = Vector3.one;
    [Title("SFX Objects")]
    [OdinSerialize, Required] AudioSource choiceSelectedSFX;
    [OdinSerialize, Required] AudioSource choiceStartSelectingSFX;

    private void OnValidate()
    {
        leftChoiceHeader.text = leftChoiceTextHeader;
        rightChoiceHeader.text = rightChoiceTextHeader;
        leftChoiceBody.text = leftChoiceTextBody;
        rightChoiceBody.text = rightChoiceTextBody;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        // DontDestroyOnLoad(this.gameObject);
    }
    
    void Start()
    {
        if (PersistentDataManager.Player != null)
        {
            PersistentDataManager.Player.GetComponent<PlayerController>().LockCamera();
            choiceCamera.Target.TrackingTarget = PersistentDataManager.Player.transform.Find("PlayerCameraRoot").transform;
        }
        
        UIManager.LockCursor(true);

        onLeftChosen.AddListener(choiceSelectedSFX.Play);
        onRightChosen.AddListener(choiceSelectedSFX.Play);
    }

    async void Update()
    {
        if (choiceSelected) return;
        if (leftPressed)
        {
            Vector3 newScale = new Vector3 ();
            newScale.x = Mathf.Clamp(leftChoosingCircle.transform.localScale.x + scalingSpeed, min.x, max.x);
            newScale.y = Mathf.Clamp(leftChoosingCircle.transform.localScale.y + scalingSpeed, min.y, max.y);
            newScale.z = Mathf.Clamp(leftChoosingCircle.transform.localScale.z + scalingSpeed, min.z, max.z);
            leftChoosingCircle.transform.localScale = newScale;

            if (newScale.x == 1f) {
                choice = ChoiceType.LEFT;
                onLeftChosen.AddListener(async () => await ResetChoices()); 
                onLeftChosen.AddListener(async () =>
                {
                    await Tween.Scale(leftChoosingCircle.transform,1.3f,0.1f);
                    await Tween.Scale(leftChoosingCircle.transform,1f,1f);
                }); 
                onLeftChosen?.Invoke();
            }
        }
        else if (rightPressed)
        {
            Vector3 newScale = new Vector3 ();
            newScale.x = Mathf.Clamp(rightChoosingCircle.transform.localScale.x + scalingSpeed, min.x, max.x);
            newScale.y = Mathf.Clamp(rightChoosingCircle.transform.localScale.y + scalingSpeed, min.y, max.y);
            newScale.z = Mathf.Clamp(rightChoosingCircle.transform.localScale.z + scalingSpeed, min.z, max.z);
            rightChoosingCircle.transform.localScale = newScale;

            if (newScale.x == 1f) {
                choice = ChoiceType.RIGHT;
                onRightChosen.AddListener(async () => await ResetChoices()); 
                onRightChosen.AddListener(async () =>
                {
                    await Tween.Scale(rightChoosingCircle.transform,1.3f,0.1f);
                    await Tween.Scale(rightChoosingCircle.transform,1f,1f);
                }); 
                onRightChosen?.Invoke();
                
            }
        }
    }

    public void OnLeft(InputAction.CallbackContext context)
    {
        if (choiceSelected) return;

        leftTween?.Stop();
        if (context.started)
        {
            leftPressed = true;
            choiceStartSelectingSFX.Play();
        }
        else if (context.canceled)
        {
            leftTween = Tween.Scale(leftChoosingCircle.transform, Vector2.zero, 2f);
            leftPressed = false;
        }
    }

    public void OnRight(InputAction.CallbackContext context)
    {
        if (choiceSelected) return;

        rightTween?.Stop();
        if (context.started)
        {
            rightPressed = true;
            choiceStartSelectingSFX.Play();
        }
        else if (context.canceled)
        {
            rightTween = Tween.Scale(rightChoosingCircle.transform, Vector2.zero, 2f);
            rightPressed = false;
        }
    }

    async UniTask ResetChoices()
    {
        leftPressed = false;
        rightPressed = false;
        leftTween?.Stop();
        rightTween?.Stop();

        choiceCamera.gameObject.SetActive(false);

        Sequence sequence = Sequence.Create();
        await sequence.Chain(Tween.Alpha(GetComponent<CanvasGroup>(), 0f, 2f));

        UIManager.RevertCursorState();
        PersistentDataManager.Player?.GetComponent<PlayerController>().UnlockCamera();
        Destroy(this.gameObject);
    }
}
