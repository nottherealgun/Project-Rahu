using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.XInput;
using Cysharp.Threading.Tasks;

public class CurrentActiveDeviceManager : SerializedMonoBehaviour
{
    // public static CurrentActiveDeviceManager Instance { get; private set; }

    public enum ActiveDevice
    {
        Keyboard,
        Xbox,
        DualShock
    }

    [Title("References")]
    [OdinSerialize] private PlayerInput playerInput;
    InputUser user;

    [Title("State")]
    [ReadOnly] public ActiveDevice activeDevice = ActiveDevice.Keyboard;
    [ReadOnly] public string controlScheme = "Keyboard&Mouse";
    [ReadOnly] public bool isGamepad = false;

    [Title("Events")]
    public UnityEvent onControllerConnected;
    public UnityEvent onControllerDisconnected;
    private void Awake()
    {
        // if (Instance != null && Instance != this)
        // {
        //     Destroy(this.gameObject);
        //     return;
        // }

        // Instance = this;

        // DontDestroyOnLoad(this.gameObject);

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        InputSystem.onDeviceChange += OnDeviceChanged;
        playerInput.neverAutoSwitchControlSchemes = true;

    }

    void Start()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            if (device is Gamepad)
            {
                onControllerConnected?.Invoke();
            }
        }

        user = playerInput.user;
    }

    void Update()
    {
        UpdateDeviceState(playerInput.currentControlScheme);
    }

    private void UpdateDeviceState(string scheme)
    {
        ActiveDevice newDevice;
        controlScheme = scheme;

        switch (scheme)
        {
            case "Xbox":
                newDevice = ActiveDevice.Xbox;
                break;
            case "DualShock":
                newDevice = ActiveDevice.DualShock;
                break;
            default:
                newDevice = ActiveDevice.Keyboard;
                break;
        }

        bool wasConnected = isGamepad;
        isGamepad = playerInput.currentControlScheme.Equals("Gamepad") ? true : false;

        // Fire events when connection state changes
        if (!wasConnected && isGamepad)
            onControllerConnected?.Invoke();

        if (wasConnected && !isGamepad)
        {
            bool noControllersLeft = true;

            foreach (InputDevice device in InputSystem.devices)
            {
                if (device is Gamepad)
                {
                    noControllersLeft = false;
                    break;
                }
            }

            if (noControllersLeft)
            {
                onControllerDisconnected?.Invoke();
            }
        }

        activeDevice = newDevice;
    }

    void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log($"Device '{device}' was {change}");
            if (device is Gamepad)
            {
                onControllerConnected?.Invoke();
            }
        }
        else if (change == InputDeviceChange.Removed)
        {
            if (device is Gamepad)
            {
                onControllerDisconnected?.Invoke();
            }
        }
    }

    public void SetControlScheme(int _controlScheme)
    {
        user = playerInput.user;
        user.UnpairDevices();

        if (_controlScheme == 0)
        {
            InputUser.PerformPairingWithDevice(Keyboard.current, user);
            InputUser.PerformPairingWithDevice(Mouse.current, user);

            playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);

            UIManager.Instance.SetControlScheme("Keyboard&Mouse", ActiveDevice.Keyboard);
        }
        else
        {
            foreach (InputDevice device in InputSystem.devices)
            {
                if (device is XInputController)
                {
                    UIManager.Instance.SetControlScheme("Xbox",ActiveDevice.Xbox);
                    playerInput.SwitchCurrentControlScheme(device);
                    break;
                }
                else if (device is DualShockGamepad)
                {
                    UIManager.Instance.SetControlScheme("DualShock",ActiveDevice.DualShock);
                    playerInput.SwitchCurrentControlScheme(device);
                    break;
                }
            }
        }
    }
}
