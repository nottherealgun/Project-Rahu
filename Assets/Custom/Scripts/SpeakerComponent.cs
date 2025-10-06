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
    Queue voicelineQueue = new Queue();
    void Start()
    {
        if(voiceSource == null)
            gameObject.transform.Find("VoiceComponent").TryGetComponent(out voiceSource);
    }

    public void Speak(VoicelineManager.VoicelineData data, AudioClip audioClip)
    {
        voicelineQueue.Enqueue((data, audioClip));

        if(isSpeaking) return;

        StartCoroutine(PlayDialogueQueue());
    }

    IEnumerator PlayDialogueQueue()
    {
        isSpeaking = true;

        while (voicelineQueue.Count > 0)
        {
            var currentTuple = ((VoicelineManager.VoicelineData, AudioClip))voicelineQueue.Dequeue();
            currentVoicelineData = currentTuple.Item1;
            voiceSource.clip = currentTuple.Item2;
            currentDialogueLine = currentVoicelineData.dialogueText;

            UIManager.Instance.DisplaySubtitle($"{currentVoicelineData.speaker}: {currentDialogueLine}");
            yield return PlayAndCheckDialogueCompletion();
        }

        isSpeaking = false;
    }

    IEnumerator PlayAndCheckDialogueCompletion()
    {
        voiceSource.Play();

        while (voiceSource.isPlaying)
            yield return null;
    }
}
