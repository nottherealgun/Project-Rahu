using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OmegaSolutionGameManager : SerializedMonoBehaviour
{
    [OdinSerialize] Image gaugeFiller;
    [OdinSerialize] Image beakerFiller;
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
    [OdinSerialize, AssetsOnly] Sprite ejectButtonPressed;
    [OdinSerialize, AssetsOnly] Sprite ejectButtonNormal;
    [OdinSerialize] Button boostButton;
    [OdinSerialize, AssetsOnly] Sprite boostButtonPressed;
    [OdinSerialize, AssetsOnly] Sprite boostButtonNormal;
    [OdinSerialize, ReadOnly] GaugeMode currentGaugeMode = GaugeMode.NONE;
    [Title("Progress Rates")]
    // [OdinSerialize]
    // [InfoBox("How fast the Right Bar\'s indicator accelerates per frame")]
    // float boostAccelerationRate = 0.0005f;
    // [OdinSerialize]
    [InfoBox("How fast the Right Bar\'s indicator decelerates per frame")]
    [OdinSerialize] float boostDecelerationRate = 0.02f;
    [InfoBox("How fast the Left Bar\'s progress RISES per frame (Max prog. is 1.0)")]
    [OdinSerialize] float progressRiseRate = 0.0025f;
    [InfoBox("How fast the Left Bar\'s progress DROPS per frame (Max prog. is 1.0)")]
    [OdinSerialize] float progressDropRate = 0.0025f;

    string currentlyPlayingBoilingSFX = "";
    const int HIGH_BOILING_POINT_IDX = 2;
    const int MID_BOILING_POINT_IDX = 1;
    const int LOW_BOILING_POINT_IDX = 0;
    List<string> boilingSFXNames = new List<string> { "chemical_boil_low", "chemical_boil_mid", "chemical_boil_high" };

    Vector2 startingFillerPos;
    Vector2 currentFillerPos;

    int round = 0;
    Tween? tween;

    void Start()
    {
        bottomTransformAnchorPos = indicator.GetComponent<RectTransform>().anchoredPosition;
        gaugeFiller.fillAmount = 0f;
        startingFillerPos = yellowFiller.GetComponent<RectTransform>().anchoredPosition;
        currentFillerPos = startingFillerPos;

        StartCoroutine(SetRandomYellowFillerPos());
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
                    gaugeFiller.fillAmount += progressRiseRate / 3;
                    break;
                case GaugeMode.FILLING:
                    gaugeFiller.fillAmount += progressRiseRate / 9;
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
        if (tween != null & tween.Value.isAlive) tween.Value.Stop();
        LeaveGame();
    }

    private IEnumerator SetRandomYellowFillerPos()
    {
        while (IsChemicalsComplete() == false)
        {
            Vector2 newPos = Vector2.zero;
            while (Math.Abs(currentFillerPos.y - newPos.y) <= 150f)
            {
                newPos = startingFillerPos + Vector2.down * UnityEngine.Random.Range(100, 340);
            }
            // yellowFiller.GetComponent<RectTransform>().anchoredPosition
            RectTransform rt = yellowFiller.GetComponent<RectTransform>();
            tween = Tween.Custom(rt.anchoredPosition, newPos, 5.0f, t => rt.anchoredPosition = t, Ease.OutSine);

            // currentGaugeMode = GaugeMode.DEPLETING;

            yield return tween;
        }
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
            // boostAcceleration += boostAccelerationRate;
            boostAcceleration = 0.2f;
        }
        else if (indicatorLevel > 0f)
        {
            boostAcceleration -= boostDecelerationRate;
            // boostAcceleration = -0.35f;
        }
    }

    public void OnBoost(InputValue value)
    {
        SetBoosting(value.isPressed);
        if (value.isPressed)
        {
            boostButton.image.sprite = boostButtonPressed;
        }
        else
        {
            boostButton.image.sprite = boostButtonNormal;
        }
    }

    public void OnEject(InputValue value)
    {
        if (value.isPressed)
        {
            CheckAndFinalizeGame();
            ejectButton.image.sprite = ejectButtonPressed;
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

    public void OnYellowFillEntered(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.FILLING;
        PlayBoilingSFX(MID_BOILING_POINT_IDX);
    }

    public void OnRedFillEntered(GameObject triggerObj)
    {
        currentGaugeMode = GaugeMode.DEPLETING;
        PlayBoilingSFX(LOW_BOILING_POINT_IDX);
    }

    void AnimateVisuals()
    {
        float newY = indicatorLevel * Mathf.Abs(topTransform.anchoredPosition.y - bottomTransformAnchorPos.y) / maxLevel;
        indicator.GetComponent<RectTransform>().anchoredPosition = bottomTransformAnchorPos + new Vector2(0f, newY);

        gaugeFiller.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f + gaugeFiller.fillAmount);
        beakerFiller.GetComponent<Image>().color = new Color(1f, gaugeFiller.fillAmount, gaugeFiller.fillAmount, 1f);
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
            // StartNewRound();
            round++;
            if (IsChemicalsComplete() && isGaugeFilled == false)
            {
                EnvironmentalAudioManager.Instance.PlaySFX("puzzle_complete");
                tween?.Stop();
                isGaugeFilled = true;
                ejectButton.interactable = true;
                boostButton.interactable = false;
                return;
            }
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

        if (PersistentDataManager.Instance.HasEventPassed("chemicalGameFinished")) return;
        NarrativeManager.Instance.CharacterSpeak("SC12_Fasai_Chemical_Finish");
        PersistentDataManager.Instance.MarkEventAsPassed("chemicalGameFinished");
    }

    // [HorizontalGroup("A"), Button("Reset Values", ButtonSizes.Large), DisableInPlayMode]
    // void ResetProgressRates()
    // {
    //     // boostAccelerationRate = 0.0005f;
    //     // boostDecelerationRate = 0.002f;
    //     progressRiseRate = 0.0002f;
    //     progressDropRate = 0.0006f;
    // }

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
        return round > 0;
    }

    void StartNewRound()
    {
        round++;
        SetRandomYellowFillerPos();
        gaugeFiller.fillAmount = 0;
    }
}
