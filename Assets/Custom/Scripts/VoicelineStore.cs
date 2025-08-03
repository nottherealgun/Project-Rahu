using UnityEngine;
using Sirenix.Serialization;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "VoicelineStore", menuName = "Project Rahu/VoicelineStore")]
public class VoicelineStore : SerializedScriptableObject
{
    [OdinSerialize] public List<AudioDataStore.DialogueLine> cutsceneVoicelines = new List<AudioDataStore.DialogueLine>();
    [OdinSerialize] public Dictionary<String,AudioDataStore.DialogueLine> eventBasedVoicelines = new Dictionary<string, AudioDataStore.DialogueLine>();
}
