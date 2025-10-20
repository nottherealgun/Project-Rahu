using UnityEngine;
using Sirenix.OdinInspector;
public class Platinum : SerializedMonoBehaviour
{
    public void EndGame()
    {
        UIManager.Instance.DisableInteractionHUD();
        PersistentDataManager.Instance.puzzles[PuzzleType.Platinum] = true;
        GameManager.Instance.OnPuzzleComplete();
    }
}
