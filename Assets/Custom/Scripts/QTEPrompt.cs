using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QTEPrompt : SerializedMonoBehaviour
{
    public enum KeyPrompt { QTE1, QTE2, QTE3, QTE4 }
    // S W A D
    public struct KeyPromptData
    {
        public Sprite keyboardSprite;
        public Sprite xboxSprite;
        public Sprite dualshockSprite;
        public KeyPromptData(Sprite keyboardSprite, Sprite xboxSprite, Sprite dualshockSprite)
        {
            this.keyboardSprite = keyboardSprite;
            this.xboxSprite = xboxSprite;
            this.dualshockSprite = dualshockSprite;
        }
    }
    public UnityEvent onQTECompleted;
    public UnityEvent onQTEVanished;
    public UnityEvent onQTESucceed;
    public UnityEvent onQTEFailed;
    [OdinSerialize] Transform uiContainer;
    [OdinSerialize] Image progressBar;
    [OdinSerialize] Image keyPromptIcon;
    [OdinSerialize] Transform choosingCircle;
    [TabGroup("QTE Key Sprites"), OdinSerialize]
    Dictionary<KeyPrompt, KeyPromptData> keyPromptSprites = new Dictionary<KeyPrompt, KeyPromptData>
    {
        { KeyPrompt.QTE1, new KeyPromptData()},
        { KeyPrompt.QTE2, new KeyPromptData()},
        { KeyPrompt.QTE3, new KeyPromptData()},
        { KeyPrompt.QTE4, new KeyPromptData()}
    };
    KeyPrompt keyPromptType;
    Sequence sequence;
    Tween failTween;
    bool pressed = false;
    public void Setup(Vector3 onScreenPosition, KeyPrompt _keyPromptType)
    {
        uiContainer.GetComponent<RectTransform>().anchoredPosition = onScreenPosition;
        keyPromptType = _keyPromptType;
        keyPromptIcon.sprite = keyPromptSprites[keyPromptType].keyboardSprite;
    }

    void Start()
    {
        onQTESucceed.AddListener(() => onQTECompleted?.Invoke());
        onQTEFailed.AddListener(() => onQTECompleted?.Invoke());
    }

    public void Show()
    {
        Sequence sequence = Sequence.Create()
            .Chain(Tween.Scale(uiContainer,1.3f,0.2f))
            .Chain(Tween.Scale(uiContainer,1f,0.5f));
        failTween = Tween.Custom(0f, 1f, 5f, onValueChange: (value) => progressBar.fillAmount = value, Ease.Linear)
        .OnComplete(async () =>
        {
            // If didn't press anything, fail
            await FailQTE().ToUniTask();
            Destroy(this.gameObject);
        });
        transform.Find("QTEShow").GetComponent<AudioSource>().Play();
    }

    void OnDestroy()
    {
        Tween.StopAll(this);
        sequence.Stop();
        onQTEVanished?.Invoke();
    }

    public void OnQTE1(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnQTEPress(KeyPrompt.QTE1);
        }
    }

    public void OnQTE2(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnQTEPress(KeyPrompt.QTE2);
        }
    }

    public void OnQTE3(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnQTEPress(KeyPrompt.QTE3);
        }
    }

    public void OnQTE4(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnQTEPress(KeyPrompt.QTE4);
        }
    }

    async void OnQTEPress(KeyPrompt keyPrompt)
    {
        if (pressed) return;
        pressed = true;
        failTween.isPaused = true;

        // If press correct key prompt
        if (keyPrompt == keyPromptType)
        {
            await SucceedQTE().ToUniTask();
        }

        // otherwise if press incorrect key prompt
        else
        {
            await FailQTE().ToUniTask();
        }

        Destroy(this.gameObject);
    }

    Sequence SucceedQTE()
    {
        choosingCircle.localScale = Vector3.one * 1.3f;
        sequence = Sequence.Create()
            .Chain(Tween.Scale(choosingCircle, 1.25f, 0.25f))
            .ChainDelay(1.5f)
            .Chain(Tween.Alpha(GetComponent<CanvasGroup>(), 0f, 2f));

        progressBar.GetComponent<Image>().color = Color.green;
        choosingCircle.GetComponent<Image>().color = Color.green;
        onQTESucceed?.Invoke();
        transform.Find("QTESucceed").GetComponent<AudioSource>().Play();

        return sequence;
    }

    Sequence FailQTE()
    {
        choosingCircle.localScale = Vector3.one * 1.3f;
        sequence = Sequence.Create()
            .Chain(Tween.Scale(choosingCircle, 1.25f, 0.25f))
            .ChainDelay(1.5f)
            .Chain(Tween.Alpha(GetComponent<CanvasGroup>(), 0f, 2f));

        progressBar.GetComponent<Image>().color = Color.red;
        choosingCircle.GetComponent<Image>().color = Color.red;
        onQTEFailed?.Invoke();
        transform.Find("QTEFail").GetComponent<AudioSource>().Play();

        return sequence;
    }
}
