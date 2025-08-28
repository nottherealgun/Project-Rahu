using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

public enum SubType { NONE, A, B }
public enum ShotType { LINEAR, EVENT }

[CreateAssetMenu(fileName = "CutsceneStore", menuName = "Project Rahu/CutsceneStore")]
public class CutsceneStore : SerializedScriptableObject
{
    public struct Shot
    {
        [OdinSerialize, EnumToggleButtons] public ShotType shotType;
        public string fileName;
        [OdinSerialize, ShowIf("@shotType == ShotType.LINEAR")] public string nextShotID;
        [OdinSerialize, ShowIf("@shotType == ShotType.LINEAR")] public bool isFinalShot;
        public Shot(string nextShotID, string fileName, ShotType shotType, bool isFinalShot = false)
        {
            this.fileName = fileName;
            this.shotType = shotType;
            this.nextShotID = nextShotID;
            this.isFinalShot = isFinalShot;
        }
    }

    [OdinSerialize, ReadOnly, DictionaryDrawerSettings(KeyLabel = "Shot Reference", ValueLabel = "Cinematic Shot Data")]
    Dictionary<string, Shot> shotStore = new Dictionary<string, Shot>
    {
        
        { "A", new Shot("B","TestShot1", ShotType.LINEAR ) },
        { "B", new Shot("C","TestShot2", ShotType.LINEAR ) },
        { "C", new Shot("","TestShot3", ShotType.LINEAR, true ) },
        
        { "01_01", new Shot("01_02","CH01_SC01_SH01", ShotType.LINEAR ) },
        { "01_02", new Shot("","CH01_SC01_SH02", ShotType.EVENT ) },
        { "01_03", new Shot("","CH01_SC01_SH03", ShotType.EVENT ) },
        { "01_04", new Shot("","CH01_SC01_SH04", ShotType.EVENT ) },
        { "01_05", new Shot("","CH01_SC01_SH05", ShotType.EVENT ) },

        { "01_06", new Shot("02_01","CH01_SC01_SH06", ShotType.LINEAR ) },

        { "02_01", new Shot("02_02","CH01_SC02_SH01", ShotType.LINEAR ) },
        { "02_02", new Shot("02_03","CH01_SC02_SH02", ShotType.LINEAR ) },
        { "02_03", new Shot("02_04","CH01_SC02_SH03", ShotType.LINEAR ) },
        { "02_04", new Shot("02_05","CH01_SC02_SH04", ShotType.LINEAR ) },
        { "02_05", new Shot("03_01","CH01_SC02_SH05", ShotType.LINEAR ) },

        { "03_01", new Shot("03_02","CH01_SC03_SH01", ShotType.LINEAR ) },
        { "03_02", new Shot("03_03","CH01_SC03_SH02", ShotType.LINEAR ) },
        { "03_03", new Shot("04_01","CH01_SC03_SH03", ShotType.LINEAR ) },

        { "04_01", new Shot("04_02","CH01_SC04_SH01", ShotType.LINEAR ) },
        { "04_02", new Shot("04_03","CH01_SC04_SH02", ShotType.LINEAR ) },
        { "04_03", new Shot("04_04","CH01_SC04_SH03", ShotType.LINEAR ) },
        { "04_04", new Shot("04_05","CH01_SC04_SH04", ShotType.LINEAR ) },
        { "04_05", new Shot("04_06","CH01_SC04_SH05", ShotType.LINEAR ) },
        { "04_06", new Shot("04_07","CH01_SC04_SH06", ShotType.LINEAR ) },
        { "04_07", new Shot("04_08","CH01_SC04_SH07", ShotType.LINEAR ) },
        { "04_08", new Shot("04_09","CH01_SC04_SH08", ShotType.LINEAR ) },
        { "04_09", new Shot("05_01","CH01_SC04_SH09", ShotType.LINEAR ) },

        { "05_01", new Shot("05_02","CH01_SC05_SH01", ShotType.LINEAR ) },
        { "05_02", new Shot("05_03","CH01_SC05_SH02", ShotType.LINEAR ) },
        { "05_03", new Shot("","CH01_SC05_SH03", ShotType.EVENT ) },

        { "05_04A", new Shot("05_05","CH01_SC05_SH04A", ShotType.LINEAR ) },
        { "05_04B1", new Shot("05_04B2","CH01_SC05_SH04B1", ShotType.LINEAR ) },
        { "05_04B2", new Shot("05_04B3","CH01_SC05_SH04B2", ShotType.LINEAR ) },
        { "05_04B3", new Shot("05_05",  "CH01_SC05_SH04B3", ShotType.LINEAR ) },
        { "05_05", new Shot("05_06","CH01_SC05_SH05", ShotType.LINEAR ) },
        { "05_06", new Shot("05_07","CH01_SC05_SH06", ShotType.LINEAR ) },
        { "05_07", new Shot("","CH01_SC05_SH07", ShotType.EVENT ) },
        // 05_08 is Puzzle 1
        { "05_09", new Shot("05_10","CH01_SC05_SH09", ShotType.LINEAR ) },
        { "05_10", new Shot("06_01","CH01_SC05_SH10", ShotType.LINEAR ) },

        { "06_01", new Shot("06_02","CH01_SC06_SH01", ShotType.LINEAR ) },
        { "06_02", new Shot("06_03","CH01_SC06_SH02", ShotType.LINEAR ) },
        { "06_03", new Shot("06_04","CH01_SC06_SH03", ShotType.LINEAR ) },
        { "06_04", new Shot("07_01","CH01_SC06_SH04", ShotType.LINEAR ) },

        { "07_01", new Shot("07_02","CH01_SC07_SH01", ShotType.LINEAR ) },
        { "07_02", new Shot("07_03","CH01_SC07_SH02", ShotType.LINEAR ) },
        { "07_03", new Shot("07_04","CH01_SC07_SH03", ShotType.LINEAR ) },
        { "07_04", new Shot("08_01","CH01_SC07_SH04", ShotType.LINEAR ) },

        { "08_01", new Shot("08_02","CH02_SC08_SH01", ShotType.LINEAR ) },
        { "08_02", new Shot("08_03","CH02_SC08_SH02", ShotType.LINEAR ) },
        { "08_03", new Shot("08_04","CH02_SC08_SH03", ShotType.LINEAR ) },
        { "08_04", new Shot("09_01","CH02_SC08_SH04", ShotType.LINEAR ) },

        { "09_01", new Shot("09_02","CH02_SC09_SH01", ShotType.LINEAR ) },
        { "09_02", new Shot("09_03","CH02_SC09_SH02", ShotType.LINEAR ) },
        { "09_03", new Shot("09_04","CH02_SC09_SH03", ShotType.LINEAR ) },
        { "09_04", new Shot("09_05","CH02_SC09_SH04", ShotType.LINEAR ) },
        { "09_05", new Shot("09_06","CH02_SC09_SH05", ShotType.LINEAR ) },
        { "09_06", new Shot("10_01","CH02_SC09_SH06", ShotType.LINEAR ) },

        { "10_01", new Shot("10_02","CH02_SC10_SH01", ShotType.LINEAR ) },
        { "10_02", new Shot("10_03","CH02_SC10_SH02", ShotType.LINEAR ) },
        { "10_03", new Shot("","CH02_SC10_SH03", ShotType.EVENT ) },
        // 10_04 is Puzzle 2
        { "10_05", new Shot("","CH02_SC10_SH05", ShotType.EVENT ) },
        { "10_06A", new Shot("10_07A","CH02_SC10_SH06A", ShotType.LINEAR ) },
        { "10_07A", new Shot("11_01", "CH02_SC10_SH07A", ShotType.LINEAR ) },
        { "10_06B", new Shot("10_07B","CH02_SC10_SH06B", ShotType.LINEAR ) },
        { "10_07B", new Shot("10_08B","CH02_SC10_SH07B", ShotType.LINEAR ) },
        { "10_08B", new Shot("10_09B","CH02_SC10_SH08B", ShotType.LINEAR ) },
        { "10_09B", new Shot("10_10B","CH02_SC10_SH09B", ShotType.LINEAR ) },
        { "10_10B", new Shot("10_11B","CH02_SC10_SH10B", ShotType.LINEAR ) },
        { "10_11B", new Shot("10_12B","CH02_SC10_SH11B", ShotType.LINEAR ) },
        { "10_12B", new Shot("","CH02_SC10_SH12B", ShotType.EVENT ) },

        { "10_13B", new Shot("10_15B","CH02_SC10_SH13B", ShotType.LINEAR ) },
        { "10_14B", new Shot("10_15B","CH02_SC10_SH14B", ShotType.LINEAR ) },
        { "10_15B", new Shot("11_01", "CH02_SC10_SH15B", ShotType.LINEAR ) },

        { "11_01", new Shot("11_02","CH02_SC11_SH01", ShotType.LINEAR ) },
        { "11_02", new Shot("11_03","CH02_SC11_SH02", ShotType.LINEAR ) },
        { "11_03", new Shot("11_04","CH02_SC11_SH03", ShotType.LINEAR ) },
        { "11_04", new Shot("11_05","CH02_SC11_SH04", ShotType.LINEAR ) },
        { "11_05", new Shot("11_06","CH02_SC11_SH05", ShotType.LINEAR ) },
        { "11_06", new Shot("11_07","CH02_SC11_SH06", ShotType.LINEAR ) },
        { "11_07", new Shot("11_08","CH02_SC11_SH07", ShotType.LINEAR ) },
        { "11_08", new Shot("12_01","CH02_SC11_SH08", ShotType.LINEAR ) },

        { "12_01", new Shot("12_02","CH02_SC12_SH01", ShotType.EVENT ) },
        { "12_02", new Shot("","CH02_SC12_SH02", ShotType.LINEAR, true ) },

        { "13_01", new Shot("","CH02_SC13_SH01", ShotType.LINEAR ) }
    };

    public Shot GetShot(string shotID)
    {
        return shotStore[shotID];
    }
}
