using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DreamOfRedMansion.Data;

namespace DreamOfRedMansion
{
    public class ResultCalculator : MonoBehaviour
    {
        [Header("���G������")]
        public ResultMapping resultMapping;

        [Header("UI ���")]
        public ScreenUIController screenUI;

        [Tooltip("���G��ܬ��")]
        public float displayDuration = 20f;
        public Text countdown;

        public IEnumerator RunResultPhase(string answerPattern, System.Action onComplete)
        {
            if (string.IsNullOrEmpty(answerPattern))
            {
                Debug.LogWarning("[ResultCalculator] ���ײզX���šI");
                //onComplete?.Invoke();
                //yield break;
            }

            Debug.Log($"[ResultCalculator] ���쵪�ײզX�G{answerPattern}");

            var result = resultMapping.GetResult(answerPattern);
            if (result == null)
            {
                Debug.LogWarning($"[ResultCalculator] �����������G�G{answerPattern}");
                //onComplete?.Invoke();
                //yield break;
            }
            else
            {
                if (screenUI != null)
                    screenUI.UpdateResultContent(result.characterName, result.title, result.Introduction, result.description, result.resultImage);

                Debug.Log($"[ResultCalculator] ��ܨ���G{result.characterName}");
                float timer = displayDuration;
                if (countdown != null)
                    countdown.gameObject.SetActive(true);

                while (timer > 0f)
                {
                    if (countdown != null)
                        countdown.text = Mathf.CeilToInt(timer).ToString();
                    yield return null;
                    timer -= Time.deltaTime;
                }

                if (countdown != null)
                {
                    countdown.text = "0";
                    countdown.gameObject.SetActive(false);
                }
            }
            yield return new WaitForSeconds(0.1f);
            onComplete?.Invoke();
        }
    }
}
