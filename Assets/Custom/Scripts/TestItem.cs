using UnityEngine;
using Unity.Cinemachine;
using PrimeTween;
using UnityEngine.InputSystem;
public class TestItem : MonoBehaviour
{
    public CinemachineCamera interactingCamera;
    public GameObject itemMesh;
    public bool isBeingInteracted;
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public void interacted(bool val)
    {
        isBeingInteracted = val;
        if (val == false)
        {
            Tween.PositionY(itemMesh.transform, endValue: initialPosition.y, duration: 1, ease: Ease.OutCubic);
            Tween.Rotation(itemMesh.transform, endValue: initialRotation, duration: 1, ease: Ease.OutCubic);
            return;
        }

        Tween.PositionY(itemMesh.transform, endValue: initialPosition.y + 0.1f, duration: 1, ease: Ease.OutCubic);
    }

    void Start()
    {
        initialPosition = itemMesh.transform.position;
        initialRotation = itemMesh.transform.rotation;
    }
}
