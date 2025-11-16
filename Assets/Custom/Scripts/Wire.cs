using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PrimeTween;
using System.Collections.Generic;

public class Wire : SerializedMonoBehaviour
{
    public enum Direction { DEFAULT, ROTATED_90, ROTATED_180, ROTATED_270 }
    Direction[] directions = new Direction[] {
        Direction.DEFAULT,
        Direction.ROTATED_90,
        Direction.ROTATED_180,
        Direction.ROTATED_270
    };
    [SerializeField, ReadOnly] int idxInParent;
    [SerializeField] WireboxGameManager gameManager;
    [SerializeField] Direction currentDirection = Direction.DEFAULT;
    [SerializeField, HideIf("@directionDoesntMatter == true")]
    Dictionary<Direction, bool> validDirections = new Dictionary<Direction, bool>()
    {
        { Direction.DEFAULT, false },
        { Direction.ROTATED_90, false },
        { Direction.ROTATED_180, false },
        { Direction.ROTATED_270, false }
    };
    [SerializeField] bool directionDoesntMatter = false;
    [SerializeField] Vector3 targetRotation = Vector3.zero;

    public UnityEvent<Wire,Direction> onWireTurned;

    [Button("Force Turn", ButtonSizes.Large)]
    void ForceTurn()
    {
        int dirIdx = System.Array.IndexOf(directions, currentDirection);
        dirIdx = (dirIdx + 1) % 4;
        currentDirection = directions[dirIdx % 4];
        targetRotation = new Vector3(0, 0, -90 * dirIdx);
        transform.rotation = Quaternion.Euler(targetRotation);
    }

    void OnValidate()
    {
        idxInParent = transform.GetSiblingIndex();
    }

    public void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnWireClicked);
        onWireTurned?.Invoke(this, currentDirection);
    }

    void OnWireClicked()
    {
        currentDirection = directions[((int)currentDirection + 1) % 4];
        int dirIdx = System.Array.IndexOf(directions, currentDirection);
        targetRotation = new Vector3(0, 0, -90 * dirIdx);
        Tween.Rotation(transform, targetRotation, 0.25f).OnComplete(() =>
        {
            onWireTurned?.Invoke(this, currentDirection);
        });
        gameManager.OnWireClicked(idxInParent, this);
    }
    
    public bool IsInValidDirection()
    {
        if (directionDoesntMatter)
            return true;
        return validDirections[currentDirection];
    }
}
