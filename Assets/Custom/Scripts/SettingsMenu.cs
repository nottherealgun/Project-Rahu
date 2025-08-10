using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsMenu : Menu
{
    void Update() {
        if(UI["cancel"].triggered)
        {
            ScenesManager.Instance.LoadScene("MainMenu");
        }
    }
}
