using UnityEngine;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using Sirenix.Serialization;

public class InteractableObject : SerializedMonoBehaviour
{
    [OdinSerialize] bool canBeRotated = true;
    [Tooltip("Adjusts the speed of rotation for interacting objects. Higher values mean faster rotation.")]
    [OdinSerialize] float objectRotationSpeed = 5f;
    [OdinSerialize] bool resetToInitialRotValue = true;
    [OdinSerialize, ShowIf("@resetToInitialRotValue == false")] Vector3 rotOnReset = Vector3.zero;
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
    bool holdingMouse;
    Quaternion deltaRotation;
    [Tooltip("Inverts vertical mouse movement for object rotation.")]
    bool _invertXObjectRotation = false;
    [Tooltip("Inverts horizontal mouse movement for object rotation.")]
    bool _invertYObjectRotation = true;
    Vector3 previousMousePos;

    public bool hasInspectionCamera
    {
        get { return customInspectionCamera != null; }
    }

    [OdinSerialize] GameObject interactionPrompt;
    [OdinSerialize] bool thisOpensPDA = false;
    [OdinSerialize] bool thisOpensHUD = true;
    [OdinSerialize, ShowIf("@thisOpensPDA == true")] PdaInitializer pdaInitializer;
    [OdinSerialize] bool customPromptActivationEnabled = false;
    public void OnInteracted(bool isBeingInteractedWith)
    {
        this.isBeingInteractedWith = isBeingInteractedWith;
        if (hasInspectionCamera)
            UIManager.OnTransitioned += () => SetInspectionCamera(isBeingInteractedWith);

        if (isBeingInteractedWith)
        {
            if (!isInstantInteraction)
            {
                UIManager.Instance.AddUILayer(gameObject.name);
                UIManager.LockCursor(false);
            }

            if (thisOpensHUD)
            {
                if (canBeRotated) UIManager.OnTransitioned += () => UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.ROTATABLE);
                else UIManager.OnTransitioned += () => UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.INTERACTABLE);
            }

            if (canBeRotated)
            {
                Tween.PositionY(transform, endValue: initialPosition.y + 0.15f, duration: 1, ease: Ease.OutCubic);
            }
            if (thisOpensPDA) pdaInitializer.OpenPDA();
            DeactivatePrompt();
            UIManager.Instance.DisableQuestHUD();
            onEnterInteraction?.Invoke();
        }
        else
        {
            if (!isInstantInteraction)
            {
                UIManager.Instance.CloseMenu();
                UIManager.LockCursor(true);
                UIManager.lastCursorState = true;
                if (!customPromptActivationEnabled)
                    UIManager.OnTransitioned += () => ActivatePrompt();
            }
            if (thisOpensPDA) pdaInitializer.ClosePDA();
            if (thisOpensHUD) UIManager.OnTransitioned += () => UIManager.Instance.DisableInteractionHUD();
            UIManager.OnTransitioned += UIManager.Instance.EnableQuestHUD;
            // If player stops interacting with the object
            onExitedInteraction?.Invoke();
        }
    }

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = itemMesh.transform.rotation;
    }

    void Update()
    {
        UpdateInputHandling();
        if (isBeingInteractedWith && canBeRotated)
        {
            UpdateInteractingObjectRotation();
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

    void UpdateInteractingObjectRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 deltaMousePos = Input.mousePosition - previousMousePos;
            float rotX = deltaMousePos.y * objectRotationSpeed * Time.deltaTime;
            float rotY = deltaMousePos.x * objectRotationSpeed * Time.deltaTime;

            Quaternion rotation = Quaternion.Euler(rotX, -rotY, 0);
            itemMesh.transform.rotation *= rotation;

            previousMousePos = Input.mousePosition;
        }
    }

    void SetInspectionCamera(bool val)
    {
        customInspectionCamera.SetActive(val);
    }

    public void ResetRotation()
    {
        if (canBeRotated == false) return;
        if (resetToInitialRotValue)
        { Tween.Rotation(itemMesh.transform, endValue: initialRotation, duration: 1, ease: Ease.OutCubic); }
        else
        {
            Tween.Rotation(itemMesh.transform, endValue: rotOnReset, duration: 1, ease: Ease.OutCubic);
        }
    }

    public void ResetInspectionTransform() // When the player leaves inspection (right click)
    {
        if (canBeRotated == false) return;
        Tween.PositionY(transform, endValue: initialPosition.y, duration: 1, ease: Ease.OutCubic);
        Tween.Rotation(itemMesh.transform, endValue: initialRotation, duration: 1, ease: Ease.OutCubic);
    }

    public GameObject GetInspectionCamera()
    {
        return customInspectionCamera;
    }

    public InteractionPrompt GetInteractionPrompt()
    {
        return interactionPrompt.GetComponent<InteractionPrompt>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShowPrompt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HidePrompt();
        }
    }

    public void ActivatePrompt()
    {
        if (interactionPrompt == null) return;
        interactionPrompt.SetActive(true);
    }

    public void DeactivatePrompt()
    {
        if (interactionPrompt == null) return;
        interactionPrompt.SetActive(false);
    }

    void ShowPrompt()
    {
        if (interactionPrompt == null) return;
        interactionPrompt.GetComponent<InteractionPrompt>().ShowPrompt();
    }

    void HidePrompt()
    {
        if (interactionPrompt == null) return;
        interactionPrompt.GetComponent<InteractionPrompt>().HidePrompt();
    }

    public void PlaySFX(string sfxName)
    {
        EnvironmentalAudioManager.Instance.PlaySFX(sfxName);
    }
}