using UnityEngine;

public class BtnSoundManager : MonoBehaviour
{
    public static BtnSoundManager Instance { get; private set; }
    public AudioSource audioSource;  // 사운드를 재생할 AudioSource
    public AudioClip buttonClickSound;  // 버튼 클릭 사운드

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}
