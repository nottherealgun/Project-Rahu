using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class WireboxGameManager : SerializedMonoBehaviour
{
    [SerializeField] Transform wireContainerObject;
    UnityEvent onWireboxPuzzleCompleted;

    void Start()
    {
        onWireboxPuzzleCompleted?.AddListener(EndGame);
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
                // print(wire.name + " is in invalid direction");
                allWiresCorrect = false;
                break;
            }
        }

        if (allWiresCorrect)
        {
            Debug.Log("All wires are correct!");
            onWireboxPuzzleCompleted?.Invoke();
        }
    }

    void EndGame()
    {
        GetComponent<PuzzleCompletionEmitter>().onPuzzleCompleted?.Invoke();
        UIManager.LockCursor(true);
    }
}
