using UnityEngine;

namespace DreamOfRedMansion.Data
{
    [CreateAssetMenu(fileName = "ResultMapping", menuName = "DreamOfRedMansion/Result Mapping")]
    public class ResultMapping : ScriptableObject
    {
        [Tooltip("�Ҧ����ײզX�P���G��������")]
        public ResultEntrySO[] results;

        /// <summary>
        /// �ھڵ��צr��M��������G
        /// </summary>
        public ResultEntrySO GetResult(string pattern)
        {
            foreach (var entry in results)
            {
                if (entry != null && entry.answerPattern == pattern)
                    return entry;
            }

            Debug.LogWarning($"[ResultMapping] �����������G�G{pattern}");
            return null;
        }
    }
}
