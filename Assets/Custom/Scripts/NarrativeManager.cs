using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class NarrativeManager : SerializedMonoBehaviour
{
    public static NarrativeManager Instance { get; private set; }
    PersistentDataManager globalDataStore
    {
        get { return PersistentDataManager.Instance; }
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    [OdinSerialize] AudioDataStore audioDataStore;
    PlayerController playerController;

    public void StartNewGame()
    {
        playerController = PersistentDataManager.Player.GetComponent<PlayerController>();
        VoicelineStore currentVoicelineStore = AudioDataStore.Instance.scenes.scenes[globalDataStore.currentScene-1];
        AudioDataStore.DialogueLine dialogueLine = currentVoicelineStore.cutsceneVoicelines[globalDataStore.currentVoicelineID - 1];
        globalDataStore.currentVoiceline = dialogueLine.text;
        playerController.Speak(dialogueLine);
    }
}
