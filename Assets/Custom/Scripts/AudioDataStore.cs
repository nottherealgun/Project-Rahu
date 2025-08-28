using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class AudioDataStore : MonoBehaviour
{
    public static AudioDataStore Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public struct DialogueLine
    {
        [GUIColor("yellow")]
        public string text;
        [GUIColor("yellow")]
        public AudioClip audioFile;
        public DialogueLine(string _text, AudioClip _audioFile)
        {
            text = _text;
            audioFile = _audioFile;
        }
    }

    [Button(ButtonSizes.Large)]
    public DialogueStore scenes;
}
