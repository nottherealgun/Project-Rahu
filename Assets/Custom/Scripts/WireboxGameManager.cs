using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class WireboxGameManager : Menu
{
    [SerializeField] Transform wireContainerObject;
    [SerializeField] List<Wire> requiredBlueWires;
    [SerializeField] List<Wire> requiredGreenWires;
    [SerializeField] List<Wire> requiredRedWires;
    [SerializeField] List<Wire> requiredYellowWires;
    [SerializeField] List<Wire> doneBlueWires = new List<Wire>();
    [SerializeField] List<Wire> doneGreenWires = new List<Wire>();
    [SerializeField] List<Wire> doneRedWires = new List<Wire>();
    [SerializeField] List<Wire> doneYellowWires = new List<Wire>();
    WirePort wirePort;
    [SerializeField] GameObject raycastBlocker;
    public UnityEvent onPuzzleCompleted;
    void Start()
    {
        EnvironmentalAudioManager.Instance.PlayAmbience("wirebox_ambiance");

        foreach (Wire wire in requiredBlueWires)
        {
            wire.onWireTurned.AddListener((w, d) => OnRequiredWireRotated(w, "blue"));
        }
        foreach (Wire wire in requiredGreenWires)
        {
            wire.onWireTurned.AddListener((w, d) => OnRequiredWireRotated(w, "green"));
        }
        foreach (Wire wire in requiredRedWires)
        {
            wire.onWireTurned.AddListener((w, d) => OnRequiredWireRotated(w, "red"));
        }
        foreach (Wire wire in requiredYellowWires)
        {
            wire.onWireTurned.AddListener((w, d) => OnRequiredWireRotated(w, "yellow"));
        }

        wirePort = GetComponent<WirePort>();
    }

    public void OnWireClicked(int wireIndex, Wire wire)
    {
        CheckWires();
        EnvironmentalAudioManager.Instance.PlaySFX("wire_selection");
    }

    void CheckWires()
    {
        bool allWiresCorrect = true;
        foreach (Wire wire in wireContainerObject.GetComponentsInChildren<Wire>())
        {
            if (!wire.IsInValidDirection())
            {
                allWiresCorrect = false;
                break;
            }
        }

        if (allWiresCorrect)
        {
            Debug.Log("All wires are correct!");
            raycastBlocker.SetActive(true);
            EnvironmentalAudioManager.Instance.PlaySFX("puzzle_complete");
            EnvironmentalAudioManager.Instance.PlaySFX("wirebox_powerup");

            UniTask.Delay(3000).ContinueWith(() =>
            {
                EndGame();
            });
        }
    }

    [Button(ButtonSizes.Large)]
    void EndGame()
    {
        UIManager.LockCursor(true);
        onPuzzleCompleted?.Invoke();
        EnvironmentalAudioManager.Instance.StopAmbience();
        UIManager.Instance.DisableInteractionHUD();
    }

    void OnRequiredWireRotated(Wire wire, string color)
    {
        if (wire.IsInValidDirection())
        {
            AddWireToDoneList(wire, color);
        }
        else
        {
            RemoveWireFromDoneList(wire, color);
        }

        CheckPorts();
    }

    void AddWireToDoneList(Wire wire, string color)
    {
        switch (color)
        {
            case "blue":
                if (doneBlueWires.Contains(wire)) return;
                doneBlueWires.Add(wire);
                break;
            case "green":
                if (doneGreenWires.Contains(wire)) return;
                doneGreenWires.Add(wire);
                break;
            case "red":
                if (doneRedWires.Contains(wire)) return;
                doneRedWires.Add(wire);
                break;
            case "yellow":
                if (doneYellowWires.Contains(wire)) return;
                doneYellowWires.Add(wire);
                break;
        }
        EnvironmentalAudioManager.Instance.PlaySFX("wirebox_finish");
    }

    void RemoveWireFromDoneList(Wire wire, string color)
    {
        switch (color)
        {
            case "blue":
                if (doneBlueWires.Contains(wire) == false) return;
                doneBlueWires.Remove(wire);
                break;
            case "green":
                if (doneGreenWires.Contains(wire) == false) return;
                doneGreenWires.Remove(wire);
                break;
            case "red":
                if (doneRedWires.Contains(wire) == false) return;
                doneRedWires.Remove(wire);
                break;
            case "yellow":
                if (doneYellowWires.Contains(wire) == false) return;
                doneYellowWires.Remove(wire);
                break;
        }
    }

    void CheckPorts()
    {
        if (doneBlueWires.Count == requiredBlueWires.Count)
        {
            wirePort.EnableGlow("blue");
        }
        else
        {
            wirePort.DisableGlow("blue");
        }

        if (doneGreenWires.Count == requiredGreenWires.Count)
        {
            wirePort.EnableGlow("green");
        }
        else
        {
            wirePort.DisableGlow("green");
        }

        if (doneRedWires.Count == requiredRedWires.Count)
        {
            wirePort.EnableGlow("red");
        }
        else
        {
            wirePort.DisableGlow("red");
        }

        if (doneYellowWires.Count == requiredYellowWires.Count)
        {
            wirePort.EnableGlow("yellow");
        }
        else
        {
            wirePort.DisableGlow("yellow");
        }
    }
}
