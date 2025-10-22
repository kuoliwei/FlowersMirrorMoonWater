using UnityEngine;

namespace DreamOfRedMansion.Data
{
    [CreateAssetMenu(fileName = "ResultMapping", menuName = "DreamOfRedMansion/Result Mapping")]
    public class ResultMapping : ScriptableObject
    {
        [Tooltip("所有答案組合與結果的對應表")]
        public ResultEntrySO[] results;

        /// <summary>
        /// 根據答案字串尋找對應結果
        /// </summary>
        public ResultEntrySO GetResult(string pattern)
        {
            foreach (var entry in results)
            {
                if (entry != null && entry.answerPattern == pattern)
                    return entry;
            }

            Debug.LogWarning($"[ResultMapping] 未找到對應結果：{pattern}");
            return null;
        }
    }
}
