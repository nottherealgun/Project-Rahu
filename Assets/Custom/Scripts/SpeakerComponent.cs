using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeakerComponent : SerializedMonoBehaviour
{
    public AudioSource voiceSource;
    [OdinSerialize] VoicelineManager.VoicelineData currentVoicelineData;
    [ReadOnly] public bool isSpeaking = false;
    [ReadOnly] public string currentDialogueLine;
    
    void Start()
    {
        if(voiceSource == null)
            gameObject.transform.Find("VoiceComponent").TryGetComponent(out voiceSource);
    }

    public void Speak(VoicelineManager.VoicelineData data, AudioClip audioClip)
    {
        
        voiceSource.clip = audioClip;
        currentVoicelineData = data;
        currentDialogueLine = data.dialogueText;
        
        StartCoroutine(PlayAndCheckDialogueCompletion());
    }

    public IEnumerator PlayAndCheckDialogueCompletion()
    {
        voiceSource.Play();
        isSpeaking = true;

        while (voiceSource.isPlaying)
            yield return null;

        isSpeaking = false;
    }
}
