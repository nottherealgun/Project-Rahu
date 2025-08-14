using UnityEngine;
using PrimeTween;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Events;

public class InteractableObject : SerializedMonoBehaviour
{
    [SceneObjectsOnly] public GameObject itemMesh;
    public bool isBeingInteracted;
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public UnityEvent onInteracting;
    public UnityEvent onInteracted;
    public void OnInteracted(bool val)
    {
        isBeingInteracted = val;
        
        if (val == false)
        {
            onInteracted?.Invoke();
            Tween.PositionY(itemMesh.transform, endValue: initialPosition.y, duration: 1, ease: Ease.OutCubic);
            Tween.Rotation(itemMesh.transform, endValue: initialRotation, duration: 1, ease: Ease.OutCubic);
            return;
        }

        onInteracting?.Invoke();
        Tween.PositionY(itemMesh.transform, endValue: initialPosition.y + 0.1f, duration: 1, ease: Ease.OutCubic);
    }

    void Start()
    {
        initialPosition = itemMesh.transform.position;
        initialRotation = itemMesh.transform.rotation;
    }
}
