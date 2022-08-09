using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;
    public UnityEngine.UIElements.Slider[] sliders;
    public UnityEngine.UI.Toggle[] toogles;
    public UnityEngine.UI.Toggle fullScreenToogle;
    public int[] screenWidths;
    public int currentScreenResolutionIndex;

    private void Start()
    {
        currentScreenResolutionIndex = PlayerPrefs.GetInt("screen res index");
        bool isFullScreen = (PlayerPrefs.GetInt("Full Screen") == 1) ? true : false;

        /* sliders[0].value = AudioManager.instance.masterVolumePercent;
         sliders[1].value = AudioManager.instance.musicVolumePercent;
         sliders[2].value = AudioManager.instance.sfxVolumePercent;*/

        for (int i = 0; i < toogles.Length; i++)
            toogles[i].isOn = i == currentScreenResolutionIndex;

        fullScreenToogle.isOn = isFullScreen;
    }

    public void Play()
    {
        ScoreKeeper.score = 0;
        SceneManager.LoadScene("PlayMode");
    }

    public void OptionsMenu()
    {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }

    public void quit()
    {
        Application.Quit();
    }

    public void mainMenu()
    {
        optionsMenuHolder.SetActive(false);
        mainMenuHolder.SetActive(true);
    }

    public void setScreenResolution(int i)
    {
        if (toogles[i].isOn)
        {
            currentScreenResolutionIndex = i;
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);
            PlayerPrefs.SetInt("screen res index", currentScreenResolutionIndex);
            PlayerPrefs.Save();
        }
    }

    public void setFullScreen(bool full)
    {
        for (int i = 0; i < toogles.Length; i++)
            toogles[i].interactable = !full;

        if (full)
        {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResolution = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        }
        else
            setScreenResolution(currentScreenResolutionIndex);

        PlayerPrefs.SetInt("Full Screen", ((full) ? 1 : 0));
        PlayerPrefs.Save();
    }

    public void setMasterVolume(float value)
    {
        AudioManager.instance.setVolume(value, AudioManager.AudioChannel.Master);
    }

    public void setMusicVolume(float value)
    {
        AudioManager.instance.setVolume(value, AudioManager.AudioChannel.Music);
    }

    public void setSfxVolume(float value)
    {
        AudioManager.instance.setVolume(value, AudioManager.AudioChannel.SFX);
    }
}
