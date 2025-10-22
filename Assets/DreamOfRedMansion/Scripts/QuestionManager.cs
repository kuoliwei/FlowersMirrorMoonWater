using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DreamOfRedMansion.Data;
using System.Collections.Generic;

namespace DreamOfRedMansion
{
    public class QuestionManager : MonoBehaviour
    {
        [Header("題庫")]
        public QuestionSet questionSet;

        [Header("UI 控制器")]
        public QuestionPanelController questionPanelController;
        public GroundEffectController groundEffectController;

        [Header("每題答題時間")]
        public float questionDuration = 10f;
        public Text countdown;

        // 改成字串儲存答案（例如 "1010"）
        [NonSerialized]
        public string collectedAnswers = "";

        private List<QuestionData> _questions;

        bool answer = false;

        bool isCurrentQuestionCutscene;

        bool isAnswerSelected = false;

        public IEnumerator RunQuestionFlow(Action onComplete)
        {
            if (questionSet == null)
            {
                Debug.LogError("[QuestionManager] 未指定題庫！");
                yield break;
            }

            _questions = questionSet.questions; // 固定四題依序
            collectedAnswers = "";

            Debug.Log($"[QuestionManager] 啟動題目階段，共 {_questions.Count} 題。");
            for (int i = 0; i < _questions.Count; i++)
            {
                yield return StartCoroutine(AskQuestion(_questions[i]));
            }

            Debug.Log($"[QuestionManager] 所有題目完成 → 答案組合：{collectedAnswers}");
            onComplete?.Invoke();
        }

        private IEnumerator AskQuestion(QuestionData question)
        {
            questionPanelController.HideSelectCircle();
            questionPanelController.ShowQuestion(question);
            yield return questionPanelController.StartCoroutine(questionPanelController.FadeIn());

            if (!question.isCutcene)
            {
                isCurrentQuestionCutscene = false;
                answer = false;
                float timer = questionDuration;
                if (countdown != null)
                    countdown.gameObject.SetActive(true);

                // 倒數計時顯示
                while (timer > 0f)
                {
                    if (countdown != null)
                        countdown.text = Mathf.CeilToInt(timer).ToString(); // 顯示整秒

                    yield return null; // 每幀遞減
                    timer -= Time.deltaTime;
                }

                if (countdown != null)
                {
                    countdown.text = "0";
                    countdown.gameObject.SetActive(false);
                }

                if (question.isRecord)
                {
                    collectedAnswers += answer ? "1" : "0";
                    Debug.Log($"[QuestionManager] 題目「{question.questionTitle}」答案：{(answer ? "是" : "否")} (已記錄)");
                }
                else
                {
                    Debug.Log($"[QuestionManager] 題目「{question.questionTitle}」答案：{(answer ? "是" : "否")} (未記錄)");
                }
                questionPanelController.ShowSelectCircle(answer);
                isAnswerSelected = true;
                yield return new WaitForSeconds(2);
                isAnswerSelected = false;
            }
            else
            {
                isCurrentQuestionCutscene = true;
                if (countdown != null)
                    countdown.gameObject.SetActive(false);
                Debug.Log($"[QuestionManager] 顯示過場：「{question.questionTitle}」");
                yield return new WaitForSeconds(questionDuration);
            }
            yield return questionPanelController.StartCoroutine(questionPanelController.FadeOut());
        }
        public void SetAnswer(bool value)
        {
            if (!isCurrentQuestionCutscene)
            {
                if (!isAnswerSelected)
                {
                    answer = value;
                    groundEffectController.SetAnswer(answer);
                    questionPanelController.SetAnswer(answer);
                }
            }
            else
            {
                groundEffectController.SetAllActiveFalse();
                questionPanelController.NonSelect();
            }
            //Debug.Log($"answer：{answer}");
        }
        private void Update()
        {

        }
    }
}
