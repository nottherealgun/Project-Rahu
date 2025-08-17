using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;
public class Crystal : SerializedMonoBehaviour
{
    public List<Sprite> crystalTextures = new List<Sprite>();
    public enum CrystalTypes
    {
        TYPE1, TYPE2, TYPE3, TYPE4, TYPE5
    }



    public static int crystalTypeAmnt
    {
        get { return Enum.GetNames(typeof(CrystalTypes)).Length; }
    }

    public static int GetRandomCrystalType(bool includeKey = false)
    {
        if (!includeKey)
            return UnityEngine.Random.Range(0, crystalTypeAmnt - 1);
        else
        {
            if (UnityEngine.Random.Range(0f, 100f) < 5f)
            {
                return crystalTypeAmnt - 1;
            }
            else return UnityEngine.Random.Range(0, crystalTypeAmnt - 1);
        }
    }

    [OdinSerialize] CrystalTypes _crystalType = CrystalTypes.TYPE1;
    public CrystalTypes crystalType
    {
        get { return _crystalType; }
        set
        {
            _crystalType = value;
            if (image == null) TryGetComponent<Image>(out image);
            image.sprite = crystalTextures[(int)_crystalType];
        }
    }
    Image image;
    public bool marked = false;

    [OdinSerialize, ReadOnly]
    public Vector2Int crystalPosition;

    [HideInInspector] public Action<Crystal, GameObject> onDisplayGridPosition;
    TMP_Text testText;

    public void SetCrystalType(int newCrystalType)
    {
        crystalType = (CrystalTypes)newCrystalType;
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

    private void Start()
    {
        gameObject.name = $"Crystal {UnityEngine.Random.Range(0, 1000)}";
    }

    private void Update()
    {
        // testText.text = $"({crystalPosition.x}, {crystalPosition.y})";
    }
}
