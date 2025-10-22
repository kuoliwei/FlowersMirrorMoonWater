using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

namespace DreamOfRedMansion
{
    /// <summary>
    /// ���� Idle ���A�U���e���]�v�� �� �R�A�ʭ��^�P�H�J�H�X�C
    /// </summary>
    public class IdlePanelController : MonoBehaviour
    {
        [Header("�ե�Ѧ�")]
        [SerializeField] private RawImage rawImage;               // �v�������
        [SerializeField] private CanvasGroup imageGroup;          // �}���ʭ��s�ա]�t�l����^
        [SerializeField] private IdleVideoPlayer idleVideoPlayer; // �t�d��ڼ���v��
        [SerializeField] private HandRaiseDetector handRaiseDetector; // ����̤��|������
        [SerializeField] private AudioController audioController;

        [Header("�H�J�H�X�]�w")]
        public float fadeDuration = 1f;

        private bool _isPlayingVideo = false;

        private void Awake()
        {
            if (idleVideoPlayer != null) { }
                idleVideoPlayer.OnVideoFinished += HandleVideoFinished;
        }

        private void OnDestroy()
        {
            if (idleVideoPlayer != null)
                idleVideoPlayer.OnVideoFinished -= HandleVideoFinished;
        }

        /// <summary>
        /// Idle ���A�}�l�ɩI�s�C
        /// </summary>
        public void OnIdleEnter()
        {
            StopAllCoroutines();
            audioController.StopBGM();
            // ���������v������
            if (handRaiseDetector != null)
            {
                handRaiseDetector.externalLock = true;
                handRaiseDetector.enabled = true; // �O���ҥΪ��A�]���� GameFlow �A�����^
            }

            // ��l�Ƶe��
            if (imageGroup != null)
            {
                imageGroup.gameObject.SetActive(false);
                imageGroup.alpha = 0f;
            }

            if (rawImage != null)
            {
                rawImage.gameObject.SetActive(true);
                SetRawImageAlpha(1f);
            }

            // �}�l����v���]���i���_�^
            _isPlayingVideo = true;
            idleVideoPlayer?.PlayOnce();
        }

        /// <summary>
        /// ��v�����񵲧���Ĳ�o�C
        /// </summary>
        private void HandleVideoFinished()
        {
            Debug.Log("HandleVideoFinished invoke");
            _isPlayingVideo = false;
            audioController.PlayQuestionBGM();
            StartCoroutine(FadeOutRawImageThenShowImage());
        }

        private IEnumerator FadeOutRawImageThenShowImage()
        {
            // RawImage �H�X
            if (rawImage != null)
            {
                float time = 0f;
                Color color = rawImage.color;
                float startAlpha = color.a;

                while (time < fadeDuration)
                {
                    time += Time.deltaTime;
                    color.a = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
                    rawImage.color = color;
                    yield return null;
                }

                color.a = 0f;
                rawImage.color = color;
                rawImage.gameObject.SetActive(false);
            }

            // ImageGroup �H�J
            if (imageGroup != null)
            {
                imageGroup.gameObject.SetActive(true);
                yield return StartCoroutine(FadeCanvasGroup(imageGroup, 0f, 1f, fadeDuration));
            }

            // �ҥΤ��|����
            if (handRaiseDetector != null)
                handRaiseDetector.externalLock = false;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
        {
            float t = 0f;
            group.alpha = from;
            while (t < duration)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            group.alpha = to;
        }

        private void SetRawImageAlpha(float a)
        {
            if (rawImage == null) return;
            Color c = rawImage.color;
            c.a = a;
            rawImage.color = c;
        }
    }
}
