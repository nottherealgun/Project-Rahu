using UnityEngine;

public class PauseMenu : Menu
{
    public void QuitGame()
    {
        Application.Quit();
    }

    void OnEnable()
    {
        ReselectFirst();
    }
}
