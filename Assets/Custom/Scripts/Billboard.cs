using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class Billboard : SerializedMonoBehaviour
{
    [OdinSerialize] BillboardType billboardType = BillboardType.CameraForward;
    public enum BillboardType { LookAtCamera, CameraForward }

    private void LateUpdate() {
        switch (billboardType)
        {
            case BillboardType.LookAtCamera:
                transform.LookAt(Camera.main.transform, Vector3.up);
                break;
            case BillboardType.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            default:
                break;
        }
    }
}
