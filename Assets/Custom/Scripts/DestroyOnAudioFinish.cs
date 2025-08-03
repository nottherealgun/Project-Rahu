using System.Collections;
using UnityEngine;

public class DestroyOnAudioFinish : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void CheckAudioFinish()
    {
        Destroy(gameObject, GetComponent<AudioSource>().clip.length);
    }
}
