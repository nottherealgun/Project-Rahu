using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public abstract class Menu : SerializedMonoBehaviour
{
    [OdinSerialize] GameObject firstSelected;

    void Awake()
    {
        if (firstSelected == null)
        {
            firstSelected = GameObject.FindFirstObjectByType<GameObject>();
        }

        // SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        ReselectFirst();
    }

    public void ReselectFirst()
    {
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
    }
}
