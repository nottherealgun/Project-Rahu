using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Video;

public class IntroSequenceManager : SerializedMonoBehaviour
{
    [OdinSerialize] PlayableDirector playableDirector;
    [OdinSerialize] VideoPlayer videoPlayer;
    [OdinSerialize] GameObject promptContainer;

    void Start()
    {
        EnvironmentalAudioManager.Instance.PlayMusic("main_menu_music");
        playableDirector.stopped += async (director) => await StartDemo();

        videoPlayer.loopPointReached += (vp) => print("Loading demo...");
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += (vp) => Play();
    }
    
    [Button(ButtonSizes.Large)]
    void Play()
    {
        videoPlayer.Play();
        playableDirector.Play();
    }

    public void OnClick(InputValue _value)
    {
        if (videoPlayer.isPaused == false) return;
        playableDirector.Resume();
        videoPlayer.Play();
        promptContainer.SetActive(false);
    }

    async UniTask StartDemo()
    {
        ScenesManager.Instance.ShowLoadingScreen();
        await ScenesManager.Instance.LoadScene("MainMenu");
        ScenesManager.Instance.HideLoadingScreen();
        ScenesManager.Instance.ShowScene();

        await UIManager.Instance.ManualFadeOut();
    }
}
