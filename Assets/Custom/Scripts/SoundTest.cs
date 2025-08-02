using UnityEngine;

public class PlayAudioOnJ : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component on the same GameObject
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Check if J key is pressed
        if (Input.GetKeyDown(KeyCode.J))
        {
            // Play the audio
            if (audioSource != null)
            {
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("No AudioSource found on this GameObject.");
            }
        }
    }
}
