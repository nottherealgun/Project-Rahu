using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class OnFillerEnter : SerializedMonoBehaviour
{
    [OdinSerialize] UnityEvent<GameObject> onTriggerEntered;
    [OdinSerialize] UnityEvent<GameObject> onTriggerExited;
    private void OnTriggerEnter2D(Collider2D other)
    {
        onTriggerEntered?.Invoke(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other) {
        onTriggerExited?.Invoke(other.gameObject);
    }
    
}
