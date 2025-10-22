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
        [Header("�D�w")]
        public QuestionSet questionSet;

        [Header("UI ���")]
        public QuestionPanelController questionPanelController;
        public GroundEffectController groundEffectController;

        [Header("�C�D���D�ɶ�")]
        public float questionDuration = 10f;
        public Text countdown;

        // �令�r���x�s���ס]�Ҧp "1010"�^
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
                Debug.LogError("[QuestionManager] �����w�D�w�I");
                yield break;
            }

            _questions = questionSet.questions; // �T�w�|�D�̧�
            collectedAnswers = "";

            Debug.Log($"[QuestionManager] �Ұ��D�ض��q�A�@ {_questions.Count} �D�C");
            for (int i = 0; i < _questions.Count; i++)
            {
                yield return StartCoroutine(AskQuestion(_questions[i]));
            }

            Debug.Log($"[QuestionManager] �Ҧ��D�ا��� �� ���ײզX�G{collectedAnswers}");
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

                // �˼ƭp�����
                while (timer > 0f)
                {
                    if (countdown != null)
                        countdown.text = Mathf.CeilToInt(timer).ToString(); // ��ܾ��

                    yield return null; // �C�V����
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
                    Debug.Log($"[QuestionManager] �D�ءu{question.questionTitle}�v���סG{(answer ? "�O" : "�_")} (�w�O��)");
                }
                else
                {
                    Debug.Log($"[QuestionManager] �D�ءu{question.questionTitle}�v���סG{(answer ? "�O" : "�_")} (���O��)");
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
                Debug.Log($"[QuestionManager] ��ܹL���G�u{question.questionTitle}�v");
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
            //Debug.Log($"answer�G{answer}");
        }
        private void Update()
        {

        }
    }
}
