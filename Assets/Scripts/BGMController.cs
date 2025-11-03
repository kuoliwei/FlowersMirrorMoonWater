using UnityEngine;

public class BGMController : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip interactiveBGM;

    private void Awake()
    {
        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
    }

    public void PlayInteractiveBGM()
    {
        if (bgmSource == null || interactiveBGM == null) return;
        if (!bgmSource.isPlaying)
        {
            bgmSource.clip = interactiveBGM;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();
    }
}
