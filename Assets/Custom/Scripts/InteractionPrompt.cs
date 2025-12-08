using System;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPrompt : SerializedMonoBehaviour
{
    public bool active = false;
    [OdinSerialize] GameObject inactivePrompt;
    [OdinSerialize] GameObject activePrompt;
    GameObject player;
    [OdinSerialize, PropertyRange(1f,20f)] float fadeDist = 2.7f;
    [OdinSerialize] Image keyboardIcon;
    [OdinSerialize] Image xboxIcon;
    [OdinSerialize] Image dualshockIcon;
    void Start()
    {
        HidePrompt();
        player = PersistentDataManager.Instance.FindPlayer();
    }

    void OnDrawGizmos() {
        Gizmos.color = new Color(1f,0f,0f,0.25f);
        Gizmos.DrawSphere(transform.position, fadeDist);    
    }

    [Button(ButtonSizes.Large)]
    public void ShowPrompt()
    {
        if (active) return;
        active = true;
        inactivePrompt.SetActive(false);
        activePrompt.SetActive(true);
    }

    [Button(ButtonSizes.Large)]
    public void HidePrompt()
    {
        if (!active) return;
        active = false;
        inactivePrompt.SetActive(true);
        activePrompt.SetActive(false);
    }

    void LateUpdate()
    {
        DoDynamicSizing();
    }

    void DoDynamicSizing()
    {
        if (player == null) return;
        // gets bigger the farther the player is, with limit at 2x size
        float distance = Vector3.Distance(player.transform.position, transform.position);
        float scale = Mathf.Clamp(1f + (distance / 10f), 1f, 10f);
        transform.localScale = new Vector3(scale, scale, scale);

        // fades out the inactive prompt when the player is far away
        float alpha = Mathf.Clamp(1f - (distance / fadeDist), 0f, 1f);
        Color inactiveColor = inactivePrompt.GetComponent<Image>().color;
        inactiveColor.a = alpha;
        inactivePrompt.GetComponent<Image>().color = inactiveColor;
    }

    [Button(ButtonSizes.Large)]
    public void TellDistance()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        Debug.Log($"Distance from player: {distance}");
    }

    public void SyncIcon(CurrentActiveDeviceManager.ActiveDevice device)
    {
        switch (device)
        {
            case CurrentActiveDeviceManager.ActiveDevice.Keyboard:
                Set(true,false,false);
                break;
            case CurrentActiveDeviceManager.ActiveDevice.Xbox:
                Set(false,true,false);
                break;
            case CurrentActiveDeviceManager.ActiveDevice.DualShock:
                Set(false,false,true);
                break;
            default:
                Set(true,false,false);
                break;
        }
    }

    private void Set(bool k, bool x, bool d)
    {
        keyboardIcon.gameObject.SetActive(k);
        xboxIcon.gameObject.SetActive(x);
        dualshockIcon.gameObject.SetActive(d);
    }
}
