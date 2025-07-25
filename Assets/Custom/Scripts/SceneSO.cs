using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneSO", menuName = "Project Rahu/SceneSO")]
public class SceneSO : ScriptableObject
{
    public string sceneName;
    public SceneAsset scene;
}
