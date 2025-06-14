using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject interactingObject;
    public GameObject followCam;
    public bool interact = false;
    void Start()
    {

    }

    void Update()
    {

    }

    public void OnInteract(InputValue value)
    {
        interact = !interact;
        if(interact) Debug.Log("Started Interacting.");
        else Debug.Log("Stopped Interacting.");
    }

    private void OnTriggerEnter(Collider other)
    {
        interactingObject = other.gameObject;
        TestItem _item;
        interactingObject.TryGetComponent<TestItem>(out _item);
        if (_item.camera != null)
            followCam.SetActive(false);
        _item.camera.gameObject.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        TestItem _item;
        interactingObject.TryGetComponent<TestItem>(out _item);
        if (interactingObject == other.gameObject)
            if (_item.camera != null)
                _item.camera.gameObject.SetActive(false);
        interactingObject = null;
        followCam.SetActive(true);
        interact = false;
    }
}
