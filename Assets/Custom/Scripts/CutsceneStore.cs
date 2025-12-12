using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

public enum SubType { NONE, A, B }
public enum ShotType { LINEAR, EVENT, CHOICE, QTE }

[CreateAssetMenu(fileName = "CutsceneStore", menuName = "Project Rahu/CutsceneStore")]
public class CutsceneStore : SerializedScriptableObject
{
    public struct Shot
    {
        [EnumToggleButtons]
        public ShotType shotType;
        public string fileName;
        public string nextShotID;
        public bool isFinalShot;
        public Shot(string nextShotID, string fileName, ShotType shotType, bool isFinalShot = false)
        {
            this.fileName = fileName;
            this.shotType = shotType;
            this.nextShotID = nextShotID;
            this.isFinalShot = isFinalShot;
        }
    }
    public struct FrameData
    {
        [DictionaryDrawerSettings(KeyLabel = "Frame", ValueLabel = "Dialogue")]
        public Dictionary<int, string> voiceLines;
        public FrameData(Dictionary<int, string> voiceLines)
        {
            this.voiceLines = voiceLines;
        }
    }
    # region Cutscenes
    [OdinSerialize, ReadOnly, DictionaryDrawerSettings(KeyLabel = "Shot Reference", ValueLabel = "Cinematic Shot Data")]
    Dictionary<string, Shot> shotStore = new Dictionary<string, Shot>
    {   // testShotID       nextShotID      fileName     shotType         isFinalShot
        //            Choice: left,right
        //            QTE: failed,succeeded
        { "A_01",   new Shot("A_02"         ,"TestShot1", ShotType.LINEAR ) },
        { "A_02",   new Shot("A_03"         ,"TestShot2", ShotType.LINEAR ) },
        { "A_03",   new Shot(""             ,"TestShot3", ShotType.LINEAR, true ) },

        { "A_04",   new Shot("A_05"         ,"TestShot1", ShotType.LINEAR ) },
        { "A_05",   new Shot("A_06,A_07"    ,"TestShot2", ShotType.CHOICE ) },
        { "A_06",   new Shot("A_08"         ,"ChoiceA", ShotType.LINEAR ) },
        { "A_07",   new Shot("A_08"         ,"ChoiceB", ShotType.LINEAR ) },
        { "A_08",   new Shot(""             ,"TestShot1", ShotType.LINEAR, true ) },

        { "A_09",   new Shot("A_10"         ,"TestShot1", ShotType.LINEAR ) },
        { "A_10",   new Shot("A_11,A_12"    ,"QTE", ShotType.QTE ) },
        { "A_11",   new Shot("A_13"         ,"ChoiceA", ShotType.LINEAR ) },
        { "A_12",   new Shot("A_13"         ,"ChoiceB", ShotType.LINEAR ) },
        { "A_13",   new Shot(""             ,"TestShot2", ShotType.LINEAR, true ) },

        { "01_01",  new Shot("01_02","CH01_SC01_SH01", ShotType.LINEAR ) },
        { "01_02",  new Shot("","CH01_SC01_SH02", ShotType.EVENT ) },
        { "01_03",  new Shot("","CH01_SC01_SH03", ShotType.EVENT ) },
        { "01_04",  new Shot("","CH01_SC01_SH04", ShotType.EVENT ) },
        { "01_05",  new Shot("","CH01_SC01_SH05", ShotType.EVENT ) },

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
        { "07_04", new Shot("08_01","CH01_SC07_SH04", ShotType.LINEAR, true ) },

        { "08_01", new Shot("08_02","CH02_SC08_SH01", ShotType.LINEAR ) },
        { "08_02", new Shot("08_03","CH02_SC08_SH02", ShotType.LINEAR ) },
        { "08_03", new Shot("08_04","CH02_SC08_SH03", ShotType.LINEAR ) },
        { "08_04", new Shot("09_01","CH02_SC08_SH04", ShotType.LINEAR ) },
// Capstone Scope Starts here
        { "09_01", new Shot("09_02","CH02_SC09_SH01", ShotType.LINEAR ) },
        { "09_02", new Shot("09_03","CH02_SC09_SH02", ShotType.LINEAR ) },
        { "09_03", new Shot("09_04","CH02_SC09_SH03", ShotType.LINEAR ) },
        { "09_04", new Shot("09_05","CH02_SC09_SH04", ShotType.LINEAR ) },
        { "09_05", new Shot("09_06","CH02_SC09_SH05", ShotType.LINEAR ) },
        { "09_06", new Shot("10_01","CH02_SC09_SH06", ShotType.LINEAR ) },

        { "10_01", new Shot("10_02","CH02_SC10_SH01", ShotType.LINEAR ) },
        { "10_02", new Shot("10_03","CH02_SC10_SH02", ShotType.LINEAR ) },
        { "10_03", new Shot("10_04","CH02_SC10_SH03", ShotType.LINEAR ) },
        { "10_04", new Shot("","CH02_SC10_SH04", ShotType.EVENT ) },
        // 10_05 is Puzzle 2
        { "10_06", new Shot("10_06A,10_06B","CH02_SC10_SH06", ShotType.CHOICE ) },
        // Choice Prompt
        { "10_06A", new Shot("10_07A","CH02_SC10_SH06A", ShotType.LINEAR ) },
        { "10_07A", new Shot("11_01", "CH02_SC10_SH07A", ShotType.LINEAR ) },

        { "10_06B", new Shot("10_07B","CH02_SC10_SH06B", ShotType.LINEAR ) },
        { "10_07B", new Shot("10_08B","CH02_SC10_SH07B", ShotType.LINEAR ) },
        { "10_08B", new Shot("10_09B","CH02_SC10_SH08B", ShotType.LINEAR ) },
        { "10_09B", new Shot("10_10B","CH02_SC10_SH09B", ShotType.LINEAR ) },
        { "10_10B", new Shot("10_11B","CH02_SC10_SH10B", ShotType.LINEAR ) },
        { "10_11B", new Shot("10_12B","CH02_SC10_SH11B", ShotType.LINEAR ) },
        { "10_12B", new Shot("10_13B2,10_13B1","CH02_SC10_SH12B", ShotType.QTE ) },
        // QTE
        { "10_13B1", new Shot("11_01","CH02_SC10_SH13B1", ShotType.LINEAR ) },
        { "10_13B2", new Shot("11_01","CH02_SC10_SH13B2", ShotType.LINEAR ) },
        // { "10_14B", new Shot("11_01","CH02_SC10_SH14B", ShotType.LINEAR ) },

        { "11_01", new Shot("11_02","CH02_SC11_SH01", ShotType.LINEAR ) },
        { "11_02", new Shot("11_03","CH02_SC11_SH02", ShotType.LINEAR ) },
        { "11_03", new Shot("11_04","CH02_SC11_SH03", ShotType.LINEAR ) },
        { "11_04", new Shot("11_05","CH02_SC11_SH04", ShotType.LINEAR ) },
        { "11_05", new Shot("11_06","CH02_SC11_SH05", ShotType.LINEAR ) },
        { "11_06", new Shot("11_07","CH02_SC11_SH06", ShotType.LINEAR ) },
        { "11_07", new Shot("11_08","CH02_SC11_SH07", ShotType.LINEAR ) },
        { "11_08", new Shot("12_01","CH02_SC11_SH08", ShotType.LINEAR ) },

        { "12_01", new Shot("","CH02_SC12_SH01", ShotType.EVENT ) },
        { "12_02", new Shot("ENDING","CH02_SC12_SH02", ShotType.LINEAR ) },
// Capstone Scope Ends here
        // { "13_01", new Shot("13_02","CH02_SC13_SH01", ShotType.LINEAR ) },
        // { "13_02", new Shot("13_03","CH02_SC13_SH02", ShotType.LINEAR ) },
        // { "13_03", new Shot("13_04","CH02_SC13_SH03", ShotType.LINEAR ) },
        // { "13_04", new Shot("13_05","CH02_SC13_SH04", ShotType.LINEAR ) },
        // { "13_05", new Shot("13_06","CH02_SC13_SH05", ShotType.LINEAR ) },
        // { "13_06", new Shot("13_07","CH02_SC13_SH06", ShotType.LINEAR ) },
        // { "13_07", new Shot("13_08","CH02_SC13_SH07", ShotType.LINEAR ) },
        // { "13_08", new Shot("13_09","CH02_SC13_SH08", ShotType.LINEAR ) },
        // { "13_09", new Shot("13_10","CH02_SC13_SH09", ShotType.LINEAR ) },
        // { "13_10", new Shot("13_11","CH02_SC13_SH10", ShotType.LINEAR ) },
        // { "13_11", new Shot("13_12","CH02_SC13_SH11", ShotType.LINEAR ) },
        // { "13_12", new Shot("13_13","CH02_SC13_SH12", ShotType.LINEAR ) },
        // { "13_13", new Shot("13_14","CH02_SC13_SH13", ShotType.LINEAR ) },
        // { "13_14", new Shot("13_15","CH02_SC13_SH14", ShotType.LINEAR ) },
        // { "13_15", new Shot("13_16","CH02_SC13_SH15", ShotType.LINEAR ) },
        // { "13_16", new Shot("13_17","CH02_SC13_SH16", ShotType.LINEAR ) },
        // { "13_17", new Shot("13_18","CH02_SC13_SH17", ShotType.LINEAR ) },
        // { "13_18", new Shot("13_19A,13_19B","CH02_SC13_SH18", ShotType.CHOICE ) },
        // // Choice Prompt

        // { "13_19A", new Shot("13_20A","CH02_SC13_SH19A", ShotType.LINEAR ) },
        // { "13_20A", new Shot("13_21A","CH02_SC13_SH20A", ShotType.LINEAR ) },
        // { "13_21A", new Shot("13_22A","CH02_SC13_SH21A", ShotType.LINEAR ) },
        // { "13_22A", new Shot("13_23A","CH02_SC13_SH22A", ShotType.LINEAR ) },
        // { "13_23A", new Shot("13_24A","CH02_SC13_SH23A", ShotType.LINEAR ) },
        // { "13_24A", new Shot("13_25A","CH02_SC13_SH24A", ShotType.LINEAR ) },
        // { "13_25A", new Shot("13_26A","CH02_SC13_SH25A", ShotType.LINEAR ) },
        // { "13_26A", new Shot("","CH02_SC13_SH26A", ShotType.EVENT ) },

        // { "13_19B", new Shot("13_20B","CH02_SC13_SH19B", ShotType.LINEAR ) },
        // { "13_20B", new Shot("13_21B2,13_21B1","CH02_SC13_SH20B", ShotType.QTE ) },
        // // QTE

        // { "13_21B2", new Shot("13_22B2","CH02_SC13_SH21B2", ShotType.LINEAR ) },
        // { "13_22B2", new Shot("13_23B2","CH02_SC13_SH22B2", ShotType.LINEAR ) },
        // { "13_23B2", new Shot("ENDING","CH02_SC13_SH23B2", ShotType.LINEAR ) },

        // { "13_21B1", new Shot("13_22B1","CH02_SC13_SH21B1", ShotType.LINEAR ) },
        // { "13_22B1", new Shot("13_23B1","CH02_SC13_SH22B1", ShotType.LINEAR ) },
        // { "13_23B1", new Shot("13_24B1","CH02_SC13_SH23B1", ShotType.LINEAR ) },
        // { "13_24B1", new Shot("13_25B1","CH02_SC13_SH24B1", ShotType.LINEAR ) },
        // { "13_25B1", new Shot("13_26B1","CH02_SC13_SH25B1", ShotType.LINEAR ) },
        // { "13_26B1", new Shot("13_27B1","CH02_SC13_SH26B1", ShotType.LINEAR ) },
        // { "13_27B1", new Shot("13_28B1","CH02_SC13_SH27B1", ShotType.LINEAR ) },
        // { "13_28B1", new Shot("13_29B1","CH02_SC13_SH28B1", ShotType.LINEAR ) },
        // { "13_29B1", new Shot("13_30B1,13_30B1","CH02_SC13_SH29B1", ShotType.CHOICE ) },
        // // Choice Prompt

        // { "13_30B1", new Shot("ENDING","CH02_SC13_SH30B1", ShotType.LINEAR ) },

        { "ENDING", new Shot("","Demo3_Epilogue", ShotType.LINEAR, true ) }
    };
    # endregion
    # region Voicelines
    [OdinSerialize, ReadOnly, DictionaryDrawerSettings(KeyLabel = "Shot Reference", ValueLabel = "Dialogue Data")]
    Dictionary<string, FrameData> voiceLineStore = new Dictionary<string, FrameData>
    {               // Frame     | Dialogue Text
        { "10_04", new FrameData(new Dictionary<int, string>
            {
                { 87, "FASAI: Oh, let\'s see what I can do here." }
            })
        },
        { "10_06", new FrameData(new Dictionary<int, string>
            {
                { 3, "FASAI: Okay, where was I?" },
                { 87, "FASAI: Oh, should I call Ajarn Somchai?" }
            })
        },
        { "10_06A", new FrameData(new Dictionary<int, string>
            {
                { 3, "FASAI: Ajarn Somchai." },
                { 35, "FASAI: Do you copy? Fasai's here." },
                { 111, "FASAI: Ajarn Somchai." }
            })
        },
        { "10_07A", new FrameData(new Dictionary<int, string>
            {
                { 87, "FASAI: Who's there?!" }
            })
        },
        { "10_08B", new FrameData(new Dictionary<int, string>
            {
                { 5, "FASAI: Who's that?!" }
            })
        },
        { "11_01", new FrameData(new Dictionary<int, string>
            {
                { 121, "FASAI: Nate?!" }
            })
        },
        { "11_02", new FrameData(new Dictionary<int, string>
            {
                { 69-40, "FASAI: Nate! Nate! Get a grip!" },
                { 137-40, "NATE: Fasai. Thank god you are here." },
                { 197-40, "NATE: There\'s something out there." },
                { 243-40, "NATE: It\'s lurking in the dark." },
                { 302-40, "NATE: Please tell me you noticed that too." },
                { 362-40, "NATE: Please tell me that there\'s-" },
                { 398-40, "NATE: something going wrong and it\'s not just me." },
                { 452-40, "FASAI: I noticed that." },
                { 513-40, "NATE: Thank god. I thought I was going crazy." }
            })
        },
        { "11_04", new FrameData(new Dictionary<int, string>
            {
                { 22, "SOMCHAI: Attention, everyone." },
                { 68, "SOMCHAI: I would like everyone to report where you are right now." }
            })
        },
        { "11_05", new FrameData(new Dictionary<int, string>
            {
                { 42, "FASAI: I\'m at the lab right now,-" },
                { 98, "FASAI: with Nate." }
            })
        },
        { "11_06", new FrameData(new Dictionary<int, string>
            {
                { 23, "GIGI: I'm with this pervert,-" },
                { 65, "GIGI: Oak." },
                { 118, "OAK: Pervert?!" },
                { 144, "OAK: You were literally in the guy\'s restroom!" },
                { 217, "GIGI: Nonsense." },
                { 271, "JENNY: I\'m at the dorm right now." },
                { 352, "SOMCHAI: OK." },
                { 375, "SOMCHAI: I\'m assigning P\'Pon with Jenny." }
            })
        },
        { "11_07", new FrameData(new Dictionary<int, string>
            {
                { 10, "SOMCHAI: Just wait there." },
                { 64, "FASAI: What\'s happening, sir?" },
                { 100, "SOMCHAI: There\'s no time to explain." }
            })
        },
        { "11_08", new FrameData(new Dictionary<int, string>
            {
                { 32, "SOMCHAI: Just go get the listed items." },
                { 112, "SOMCHAI: I have sent you the list of materials for your PDAs." },
                { 222, "SOMCHAI: Head back to my research lab once you've gathered everything." },
                { 329, "SOMCHAI: You two, dismissed." },
                { 406, "SOMCHAI: Now Oak and Gigi-" }
            })
        },
        { "12_02", new FrameData(new Dictionary<int, string>
            {
                { 6, "FASAI: That should be the last one." },
                { 75, "NATE: Well,-" },
                { 97, "NATE: that wasn\'t so hard." },
                { 156, "FASAI: Yeah, let\'s go back to Ajarn Somchai." },
                { 232, "FASAI: It must have been pretty urgent if we need this kind of stuff." },
                { 337, "NATE: Let\'s get out of here." },
            })
        },
    };
    # endregion
    public Shot GetShot(string shotID)
    {
        return shotStore[shotID];
    }
    public string GetVoicelineAt(string shotID, int frame)
    {
        if (!voiceLineStore.ContainsKey(shotID) || !voiceLineStore[shotID].voiceLines.ContainsKey(frame))
        {
            return "";
        }
        return voiceLineStore[shotID].voiceLines[frame];
    }

    public struct ChoicePromptData
    {
        public string leftHeader;
        public string leftBody;
        public string rightHeader;
        public string rightBody;

        public ChoicePromptData(string leftHeader, string leftBody, string rightHeader, string rightBody)
        {
            this.leftHeader = leftHeader;
            this.leftBody = leftBody;
            this.rightHeader = rightHeader;
            this.rightBody = rightBody;
        }
    }

    [OdinSerialize, ReadOnly, DictionaryDrawerSettings(KeyLabel = "ChoicePrompt Name", ValueLabel = "ChoicePrompt Data")]
    Dictionary<string, ChoicePromptData> choicePromptStore = new Dictionary<string, ChoicePromptData>
    {
        { "05_03",      new ChoicePromptData("STERILISED PROTOCOL", "Side With Somchai", "HANDS-ON EMERGENCY", "Side With Rueangsak")},
        { "10_06",      new ChoicePromptData("INSECURE", "Contact Professor Somchai", "ADVENTUROUS", "Investigate The Corridor") },
        { "13_18",      new ChoicePromptData("ESCAPE", "Get To Safety", "RESCUE", "Risk Saving Your Friend") },
        { "13_29B1",    new ChoicePromptData("THANKFUL", "Thanks for asking, Nate.", "COLD", "Take care of yourself.") },

        { "A_05",    new ChoicePromptData("TEST CHOICE A", "DESCRIPTION A", "TEST CHOICE B", "DESCRIPTION B") },
    };

    public ChoicePromptData GetChoicePromptData(string shotID)
    {
        return choicePromptStore[shotID];
    }

    public struct QTEPromptData
    {
        public QTEPrompt.KeyPrompt keyPrompt;
        public Vector3 onScreenPosition;
        public QTEPromptData(QTEPrompt.KeyPrompt keyPrompt, Vector3 onScreenPosition)
        {
            this.keyPrompt = keyPrompt;
            this.onScreenPosition = onScreenPosition;
        }
    }

    [OdinSerialize, ReadOnly, DictionaryDrawerSettings(KeyLabel = "QTEPrompt Name", ValueLabel = "QTEPrompt Data")]
    Dictionary<string, QTEPromptData> qtePromptStore = new Dictionary<string, QTEPromptData>
    {
        { "A_10", new QTEPromptData(QTEPrompt.KeyPrompt.QTE1, new Vector3(-483,-120,0)) },
        { "10_12B", new QTEPromptData(QTEPrompt.KeyPrompt.QTE1, new Vector3(-483,-120,0)) },
        { "10_20B", new QTEPromptData(QTEPrompt.KeyPrompt.QTE1, new Vector3(0,0,0)) }
    };

    public QTEPromptData GetQTEPromptData(string shotID)
    {
        return qtePromptStore[shotID];
    }
}
