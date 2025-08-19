using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;
using Unity.Cinemachine;

public class OmegaSolutionGameManager : SerializedMonoBehaviour
{
    [OdinSerialize] Image gaugeFiller;
    [OdinSerialize] GameObject indicator;
    [OdinSerialize] RectTransform topTransform;
    [OdinSerialize, ReadOnly] Vector2 bottomTransformAnchorPos;
    [OdinSerialize, ReadOnly] float indicatorLevel = 0f;
    [OdinSerialize, ReadOnly] bool boosting = false;
    [OdinSerialize, ReadOnly, LabelText("In Green Area")] bool inRange = false;
    [OdinSerialize, ReadOnly] float boostAcceleration = 0f;

    float maxLevel = 100f;
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
    [Button("Reset Values", ButtonSizes.Medium),DisableInPlayMode]
    void ResetProgressRates()
    {
        boostAccelerationRate = 0.0005f;
        boostDecelerationRate = 0.002f;
        progressRiseRate = 0.0002f;
        progressDropRate = 0.0006f;
    }
    
    private void Start()
    {
        bottomTransformAnchorPos = indicator.GetComponent<RectTransform>().anchoredPosition;
        gaugeFiller.fillAmount = 0f;
    }
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     boosting = true;
        // }
        // else if (Input.GetKeyUp(KeyCode.E))
        // {
        //     boosting = false;
        // }
        indicatorLevel += boostAcceleration;
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

        if (boosting)
        {
            boostAcceleration += boostAccelerationRate;
        }
        else if (indicatorLevel > 0f)
        {
            boostAcceleration -= boostDecelerationRate;
        }

        AnimateVisuals();
        LevelCheck();
        WinCheck();
    }

    public void SetBoosting(bool value)
    {
        boosting = value;
    }

    void AnimateVisuals()
    {
        float newY = indicatorLevel * Mathf.Abs(topTransform.anchoredPosition.y - bottomTransformAnchorPos.y) / maxLevel;
        indicator.GetComponent<RectTransform>().anchoredPosition = bottomTransformAnchorPos + new Vector2(0f, newY);
    }

    void LevelCheck()
    {
        if (indicatorLevel < 80 && indicatorLevel > 70)
        {
            inRange = true;
            gaugeFiller.fillAmount += progressRiseRate;
        }
        else if (indicatorLevel < 95 && indicatorLevel > 60) {
            inRange = true;
            gaugeFiller.fillAmount += progressRiseRate/3f;
        }
        else
        {
            inRange = false;
            gaugeFiller.fillAmount -= progressDropRate;
        }
        ;

    }

    void WinCheck()
    {
        if (gaugeFiller.fillAmount == 1f)
        {
            EndGame();
        }
    }
    
    void EndGame()
    {
        gameObject.GetComponent<PuzzleCompletionEmitter>().onPuzzleCompleted?.Invoke();
    }
}
