using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel
    {
        Master,
        SFX,
        Music
    };

    public float masterVolumePercent { get; private set; }
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }
    AudioSource[] musicSources;
    int activeMusicSource;
    public static AudioManager instance;
    Transform audioListener;
    Transform PlayerT;
    SoundLibrary library;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLibrary>();
            instance = this;
            musicSources = new AudioSource[2];

            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            audioListener = FindObjectOfType<AudioListener>().transform;

            if (FindObjectOfType<PlayerController>() != null)
                PlayerT = FindObjectOfType<PlayerController>().transform;

            masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", 1);
        }
    }

    void Update()
    {
        if (PlayerT != null)
            audioListener.position = PlayerT.position;
    }

    public void setVolume(float volume, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volume;
                break;

            case AudioChannel.SFX:
                sfxVolumePercent = volume;
                break;

            case AudioChannel.Music:
                musicVolumePercent = volume;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    public void playMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSource = 1 - activeMusicSource;
        musicSources[activeMusicSource].clip = clip;
        musicSources[activeMusicSource].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
    }

    public void playSound(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
    }

    public void playSound(string soundName, Vector3 pos)
    {
        playSound(library.getClipByName(soundName), pos);
    }

    IEnumerator AnimateMusicCrossFade(float duration)
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSource].volume = Mathf.Lerp(
                0,
                musicVolumePercent * masterVolumePercent,
                percent
            );
            musicSources[1 - activeMusicSource].volume = Mathf.Lerp(
                musicVolumePercent * masterVolumePercent,
                0,
                percent
            );
            yield return null;
        }
    }
}
