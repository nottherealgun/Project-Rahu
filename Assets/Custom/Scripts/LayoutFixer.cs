using UnityEngine;
using UnityEngine.UI;

public class LayoutFixer : MonoBehaviour
{
    void Update()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform as RectTransform);
    }

    void OnValidate()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform as RectTransform);
    }
}
