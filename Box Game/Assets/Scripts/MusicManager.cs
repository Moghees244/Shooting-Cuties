using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme;
    public AudioClip menuTheme;
    string sceneName;

    void Start()
    {
        onLevelWasLoaded(0);
    }

    public void onLevelWasLoaded(int sceneIndex)
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        if (newSceneName != sceneName)
        {
            sceneName = newSceneName;
            Invoke("playMusic", 0.2f);
        }
    }

    void playMusic()
    {
        AudioClip clipToPlay = mainTheme;

        if (sceneName == "Menu")
            clipToPlay = menuTheme;
        else
            clipToPlay = mainTheme;

        if (clipToPlay != null)
        {
            AudioManager.instance.playMusic(clipToPlay, 2);
            Invoke("playMusic", clipToPlay.length);
        }
    }
}
