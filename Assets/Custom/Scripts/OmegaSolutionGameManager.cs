using System;
using System.Collections;
using System.Collections.Generic;
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
    [OdinSerialize] Button ejectButton;
    [OdinSerialize, ReadOnly] GaugeMode currentGaugeMode = GaugeMode.NONE;
    [Title("Progress Rates")]
    [OdinSerialize]
    [InfoBox("How fast the Right Bar\'s indicator accelerates per frame")]
    float boostAccelerationRate = 0.0005f;
    [OdinSerialize]
    [InfoBox("How fast the Right Bar\'s indicator decelerates per frame")]
    float boostDecelerationRate = 0.002f;
    [InfoBox("How fast the Left Bar\'s progress RISES per frame (Max prog. is 1.0)")]
    [OdinSerialize] float progressRiseRate = 0.0002f;
    [InfoBox("How fast the Left Bar\'s progress DROPS per frame (Max prog. is 1.0)")]
    [OdinSerialize] float progressDropRate = 0.0006f;

    string currentlyPlayingBoilingSFX = "";
    const int HIGH_BOILING_POINT_IDX = 2;
    const int MID_BOILING_POINT_IDX = 1;
    const int LOW_BOILING_POINT_IDX = 0;
    List<string> boilingSFXNames = new List<string> { "chemical_boil_low", "chemical_boil_mid", "chemical_boil_high" };

    Vector3 startingFillerPos;
    Vector3 currentFillerPos;

    int round = 1;

    void Start()
    {
        bottomTransformAnchorPos = indicator.GetComponent<RectTransform>().anchoredPosition;
        gaugeFiller.fillAmount = 0f;
        startingFillerPos = yellowFiller.GetComponent<RectTransform>().anchoredPosition;
        currentFillerPos = startingFillerPos;

        SetRandomYellowFillerPos();
    }

    private void SetRandomYellowFillerPos()
    {
        Vector3 previousFillerPos = currentFillerPos;
        while (Math.Abs(previousFillerPos.y - currentFillerPos.y) <= 150f)
        {
            yellowFiller.GetComponent<RectTransform>().anchoredPosition = startingFillerPos + Vector3.down * UnityEngine.Random.Range(0, 930f);
            currentFillerPos = yellowFiller.GetComponent<RectTransform>().anchoredPosition;
        }
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

    void OnDestroy()
    {
        LeaveGame();
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
        PlayBoilingSFX(HIGH_BOILING_POINT_IDX);
    }

    public void OnGreenFillExited(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.FILLING;
        PlayBoilingSFX(MID_BOILING_POINT_IDX);
    }

    public void OnYellowFillEntered(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.FILLING;
        PlayBoilingSFX(MID_BOILING_POINT_IDX);
    }

    public void OnYellowFillExited(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.DEPLETING;
        PlayBoilingSFX(LOW_BOILING_POINT_IDX);
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
            if (IsChemicalsComplete())
            {
                isGaugeFilled = true;
                ejectButton.interactable = true;
                return;
            }
            StartNewRound();
        }
    }

    void LeaveGame()
    {
        UIManager.LockCursor(true);
        StopBoilingSFX();
        EnvironmentalAudioManager.Instance.StopMusic();
    }

    [HorizontalGroup("A"), Button(ButtonSizes.Large), LabelText("Instant Win"), DisableInEditorMode]
    void EndGame()
    {
        gameObject.GetComponent<PuzzleCompletionEmitter>().onPuzzleCompleted?.Invoke();
        UIManager.LockCursor(true);
        StopBoilingSFX();
    }

    [HorizontalGroup("A"), Button("Reset Values", ButtonSizes.Large), DisableInPlayMode]
    void ResetProgressRates()
    {
        boostAccelerationRate = 0.0005f;
        boostDecelerationRate = 0.002f;
        progressRiseRate = 0.0002f;
        progressDropRate = 0.0006f;
    }

    void PlayBoilingSFX(int boilingLevel)
    {
        string newBoilingSFX = boilingSFXNames[boilingLevel];

        // If the same boiling SFX is already playing, do nothing
        if (newBoilingSFX == currentlyPlayingBoilingSFX) return;

        // If a different boiling SFX is playing, stop it first
        else if (currentlyPlayingBoilingSFX != "")
        {
            EnvironmentalAudioManager.Instance.StopLoopingSFX(currentlyPlayingBoilingSFX);
        }

        // Play the new boiling SFX
        currentlyPlayingBoilingSFX = newBoilingSFX;
        EnvironmentalAudioManager.Instance.PlayLoopingSFX(newBoilingSFX);
    }

    void StopBoilingSFX()
    {
        if (currentlyPlayingBoilingSFX != "")
        {
            EnvironmentalAudioManager.Instance.StopLoopingSFX(currentlyPlayingBoilingSFX);
            currentlyPlayingBoilingSFX = "";
        }
    }

    bool IsChemicalsComplete()
    {
        return round > 3;
    }

    void StartNewRound()
    {
        round++;
        SetRandomYellowFillerPos();
        gaugeFiller.fillAmount = 0;
    }
}
