using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.UI;
using UnityEngine.Events;

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

public class PlayerController : SerializedMonoBehaviour
{
    [OdinSerialize, ReadOnly] GameObject interactingObject;
    GameObject interactingObjectMesh;
    [OdinSerialize, TabGroup("Character")] GameObject _mainCamera;
    [TabGroup("Character")] public GameObject playerInteractionCamera;
    [TabGroup("Character")] public GameObject followCamera;
    [TabGroup("Character")]
    [OdinSerialize] InputManager _inputManager;
    [TabGroup("Character")]
    [OdinSerialize] bool holdingMouse = false;
    #region "PHYSICS"
    [TabGroup("Physics")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;
    [TabGroup("Physics")]
    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;
    [TabGroup("Physics")]
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [TabGroup("Physics")]
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [TabGroup("Physics")]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [TabGroup("Physics")]
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;
    [TabGroup("Physics")]
    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;
    [TabGroup("Physics")]
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;
    #endregion
    #region "PLAYER GROUNDED"
    [TabGroup("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [TabGroup("Player Grounded")]
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [TabGroup("Player Grounded")]
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [TabGroup("Player Grounded")]
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;
    #endregion
    #region "CINEMACHINE"
    [TabGroup("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [TabGroup("Cinemachine")]
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [TabGroup("Cinemachine")]
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [TabGroup("Cinemachine")]
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [TabGroup("Cinemachine")]
    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    [TabGroup("Cinemachine"), OdinSerialize, ReadOnly] LayerMask defaultMask;
    [TabGroup("Cinemachine"), OdinSerialize] LayerMask interactionMask;
    #endregion
    // player
    private bool isInteracting = false;
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;
    private CharacterController _controller;
    private InputManager _input;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
            return false;
#endif
        }
    }

    private Vector3 currentRotationOffset;
    private float rotationSpeed = 5f;
    private MouseRotator _mouseRotator;
    private Quaternion _initialObjectRotationOnDragStart;

    [Tooltip("Adjusts the speed of rotation for interacting objects. Higher values mean faster rotation.")]
    private float _objectRotationSpeed = 0.5f;
    [Tooltip("Inverts vertical mouse movement for object rotation.")]
    private bool _invertXObjectRotation = false;
    [Tooltip("Inverts horizontal mouse movement for object rotation.")]
    private bool _invertYObjectRotation = false;

    [FoldoutGroup("Audio")]
    [OdinSerialize] AudioSource voiceSource;
    [FoldoutGroup("Audio")]
    [OdinSerialize] bool speaking = false;
    [FoldoutGroup("Audio")]
    [OdinSerialize] AudioDataStore.DialogueLine? currentDialogueLine;

    [TabGroup("Events")]
    public UnityEvent onMouseHold;
    public UnityEvent onMouseRelease;
    void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        if (followCamera == null)
            followCamera = GameObject.Find("FollowCamera");

    }
    void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputManager>();
        _playerInput = GetComponent<PlayerInput>();
        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        _mouseRotator = new MouseRotator(Input.mousePosition);
        _mouseRotator.RotationSpeed = _objectRotationSpeed;
        _mouseRotator.InvertXRotation = _invertXObjectRotation;
        _mouseRotator.InvertYRotation = _invertYObjectRotation;
        defaultMask = _mainCamera.GetComponent<Camera>().cullingMask;
    }

    void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        JumpAndGravity();
        GroundedCheck();
        Move();
        if (isInteracting)
            UpdateInputHandling();
    }

    void UpdateInputHandling()
    {
        // Start dragging when mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            holdingMouse = true;
            onMouseHold?.Invoke();
        }
        // End dragging when mouse button is released
        else if (Input.GetMouseButtonUp(0))
        {
            holdingMouse = false;
            onMouseRelease?.Invoke();
        }
    }

    public MouseRotator GetMouseRotator()
    {
        return _mouseRotator;
    }

    public void OnPrimaryInteract(InputValue _value)
    {
        // If there's no interactable object in vicinity, do nothing
        if (interactingObject == null) return;

        if (isInteracting == false)
        {
            // If not interacting, start interaction
            EnterInteractionWithObject();
        }
        else
        {
            // If already interacting, exit interaction
            ExitInteractionWithObject();
        }
    }

    public void OnMenu(InputValue value)
    {
        // On Windows PC, Menu = ESC
        if (UIManager.isGamePaused)
        {
            // If game was paused, resume it
            UIManager.Instance.ResumeGame();

            // Not setting cursor state here, because resume game function will set it to the most recent state
        }
        else
        {
            // If game was not paused, pause it
            UIManager.Instance.PauseGame();
            // Free cursor as well
            UIManager.SetCursorState(false);
        }
    }

    void EnterInteractionWithObject()
    {
        isInteracting = true;
        InteractableObject interactingObjScript = interactingObject.GetComponent<InteractableObject>();

        // Emit event to notify object that interaction has started
        interactingObjScript.OnInteracted(true);

        // When transitioned to interaction...
        UIManager.OnTransitioned += () =>
        {
            // Ignore player layer on camera
            // (so that player model won't be seen phasing into the camera)
            _mainCamera.GetComponent<Camera>().cullingMask = ~interactionMask;

            // Enable interaction camera if the object does not have its own
            if (interactingObjScript.hasInspectionCamera == false)
                EnableInteractionCamera();
        };

        // Also let cursor be free
        UIManager.SetCursorState(false);

        // Record last cursor state
        // When unpaused, cursor state will be set to the last recorded state
        UIManager.lastCursorState = false;

        // Camera transition in (fade)
        UIManager.Instance.ToggleTransitionPanel(true);
    }

    void ExitInteractionWithObject()
    {
        isInteracting = false;
        InteractableObject interactingObjScript = interactingObject.GetComponent<InteractableObject>();

        // Emit event to notify object that interaction has finished
        interactingObjScript.OnInteracted(false);

        // When transitioned out of interaction...
        UIManager.OnTransitioned += () =>
        {
            // Show all layers on camera
            _mainCamera.GetComponent<Camera>().cullingMask = defaultMask;

            // Disable interaction camera
            DisableInteractionCamera();
        };

        // Reset mouse rotation when interaction ends
        // (mouse rotator is used to rotate the object when interacting)
        _mouseRotator.ResetRotation();

        // Also lock cursor
        UIManager.SetCursorState(true);
        UIManager.lastCursorState = true;

        // Camera transition out (fade)
        UIManager.Instance.ToggleTransitionPanel(false);
    }

    public void DisconnectFromInteractingObject()
    {
        // Clear interaction object references
        interactingObject = null;
        interactingObjectMesh = null;
    }

    public void ForceExitInteraction()
    {
        // If not interacting already, do nothing
        if (isInteracting == false) return;

        ExitInteractionWithObject();
    }

    void EnableInteractionCamera()
    {
        // If interacting, enable it
        playerInteractionCamera.gameObject.SetActive(true);

        // Make camera look at the object
        playerInteractionCamera.GetComponent<CinemachineCamera>().Target.TrackingTarget = interactingObject.transform;
    }

    void DisableInteractionCamera()
    {
        // If interaction camera is already disabled, do nothing
        if (playerInteractionCamera.gameObject.activeInHierarchy == false) return;

        // Disable interaction camera
        playerInteractionCamera.gameObject.SetActive(false);
    }

    bool IsValidInteractableObject(GameObject _obj)
    {
        _obj.TryGetComponent<InteractableObject>(out InteractableObject _script);
        return _script != null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsValidInteractableObject(other.gameObject))
        {
            interactingObject = other.gameObject;
            interactingObjectMesh = interactingObject.GetComponent<InteractableObject>().itemMesh;   
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (interactingObject == other.gameObject)
        {
            DisconnectFromInteractingObject();
        }
    }

    void LateUpdate()
    {
        CameraRotation();
    }

    void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        if (isInteracting)
        {
            targetSpeed = 0.0f;
            _input.move = Vector2.zero;
        }

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    void OnFootstep(AnimationEvent animationEvent)
    {
        if (isInteracting) return;
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            EnvironmentalAudioManager.Instance.PlaySFX("footstep", transform, true);
        }
    }

    void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            EnvironmentalAudioManager.Instance.PlaySFX("land", transform, false);
        }
    }

    [Tooltip("The force applied when pushing a Rigidbody object.")]
    float PushForce = 1.5f; // Adjust this value in the Inspector
    // This function is called when the CharacterController hits a collider
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Get the Rigidbody of the object we hit
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic || isInteracting)
        {
            return;
        }
        Vector3 pushDirection = (hit.gameObject.transform.position - transform.position).normalized;
        // Ensure the push is primarily horizontal, unless you want to push objects upwards too
        pushDirection.y = 0; // Zero out the Y component to only push horizontally
        pushDirection.Normalize(); // Ensure it's a unit vector
        body.AddForce(pushDirection * PushForce, ForceMode.Impulse);
    }

    [FoldoutGroup("Audio")]
    [OdinSerialize] AudioDataStore.DialogueLine testDL = new AudioDataStore.DialogueLine();
    [Button(ButtonSizes.Large)]
    [FoldoutGroup("Audio")]
    public void VoiceTest()
    {
        Speak(testDL);
    }

    public AudioDataStore.DialogueLine Speak(AudioDataStore.DialogueLine dialogueLine)
    {
        if (dialogueLine.audioFile == null)
        {
            Debug.LogError($"Dialogue line struct does NOT contain valid audio file.\n Ran on {name} object.");
            return dialogueLine;
        }
        voiceSource.clip = dialogueLine.audioFile;
        currentDialogueLine = dialogueLine;
        StartCoroutine(PlayAndCheckDialogueCompletion());

        return dialogueLine;
    }

    IEnumerator PlayAndCheckDialogueCompletion()
    {
        voiceSource.Play();
        speaking = true;

        while (voiceSource.isPlaying)
            yield return null;

        speaking = false;
        currentDialogueLine = null;
    }
    void OnDestroy()
    {
        UIManager.SetCursorState(false);
    }
}