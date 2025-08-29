using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public abstract class Menu : SerializedMonoBehaviour
{
    [OdinSerialize] GameObject firstSelected;

    [HideInInspector] public Dictionary<string, InputAction> UI;

    void Awake()
    {
        if (firstSelected == null)
        {
            firstSelected = GameObject.FindFirstObjectByType<GameObject>();
        }
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);

        UI = new Dictionary<string, InputAction>
        {
            { "navigate",   InputSystem.actions["UI/Navigate"] },
            { "submit",     InputSystem.actions["UI/Submit"] },
            { "cancel",     InputSystem.actions["UI/Cancel"] },
            { "point",      InputSystem.actions["UI/Point"] },
            { "click",      InputSystem.actions["UI/Click"] },
            { "right_click",InputSystem.actions["UI/RightClick"] },
            { "middle_click",      InputSystem.actions["UI/MiddleClick"] },
            { "scroll",      InputSystem.actions["UI/ScrollWheel"] },
            { "menu",       InputSystem.actions["UI/Menu"] }
        };
    }
}
