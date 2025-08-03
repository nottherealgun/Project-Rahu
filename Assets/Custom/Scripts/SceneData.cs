using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "Project Rahu/SceneData")]
public class SceneData : ScriptableObject
{
    public string sceneName { get { return scene.name; } }
    public SceneAsset scene;
}
