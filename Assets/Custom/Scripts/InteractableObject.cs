using UnityEngine;
using PrimeTween;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Events;
using Sirenix.Serialization;
using Unity.VisualScripting;
using Unity.Cinemachine;

public class InteractableObject : SerializedMonoBehaviour
{
    [SceneObjectsOnly] public GameObject itemMesh;
    public bool isBeingInteracted;
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public UnityEvent onEnterInteraction;
    public UnityEvent onExitedInteraction;
    GameObject _playerCharacter;
    public GameObject playerCharacter
    {
        get { return _playerCharacter; }
        set
        {
            _playerCharacter = value;
            playerScript = _playerCharacter.GetComponent<PlayerController>();
        }
    }
    PlayerController playerScript;
    MouseRotator mouseRotator
    {
        get { return playerScript.GetMouseRotator(); }
        set { mouseRotator = value; }
    }
    bool holdingMouse;
    Quaternion deltaRotation;
    [OdinSerialize] bool canBeRotated = true;
    [OdinSerialize] GameObject customInspectionCamera;
    public bool hasInspectionCamera
    {
        get { return customInspectionCamera != null; }
        set { hasInspectionCamera = value; }
    }
    public void OnInteracted(bool val)
    {
        isBeingInteracted = val;
        if(hasInspectionCamera)
            UIManager.OnEnteredNewScene += () => customInspectionCamera.SetActive(val);

        if (val == false)
        {
            onExitedInteraction?.Invoke();
            Tween.PositionY(itemMesh.transform, endValue: initialPosition.y, duration: 1, ease: Ease.OutCubic);
            Tween.Rotation(itemMesh.transform, endValue: initialRotation, duration: 1, ease: Ease.OutCubic);
            return;
        }

        onEnterInteraction?.Invoke();
        if (canBeRotated)
            Tween.PositionY(itemMesh.transform, endValue: initialPosition.y + 0.1f, duration: 1, ease: Ease.OutCubic);
    }

    void Start()
    {
        initialPosition = itemMesh.transform.position;
        initialRotation = itemMesh.transform.rotation;
    }

    private void Update() {
        UpdateInteractingObjectRotation();
    }

    void UpdateInteractingObjectRotation()
    {
        if (isBeingInteracted && canBeRotated)
        {
            deltaRotation = mouseRotator.UpdateRotation(Input.mousePosition, holdingMouse);
            itemMesh.transform.rotation = initialRotation * deltaRotation;
        }

    }
}
