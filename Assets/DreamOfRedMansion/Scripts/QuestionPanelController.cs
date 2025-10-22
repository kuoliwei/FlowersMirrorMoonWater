using UnityEngine;
using UnityEngine.UI;
using DreamOfRedMansion.Data;
using TMPro;

namespace DreamOfRedMansion
{
    /// <summary>
    /// �t�d�����D�صe���W�Ҧ���r��ܡC
    /// �� QuestionManager �I�s�H��s�e�����e�C
    /// </summary>
    public class QuestionPanelController : MonoBehaviour
    {
        [Header("UI Text ����")]
        [Tooltip("��ܱ��Ҵy�z����r�϶�")]
        public Text contextText;

        [Tooltip("����D�إ�������r�϶�")]
        public Text questionText;

        [Tooltip("��ܪ֩w���ת���r�϶� (�O / ��)")]
        public TMP_Text positiveAnswerText;
        private string positiveAnswerTemp;

        [Tooltip("��ܧ_�w���ת���r�϶� (�_ / �e)")]
        public TMP_Text negativeAnswerText;
        private string negativeAnswerTemp;

        [Header("�ʵe�ίS�ġ]�i��^")]
        [Tooltip("�O�_�ҥΤ�r�H�J�ĪG")]
        public bool useFadeIn = false;

        [Header("UI �e�� (�i��)")]
        [Tooltip("����H�J���ؼ� UI Panel (�ݦ� CanvasGroup)")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("�T�{��ܥΪ�������")]
        [SerializeField] private GameObject selectCircle_positive;
        [SerializeField] private GameObject selectCircle_negative;
        private void Awake()
        {

        }

        /// <summary>
        /// ��ܫ��w�D�ؤ��e�C
        /// </summary>
        public void ShowQuestion(QuestionData question)
        {
            Clear();
            if (question == null)
            {
                Debug.LogWarning("[QuestionPanelController] �ǤJ�� QuestionData ���šI");
                return;
            }

            if (!question.isCutcene)
            {
                if (questionText != null)
                    questionText.text = question.questionText;

                if (positiveAnswerText != null)
                {
                    //positiveAnswerText.text = $"{question.optionCircle}�G{question.circleDescription}";
                    positiveAnswerText.text = $"{question.circleDescription}";
                    positiveAnswerTemp = $"{question.circleDescription}";
                }

                if (negativeAnswerText != null)
                {
                    //negativeAnswerText.text = $"{question.optionCross}�G{question.crossDescription}";
                    negativeAnswerText.text = $"{question.crossDescription}";
                    negativeAnswerTemp = $"{question.crossDescription}";
                }
            }
            else
            {
                if (contextText != null)
                    contextText.text = question.contextText;
                // ���T�M�Ť�r�P�Ȧs�A�קK�Q���]
                if (positiveAnswerText != null) positiveAnswerText.text = "";
                if (negativeAnswerText != null) negativeAnswerText.text = "";
                positiveAnswerTemp = "";
                negativeAnswerTemp = "";
            }
            //if (useFadeIn)
            //    StartCoroutine(FadeIn());
        }

        /// <summary>
        /// �M����r���e�]�Ҧp�������A�ɡ^
        /// </summary>
        public void Clear()
        {
            if (contextText != null) contextText.text = "";
            if (questionText != null) questionText.text = "";
            if (positiveAnswerText != null) positiveAnswerText.text = "";
            if (negativeAnswerText != null) negativeAnswerText.text = "";
        }

        public System.Collections.IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f; // �H�J�t��
                canvasGroup.alpha = Mathf.Lerp(0, 1, t);
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
        public System.Collections.IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("[QuestionPanelController] �����w fadeTarget�A�H�X���L�C");
                yield break;
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f; // �H�X�t�סA�i��
                canvasGroup.alpha = Mathf.Lerp(1, 0, t);
                yield return null;
            }
            canvasGroup.alpha = 0;
        }
        public void ShowSelectCircle(bool answer)
        {
            if (answer)
            {
                selectCircle_positive.SetActive(true);
                selectCircle_negative.SetActive(false);
            }
            else
            {
                selectCircle_positive.SetActive(false);
                selectCircle_negative.SetActive(true);
            }
        }
        public void HideSelectCircle()
        {
            selectCircle_positive.SetActive(false);
            selectCircle_negative.SetActive(false);
        }
        public void SetAnswer(bool answer)
        {
            if (answer)
            {
                positiveAnswerText.rectTransform.localScale = new Vector3(1.3f, 1.3f, 1);
                negativeAnswerText.rectTransform.localScale = new Vector3(1, 1, 1);

                positiveAnswerText.text = AddBoldAndUnderline(positiveAnswerTemp);
                negativeAnswerText.text = negativeAnswerTemp;
            }
            else
            {
                positiveAnswerText.rectTransform.localScale = new Vector3(1, 1, 1);
                negativeAnswerText.rectTransform.localScale = new Vector3(1.3f, 1.3f, 1);
                positiveAnswerText.text = positiveAnswerTemp;
                negativeAnswerText.text = AddBoldAndUnderline(negativeAnswerTemp);
            }
        }
        public void NonSelect()
        {
            positiveAnswerText.rectTransform.localScale = new Vector3(1, 1, 1);
            negativeAnswerText.rectTransform.localScale = new Vector3(1, 1, 1);
            positiveAnswerText.text = positiveAnswerTemp;
            negativeAnswerText.text = negativeAnswerTemp;
        }
        public string AddBoldAndUnderline(string content)
        {
            return $"<b><u>{content}</u></b>";
        }
    }
}
