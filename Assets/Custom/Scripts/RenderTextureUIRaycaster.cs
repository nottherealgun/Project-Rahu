using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RenderTextureUIRaycaster : MonoBehaviour
{
    public Camera uiCamera;        // Camera rendering the UI to RT
    public EventSystem eventSystem;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            bool isPressing = Input.GetMouseButtonDown(0);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject) // this object has the RT
                {
                    // Convert hit point to local UV (0-1 range)
                    Vector2 uv = hit.textureCoord;

                    // Convert UV to pixel coords for UI camera
                    Vector2 pixelPos = new Vector2(
                        uv.x * uiCamera.pixelWidth,
                        uv.y * uiCamera.pixelHeight
                    );

                    // Create PointerEvent
                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                    pointerData.position = pixelPos;

                    // Raycast into UI
                    var results = new List<RaycastResult>();
                    eventSystem.RaycastAll(pointerData, results);

                    foreach (var r in results)
                    {
                        var btn = r.gameObject.GetComponent<UnityEngine.UI.Button>();
                        if (btn == null) continue;
                        if (isPressing)
                        {
                            ExecuteEvents.Execute(btn.gameObject, pointerData, ExecuteEvents.pointerDownHandler);
                            ExecuteEvents.Execute(btn.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                            // btn.onClick.Invoke();
                            Debug.Log(btn.name + " clicked!");
                        }
                        else
                        {
                            ExecuteEvents.Execute(btn.gameObject, pointerData, ExecuteEvents.pointerUpHandler);
                        }
                    }
                }
            }
        }
    }
}
