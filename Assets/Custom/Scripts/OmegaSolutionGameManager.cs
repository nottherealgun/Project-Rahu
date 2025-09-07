using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class OmegaSolutionGameManager : SerializedMonoBehaviour
{
    [OdinSerialize] Image gaugeFiller;
    [OdinSerialize] GameObject indicator;
    [OdinSerialize] RectTransform topTransform;
    [OdinSerialize, ReadOnly] Vector2 bottomTransformAnchorPos;
    [OdinSerialize, ReadOnly] float indicatorLevel = 0f;
    [OdinSerialize, ReadOnly] bool boosting = false;
    [OdinSerialize, ReadOnly] float boostAcceleration = 0f;
    bool isGaugeFilled = false;
    float maxLevel = 100f;

    enum GaugeMode
    {
        NONE, FILLING_FAST, FILLING, DEPLETING
    }

    [OdinSerialize] GameObject yellowFiller;

    [OdinSerialize, ReadOnly] GaugeMode currentGaugeMode = GaugeMode.NONE;

    [Title("Progress Rates")]
    [OdinSerialize, DisableInPlayMode]
    [InfoBox("How fast the Right Bar\'s indicator accelerates per frame")]
    float boostAccelerationRate = 0.0005f;
    [OdinSerialize, DisableInPlayMode]
    [InfoBox("How fast the Right Bar\'s indicator decelerates per frame")]
    float boostDecelerationRate = 0.002f;
    [InfoBox("How fast the Left Bar\'s progress RISES per frame (Max prog. is 1.0)")]
    [OdinSerialize, DisableInPlayMode] float progressRiseRate = 0.0002f;
    [InfoBox("How fast the Left Bar\'s progress DROPS per frame (Max prog. is 1.0)")]
    [OdinSerialize, DisableInPlayMode] float progressDropRate = 0.0006f;

    void Start()
    {
        bottomTransformAnchorPos = indicator.GetComponent<RectTransform>().anchoredPosition;
        gaugeFiller.fillAmount = 0f;
        
        SetRandomYellowFillerPos();
    }

    private void SetRandomYellowFillerPos()
    {
        Vector3 oldPos = yellowFiller.GetComponent<RectTransform>().anchoredPosition;
        yellowFiller.GetComponent<RectTransform>().anchoredPosition = oldPos + Vector3.down * UnityEngine.Random.Range(0, 930f);
    }

    void Update()
    {
        indicatorLevel += boostAcceleration;
        CheckAndClampIndicatorLevel();
        CheckAndSetBoostAcceleration();
        AnimateVisuals();
        if (isGaugeFilled == false)
        {
            CheckIndicatorLevel();
            switch (currentGaugeMode)
            {
                case GaugeMode.FILLING_FAST:
                    gaugeFiller.fillAmount += progressRiseRate;
                    break;
                case GaugeMode.FILLING:
                    gaugeFiller.fillAmount += progressRiseRate / 3f;
                    break;
                case GaugeMode.DEPLETING:
                    gaugeFiller.fillAmount -= progressDropRate;
                    break;
            }
        }
        CheckGaugeFill();
    }

    void CheckAndClampIndicatorLevel()
    {
        if (indicatorLevel > maxLevel)
        {
            indicatorLevel = maxLevel;
            boostAcceleration = 0;
        }
        else if (indicatorLevel < 0f)
        {
            indicatorLevel = 0f;
            boostAcceleration = 0;
        }
        ;
    }

    void CheckAndSetBoostAcceleration()
    {
        if (boosting)
        {
            boostAcceleration += boostAccelerationRate;
        }
        else if (indicatorLevel > 0f)
        {
            boostAcceleration -= boostDecelerationRate;
        }
    }

    public void SetBoosting(bool value)
    {
        boosting = value;
    }

    public void CheckAndFinalizeGame()
    {
        if (isGaugeFilled) EndGame();
    }

    public void OnGreenFillEntered(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.FILLING_FAST;
    }

    public void OnGreenFillExited(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.FILLING;
    }

    public void OnYellowFillEntered(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.FILLING;
    }

    public void OnYellowFillExited(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.DEPLETING;
    }

    void AnimateVisuals()
    {
        float newY = indicatorLevel * Mathf.Abs(topTransform.anchoredPosition.y - bottomTransformAnchorPos.y) / maxLevel;
        indicator.GetComponent<RectTransform>().anchoredPosition = bottomTransformAnchorPos + new Vector2(0f, newY);
    }

    void CheckIndicatorLevel()
    {
        if (currentGaugeMode == GaugeMode.DEPLETING && gaugeFiller.fillAmount <= 0)
        {
            currentGaugeMode = GaugeMode.NONE;
        }
        ;

    }

    void CheckGaugeFill()
    {
        if (gaugeFiller.fillAmount == 1f)
        {
            isGaugeFilled = true;
        }
    }

    [HorizontalGroup("A"), Button(ButtonSizes.Large), LabelText("Instant Win"), DisableInEditorMode]
    void EndGame()
    {
        gameObject.GetComponent<PuzzleCompletionEmitter>().onPuzzleCompleted?.Invoke();
        UIManager.SetCursorState(true);
    }
    [HorizontalGroup("A"), Button("Reset Values", ButtonSizes.Large), DisableInPlayMode]
    void ResetProgressRates()
    {
        boostAccelerationRate = 0.0005f;
        boostDecelerationRate = 0.002f;
        progressRiseRate = 0.0002f;
        progressDropRate = 0.0006f;
    }
}
