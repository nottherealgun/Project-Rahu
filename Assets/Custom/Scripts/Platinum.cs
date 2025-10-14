using UnityEngine;
using Sirenix.OdinInspector;
public class Platinum : SerializedMonoBehaviour
{
    public void EndGame()
    {
        UIManager.Instance.CloseInteractionHUD();
        PersistentDataManager.Instance.puzzles[PuzzleType.Platinum] = true;
        GameManager.Instance.OnPuzzleComplete();
    }
}
