using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class WirePort : SerializedMonoBehaviour
{
    enum PortMode { IN, OUT }

    [SerializeField]
    Dictionary<string, Material> glowMaterials = new Dictionary<string, Material>
    {
        { "red", null },
        { "green", null },
        { "blue", null },
        { "yellow", null }
    };

    void Awake()
    {
        DisableGlow("red");
        DisableGlow("green");
        DisableGlow("blue");
        DisableGlow("yellow");
    }
    
    void OnValidate()
    {
        DisableGlow("red");
        DisableGlow("green");
        DisableGlow("blue");
        DisableGlow("yellow");
    }

    public void DisableGlow(string color)
    {
        Material glowMaterial = glowMaterials[color];
        glowMaterial.SetFloat("_GlowAmount", 0f);
    }

    public void EnableGlow(string color)
    {
        Material glowMaterial = glowMaterials[color];
        glowMaterial.SetFloat("_GlowAmount", -20f);
    }
}
