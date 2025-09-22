using UnityEngine;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using Sirenix.Serialization;

public class InteractableObject : SerializedMonoBehaviour
{
    [OdinSerialize] bool canBeRotated = true;
    public bool isInstantInteraction = false;
    [OdinSerialize, SceneObjectsOnly] GameObject customInspectionCamera;
    [SceneObjectsOnly] public GameObject itemMesh;
    [ReadOnly] public bool isBeingInteractedWith;
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public UnityEvent onEnterInteraction;
    public UnityEvent onExitedInteraction;
    GameObject _playerCharacter
    {
        get { return PersistentDataManager.Player; }
    }
    PlayerController playerScript
    {
        get { return _playerCharacter.GetComponent<PlayerController>(); }
    }
    MouseRotator mouseRotator;
    bool holdingMouse;
    Quaternion deltaRotation;
    [Tooltip("Adjusts the speed of rotation for interacting objects. Higher values mean faster rotation.")]
    float _objectRotationSpeed = 0.5f;
    [Tooltip("Inverts vertical mouse movement for object rotation.")]
    bool _invertXObjectRotation = false;
    [Tooltip("Inverts horizontal mouse movement for object rotation.")]
    bool _invertYObjectRotation = true;

    public bool hasInspectionCamera
    {
        get { return customInspectionCamera != null; }
    }
    public void OnInteracted(bool val)
    {
        isBeingInteractedWith = val;

        if (hasInspectionCamera)
            UIManager.OnTransitioned += () => SetInspectionCamera(val);

        if (isBeingInteractedWith)
        {
            onEnterInteraction?.Invoke();
            if (canBeRotated)
            {
                // Tween.PositionY(itemMesh.transform, endValue: initialPosition.y + 0.15f, duration: 1, ease: Ease.OutCubic);
                Tween.PositionY(transform, endValue: initialPosition.y + 0.15f, duration: 1, ease: Ease.OutCubic);
            }
        }
        else
        {
            // If player stops interacting with the object
            onExitedInteraction?.Invoke();
        }
    }

    void Start()
    {
        // initialPosition = itemMesh.transform.position;
        initialPosition = transform.position;
        initialRotation = itemMesh.transform.rotation;

        mouseRotator = new MouseRotator(Input.mousePosition);
        mouseRotator.RotationSpeed = _objectRotationSpeed;
        mouseRotator.InvertXRotation = _invertXObjectRotation;
        mouseRotator.InvertYRotation = _invertYObjectRotation;
    }

    void Update()
    {
        UpdateInputHandling();
        UpdateInteractingObjectRotation();
    }

    void UpdateInteractingObjectRotation()
    {
        if (this.isBeingInteractedWith && this.canBeRotated)
        {
            deltaRotation = mouseRotator.UpdateRotation(Input.mousePosition, holdingMouse);
            itemMesh.transform.rotation = initialRotation * deltaRotation;
        }
    }

    void UpdateInputHandling()
    {
        // Start dragging when mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            holdingMouse = true;
        }
        // End dragging when mouse button is released
        else if (Input.GetMouseButtonUp(0))
        {
            holdingMouse = false;
        }
    }

    void SetInspectionCamera(bool val)
    {
        customInspectionCamera.SetActive(val);
    }

    public void ResetRotation()
    {
        if (canBeRotated)
        {
            // Tween.PositionY(itemMesh.transform, endValue: initialPosition.y, duration: 1, ease: Ease.OutCubic);
            Tween.PositionY(transform, endValue: initialPosition.y, duration: 1, ease: Ease.OutCubic);
            Tween.Rotation(itemMesh.transform, endValue: initialRotation, duration: 1, ease: Ease.OutCubic).OnComplete(() =>
            {
                mouseRotator.ResetRotation();
            });
        }
    }

    public GameObject GetInspectionCamera()
    {
        return customInspectionCamera;
    }
}

public class MouseRotator
{

    public float RotationSpeed { get; set; } = 0.5f;
    public bool InvertXRotation { get; set; } = false;
    public bool InvertYRotation { get; set; } = false;
    private Vector2 _lastMousePosition;

    private Quaternion _currentRotation = Quaternion.identity; // Using Quaternion.identity for Unity

    public MouseRotator(Vector2 initialMousePosition)
    {
        _lastMousePosition = initialMousePosition;
    }

    public Quaternion UpdateRotation(Vector2 currentMousePosition, bool isDragging)
    {
        if (isDragging)
        {
            // Calculate the change in mouse position since the last frame
            Vector2 mouseDelta = currentMousePosition - _lastMousePosition;

            float rotationAmountX = mouseDelta.y * RotationSpeed * (InvertXRotation ? -1f : 1f);
            float rotationAmountY = mouseDelta.x * RotationSpeed * (InvertYRotation ? -1f : 1f);

            Quaternion pitchRotation = Quaternion.AngleAxis(rotationAmountX, Vector3.right); // Rotate around object's local right
            Quaternion yawRotation = Quaternion.AngleAxis(rotationAmountY, Vector3.up);    // Rotate around world up (for turntable)

            _currentRotation = yawRotation * pitchRotation * _currentRotation;

            _currentRotation = Quaternion.Normalize(_currentRotation);
        }
        _lastMousePosition = currentMousePosition;

        return _currentRotation;
    }
    public void ResetRotation()
    {
        _currentRotation = Quaternion.identity;
    }
    public void SetRotation(Quaternion newRotation)
    {
        _currentRotation = newRotation;
    }
    public Quaternion GetCurrentRotation()
    {
        return _currentRotation;
    }
}