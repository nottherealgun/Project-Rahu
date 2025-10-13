using Sirenix.OdinInspector;
using UnityEngine;

public class PdaInitializer : SerializedMonoBehaviour
{
    public string header;
    [TextArea(10, 20)] public string body;
    [PreviewField(50), AssetsOnly] public Sprite image;

    public void OpenPDA()
    {
        UIManager.Instance.SetupPDA(header, body, image);
        UIManager.OnTransitioned +=  UIManager.Instance.OpenPDA;
    }

    public void ClosePDA()
    {
        UIManager.OnTransitioned +=  UIManager.Instance.ClosePDA;
    }
}
