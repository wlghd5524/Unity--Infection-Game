using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public AudioSource backgroundMusic;
    public AudioSource tenseBGM;
    public AudioSource[] sfxSources;

    private float masterVolume = 1.0f;
    private float musicVolume = 1.0f;
    private float sfxVolume = 1.0f;

    private void Awake()
    {
        // 배경음악 초기화
        if (backgroundMusic == null)
        {
            backgroundMusic = GetComponent<AudioSource>();
            if (backgroundMusic == null)
            {
                Debug.LogError("No AudioSource component found on this object.");
                return;
            }
        }

        // 초기 설정값 로드 및 적용
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        musicVolume = PlayerPrefs.GetFloat("BGMVolume", 0.3f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.6f);

        ApplyVolumes();

        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.volume = musicVolume * masterVolume;
            backgroundMusic.Play();
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        SaveVolumeSettings();
        ApplyVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        SaveVolumeSettings();
        ApplyVolumes();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        SaveVolumeSettings();
        ApplyVolumes();
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", musicVolume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.Save();  // 애플리케이션 종료 시 데이터 영구 저장
    }

    private void ApplyVolumes()
    {
        // 각 오디오 소스에 볼륨 반영
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = musicVolume * masterVolume;
        }

        if (tenseBGM != null)
        {
            tenseBGM.volume = musicVolume * masterVolume;
        }

        foreach (var sfxSource in sfxSources)
        {
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume * masterVolume;
            }
        }
    }

    // 긴장감 있는 BGM으로 전환하는 함수
    public void SwitchToTenseBGM()
    {
        StartCoroutine(FadeOut(backgroundMusic, 1f));
        StartCoroutine(FadeIn(tenseBGM, musicVolume * masterVolume, 1f));
    }

    // 일반 배경음악으로 돌아가는 함수
    public void SwitchToNormalBGM()
    {
        StartCoroutine(FadeOut(tenseBGM, 1f));
        StartCoroutine(FadeIn(backgroundMusic, musicVolume * masterVolume, 1f));
    }

    // 페이드 아웃 함수
    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    // 페이드 인 함수
    private IEnumerator FadeIn(AudioSource audioSource, float targetVolume, float duration)
    {
        audioSource.Play();
        audioSource.volume = 0f;

        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += targetVolume * Time.deltaTime / duration;
            yield return null;
        }
    }
}
