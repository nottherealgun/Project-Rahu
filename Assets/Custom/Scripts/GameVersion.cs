using TMPro;
using UnityEngine;

public class GameVersion : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    void Start()
    {
        label = GetComponent<TMP_Text>();
        label.text = Application.version;
    }
}
