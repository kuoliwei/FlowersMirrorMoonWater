using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;

    [Header("Question BGM Clips")]
    public AudioClip questionIntroBGM; // 第一首（只播一次）
    public AudioClip questionLoopBGM;  // 第二首（無限循環）

    [Header("Settings")]
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private Coroutine questionRoutine;

    public bool debugLog = false;
    /// <summary>
    /// 播放問答題背景音樂（先 intro 再 loop）
    /// </summary>
    public void PlayQuestionBGM()
    {
        if (debugLog) Debug.Log($"PlayQuestionBGM invoke");
        //若有上次殘留的協程就停止
        if (questionRoutine != null)
            StopCoroutine(questionRoutine);
        questionRoutine = StartCoroutine(PlayQuestionSequence());

        //bgmSource.Play();
    }

    /// <summary>
    /// 停止目前所有 BGM
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
        // 播放第一首（Intro）
        yield return StartCoroutine(FadeIn(questionIntroBGM));

        // 等待第一首播完
        //yield return new WaitForSeconds(questionIntroBGM.length);
        yield return new WaitUntil(() => !bgmSource.isPlaying);
        if (debugLog) Debug.Log($"第一首播放完畢，允許loop:{bgmSource.loop}");

        // 接著播放第二首（Loop）
        bgmSource.clip = questionLoopBGM;
        bgmSource.loop = true;
        bgmSource.Play();
        yield return new WaitUntil(() => bgmSource.isPlaying);
        if (debugLog) Debug.Log($"開始播放第二首，允許loop:{bgmSource.loop}");
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
        if (debugLog) Debug.Log($"開始播放第一首，允許loop:{bgmSource.loop}");
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
