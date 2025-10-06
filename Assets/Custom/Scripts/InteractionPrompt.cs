using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPrompt : SerializedMonoBehaviour
{
    public bool active = false;
    [OdinSerialize] GameObject inactivePrompt;
    [OdinSerialize] GameObject activePrompt;
    GameObject player;
    void Start()
    {
        HidePrompt();
        player = PersistentDataManager.Instance.FindPlayer();
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
        float scale = Mathf.Clamp(1f + (distance / 5f), 1f, 10f);
        transform.localScale = new Vector3(scale, scale, scale);

        // fades out the inactive prompt when the player is far away
        float alpha = Mathf.Clamp(1f - (distance / 10f), 0f, 1f);
        Color inactiveColor = inactivePrompt.GetComponent<Image>().color;
        inactiveColor.a = alpha;
        inactivePrompt.GetComponent<Image>().color = inactiveColor;
    }
}
