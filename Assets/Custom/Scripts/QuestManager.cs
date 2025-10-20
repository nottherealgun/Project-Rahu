using PixelCrushers.DialogueSystem;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class QuestManager : SerializedMonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }
}
