using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;
public class Gem : SerializedMonoBehaviour
{
    public List<Sprite> gemTextures = new List<Sprite>();
    public enum GemTypes
    {
        TYPE1, TYPE2, TYPE3, TYPE4, TYPE5
    }

    

    static int gemTypeAmnt
    {
        get { return Enum.GetNames(typeof(GemTypes)).Length; }
    }

    public static int GetRandomGemType()
    {
        return UnityEngine.Random.Range(0, gemTypeAmnt - 1);
    }

    public GemTypes _gemType = GemTypes.TYPE1;
    public GemTypes gemType
    {
        get { return _gemType; }
        set
        {
            _gemType = value;
            if (image == null) TryGetComponent<Image>(out image);
            image.sprite = gemTextures[(int)_gemType];
        }
    }
    Image image;
    public bool marked = false;

    public void SetGemType(int newGemType)
    {
        gemType = (GemTypes)newGemType;
    }

    public void MarkForDestroy()
    {
        marked = true;
        image.enabled = false;
    }
    
    private void Awake()
    {
        TryGetComponent<Image>(out image);
    }
}
