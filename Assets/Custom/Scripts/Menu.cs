using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public abstract class Menu : SerializedMonoBehaviour
{
    [OdinSerialize] GameObject firstSelected;

    [OdinSerialize] public Dictionary<string, InputAction> UI;

    void Awake()
    {
        if (firstSelected == null)
        {
            firstSelected = GameObject.FindFirstObjectByType<GameObject>();
        }
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
    }
}
