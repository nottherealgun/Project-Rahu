using UnityEngine;
using Unity.Cinemachine;
using PrimeTween;
using UnityEngine.InputSystem;
public class TestItem : MonoBehaviour
{
    public CinemachineCamera interactingCamera;
    public bool isBeingInteracted;
    public Vector3 initialPosition;
    public Vector3 initialRotation;
    public void interacted(bool val)
    {
        isBeingInteracted = val;
        if (val == false)
        {
            Tween.PositionY(transform, endValue: initialPosition.y, duration: 1, ease: Ease.OutCubic);
            return;
        }

        Tween.PositionY(transform, endValue: initialPosition.y+0.1f, duration: 1, ease: Ease.OutCubic);
    }

    void Start()
    {
        initialPosition = gameObject.transform.position;
        initialRotation = gameObject.transform.rotation.eulerAngles;
    }
}
