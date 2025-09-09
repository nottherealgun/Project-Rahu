using UnityEngine.InputSystem;

public class SettingsMenu : Menu
{
    public void OnMenu(InputValue value)
    {
        LoadMainMenu();
    }

    public async void LoadMainMenu()
    {
        ScenesManager.Instance.ShowLoadingScreen();
        await ScenesManager.Instance.LoadScene("MainMenu");
        ScenesManager.Instance.HideLoadingScreen();
        ScenesManager.Instance.ShowScene();
    }
}
