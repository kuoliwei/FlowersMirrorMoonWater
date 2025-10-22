using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;

    [Header("Question BGM Clips")]
    public AudioClip questionIntroBGM; // �Ĥ@���]�u���@���^
    public AudioClip questionLoopBGM;  // �ĤG���]�L���`���^

    [Header("Settings")]
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private Coroutine questionRoutine;

    public bool debugLog = false;
    /// <summary>
    /// ����ݵ��D�I�����֡]�� intro �A loop�^
    /// </summary>
    public void PlayQuestionBGM()
    {
        if (debugLog) Debug.Log($"PlayQuestionBGM invoke");
        //�Y���W���ݯd����{�N����
        if (questionRoutine != null)
            StopCoroutine(questionRoutine);
        questionRoutine = StartCoroutine(PlayQuestionSequence());

        //bgmSource.Play();
    }

    /// <summary>
    /// ����ثe�Ҧ� BGM
    /// </summary>
    public void StopBGM()
    {
        if (debugLog) Debug.Log($"StopBGM invoke");
        //if (questionRoutine != null)
        //{
        //    StopCoroutine(questionRoutine);
        //    questionRoutine = null;
        //}

        //if (fadeCoroutine != null)
        //    StopCoroutine(fadeCoroutine);
        //fadeCoroutine = StartCoroutine(FadeOut());

        bgmSource.Stop();
    }

    private IEnumerator PlayQuestionSequence()
    {
        // ����Ĥ@���]Intro�^
        yield return StartCoroutine(FadeIn(questionIntroBGM));

        // ���ݲĤ@������
        //yield return new WaitForSeconds(questionIntroBGM.length);
        yield return new WaitUntil(() => !bgmSource.isPlaying);
        if (debugLog) Debug.Log($"�Ĥ@�����񧹲��A���\loop:{bgmSource.loop}");

        // ���ۼ���ĤG���]Loop�^
        bgmSource.clip = questionLoopBGM;
        bgmSource.loop = true;
        bgmSource.Play();
        yield return new WaitUntil(() => bgmSource.isPlaying);
        if (debugLog) Debug.Log($"�}�l����ĤG���A���\loop:{bgmSource.loop}");
    }
    private IEnumerator FadeIn(AudioClip clip)
    {
        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = false;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        bgmSource.volume = 1f;
        yield return new WaitUntil(() => bgmSource.isPlaying);
        if (debugLog) Debug.Log($"�}�l����Ĥ@���A���\loop:{bgmSource.loop}");
    }

    private IEnumerator FadeOut()
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }
}
