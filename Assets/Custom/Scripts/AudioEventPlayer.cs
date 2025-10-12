using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class AudioEventPlayer : SerializedMonoBehaviour
{
    [OdinSerialize] List<string> audioTracks = new List<string>();

    public void PlayAudio()
    {
        foreach(string audioTrack in audioTracks)
        {
            EnvironmentalAudioManager.Instance.PlaySFX(audioTrack);
        }
    }
}
