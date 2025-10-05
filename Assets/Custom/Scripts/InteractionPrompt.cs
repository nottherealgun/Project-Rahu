using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class InteractionPrompt : SerializedMonoBehaviour
{
    public bool active = false;
    [OdinSerialize] GameObject InactivePrompt;
    [OdinSerialize] GameObject ActivePrompt;
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
        InactivePrompt.SetActive(false);
        ActivePrompt.SetActive(true);
    }

    [Button(ButtonSizes.Large)]
    public void HidePrompt()
    {
        if (!active) return;
        active = false;
        InactivePrompt.SetActive(true);
        ActivePrompt.SetActive(false);
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
    }
}
