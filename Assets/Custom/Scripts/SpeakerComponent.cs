using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeakerComponent : SerializedMonoBehaviour
{
    [OdinSerialize] AudioSource voiceSource;
    [OdinSerialize] VoicelineManager.VoicelineData currentVoicelineData;
    [OdinSerialize, ReadOnly] bool isSpeaking = false;
    [OdinSerialize, ReadOnly] string currentDialogueLine;

    void Start()
    {
        if(voiceSource == null)
            gameObject.transform.Find("VoiceComponent").TryGetComponent(out voiceSource);
    }

    public void Speak(VoicelineManager.VoicelineData data, AudioClip audioClip)
    {
        currentVoicelineData = data;
        voiceSource.clip = audioClip;
        currentDialogueLine = data.dialogueText;
        StartCoroutine(PlayAndCheckDialogueCompletion());
    }

    IEnumerator PlayAndCheckDialogueCompletion()
    {
        voiceSource.Play();
        isSpeaking = true;

        while (voiceSource.isPlaying)
            yield return null;

        isSpeaking = false;
    }
}
