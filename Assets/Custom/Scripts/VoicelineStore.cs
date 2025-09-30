using UnityEngine;
using Sirenix.Serialization;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

public enum Character { FASAI, NATE, NUENG, SOMCHAI }

[CreateAssetMenu(fileName = "VoicelineStore", menuName = "Project Rahu/VoicelineStore")]
public class VoicelineStore : SerializedScriptableObject
{
    public List<AudioDataStore.DialogueLine> cutsceneVoicelines = new List<AudioDataStore.DialogueLine>();
    public Dictionary<Character, List<AudioDataStore.DialogueLine>> eventBasedVoicelines = new Dictionary<Character, List<AudioDataStore.DialogueLine>>();
    public Dictionary<string, List<AudioDataStore.DialogueLine>> randomVoicelinePools = new Dictionary<string, List<AudioDataStore.DialogueLine>>();
}
