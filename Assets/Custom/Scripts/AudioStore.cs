using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioStore", menuName = "Project Rahu/AudioStore")]
public class AudioStore : SerializedScriptableObject
{
    [Button(ButtonSizes.Gigantic)]
    [TabGroup("SFX")]
    [OdinSerialize] public Dictionary<string, List<AudioClip>> sfx = new Dictionary<string, List<AudioClip>>();
    [TabGroup("Music")]
    [OdinSerialize] public Dictionary<string, AudioClip> music = new Dictionary<string, AudioClip>();
    [TabGroup("Ambience")]
    [OdinSerialize] public Dictionary<string, AudioClip> ambience = new Dictionary<string, AudioClip>();
}
