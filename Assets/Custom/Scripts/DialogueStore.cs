using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "DialogueStore", menuName = "Project Rahu/DialogueStore")]
public class DialogueStore : SerializedScriptableObject
{
    [Button(ButtonSizes.Large)]
    [Title("Dialogue Scriptable Objects")]
    [RequiredListLength(1,null)]
    public VoicelineStore[] scenes = new VoicelineStore[13];
}
