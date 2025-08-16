using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;
using System.Numerics;
public class Gem : SerializedMonoBehaviour
{
    public List<Sprite> gemTextures = new List<Sprite>();
    public enum GemTypes
    {
        TYPE1, TYPE2, TYPE3, TYPE4, TYPE5
    }



    public static int gemTypeAmnt
    {
        get { return Enum.GetNames(typeof(GemTypes)).Length; }
    }

    public static int GetRandomGemType(bool includeKey = false)
    {
        if (!includeKey)
            return UnityEngine.Random.Range(0, gemTypeAmnt - 1);
        else
        {
            if (UnityEngine.Random.Range(0f, 100f) < 5f)
            {
                return gemTypeAmnt - 1;
            }else return UnityEngine.Random.Range(0, gemTypeAmnt - 1);
        }
    }

    [OdinSerialize] GemTypes _gemType = GemTypes.TYPE1;
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

    [OdinSerialize, ReadOnly]
    public Vector2Int gemPosition;

    public Action<Gem, GameObject> onDisplayGridPosition;
    TMP_Text testText;

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
        testText = GetComponentInChildren<TMP_Text>();
    }

    private void Start() {
        gameObject.name = $"Gem {UnityEngine.Random.Range(0, 1000)}";
    }

    private void Update()
    {
        // testText.text = $"({gemPosition.x}, {gemPosition.y})";
    }
}
