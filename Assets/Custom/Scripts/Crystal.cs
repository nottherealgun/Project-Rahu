using UnityEngine;
using UnityEngine.Events;
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
    [OdinSerialize] Image image;
    [OdinSerialize] Image selectingVisual;
    public bool marked = false;

    [OdinSerialize, ReadOnly]
    public Vector2Int crystalPosition;

    [HideInInspector] public UnityAction<Crystal, GameObject> OnGridPositionDisplayed;
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

    public void Mark()
    {
        selectingVisual.gameObject.SetActive(true);
    }
    
    public void Unmark()
    {
        selectingVisual.gameObject.SetActive(false);
    }

    void Awake()
    {
        testText = GetComponentInChildren<TMP_Text>();
    }

    void Start()
    {
        gameObject.name = $"Crystal {UnityEngine.Random.Range(0, 1000)}";
    }

    void Update()
    {
        // testText.text = $"({crystalPosition.x}, {crystalPosition.y})";
    }
}
