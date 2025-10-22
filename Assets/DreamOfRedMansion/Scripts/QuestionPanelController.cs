using UnityEngine;
using UnityEngine.UI;
using DreamOfRedMansion.Data;
using TMPro;

namespace DreamOfRedMansion
{
    /// <summary>
    /// 負責控制題目畫面上所有文字顯示。
    /// 由 QuestionManager 呼叫以更新畫面內容。
    /// </summary>
    public class QuestionPanelController : MonoBehaviour
    {
        [Header("UI Text 元件")]
        [Tooltip("顯示情境描述的文字區塊")]
        public Text contextText;

        [Tooltip("顯示題目本身的文字區塊")]
        public Text questionText;

        [Tooltip("顯示肯定答案的文字區塊 (是 / 圈)")]
        public TMP_Text positiveAnswerText;
        private string positiveAnswerTemp;

        [Tooltip("顯示否定答案的文字區塊 (否 / 叉)")]
        public TMP_Text negativeAnswerText;
        private string negativeAnswerTemp;

        [Header("動畫或特效（可選）")]
        [Tooltip("是否啟用文字淡入效果")]
        public bool useFadeIn = false;

        [Header("UI 容器 (可選)")]
        [Tooltip("控制淡入的目標 UI Panel (需有 CanvasGroup)")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("確認選擇用的紅色圈圈")]
        [SerializeField] private GameObject selectCircle_positive;
        [SerializeField] private GameObject selectCircle_negative;
        private void Awake()
        {

        }

        /// <summary>
        /// 顯示指定題目內容。
        /// </summary>
        public void ShowQuestion(QuestionData question)
        {
            Clear();
            if (question == null)
            {
                Debug.LogWarning("[QuestionPanelController] 傳入的 QuestionData 為空！");
                return;
            }

            if (!question.isCutcene)
            {
                if (questionText != null)
                    questionText.text = question.questionText;

                if (positiveAnswerText != null)
                {
                    //positiveAnswerText.text = $"{question.optionCircle}：{question.circleDescription}";
                    positiveAnswerText.text = $"{question.circleDescription}";
                    positiveAnswerTemp = $"{question.circleDescription}";
                }

                if (negativeAnswerText != null)
                {
                    //negativeAnswerText.text = $"{question.optionCross}：{question.crossDescription}";
                    negativeAnswerText.text = $"{question.crossDescription}";
                    negativeAnswerTemp = $"{question.crossDescription}";
                }
            }
            else
            {
                if (contextText != null)
                    contextText.text = question.contextText;
                // 明確清空文字與暫存，避免被重設
                if (positiveAnswerText != null) positiveAnswerText.text = "";
                if (negativeAnswerText != null) negativeAnswerText.text = "";
                positiveAnswerTemp = "";
                negativeAnswerTemp = "";
            }
            //if (useFadeIn)
            //    StartCoroutine(FadeIn());
        }

        /// <summary>
        /// 清除文字內容（例如切換狀態時）
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
                t += Time.deltaTime * 2f; // 淡入速度
                canvasGroup.alpha = Mathf.Lerp(0, 1, t);
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
        public System.Collections.IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("[QuestionPanelController] 未指定 fadeTarget，淡出略過。");
                yield break;
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f; // 淡出速度，可調
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
