using UnityEngine;
using Sirenix.OdinInspector;
public class Platinum : SerializedMonoBehaviour
{
    public void EndGame()
    {
        PersistentDataManager.Instance.puzzles[PuzzleType.Platinum] = true;
        GameManager.Instance.OnPuzzleComplete();
    }
}
