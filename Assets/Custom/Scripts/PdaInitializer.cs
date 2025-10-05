using Sirenix.OdinInspector;
using UnityEngine;

public class PdaInitializer : SerializedMonoBehaviour
{
    public string header;
    [TextArea(10, 20)] public string body;
    [PreviewField(50), AssetsOnly] public Sprite image;

    public void SetupPDA()
    {
        UIManager.Instance.SetupPDA(header, body, image);
    }

    public void OpenPDA()
    {
        UIManager.OnTransitioned +=  UIManager.Instance.OpenPDA;
    }

    public void ClosePDA()
    {
        UIManager.OnTransitioned +=  UIManager.Instance.ClosePDA;
    }
}
