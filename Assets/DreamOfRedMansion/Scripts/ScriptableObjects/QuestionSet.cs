using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DreamOfRedMansion.Data
{
    [CreateAssetMenu(fileName = "QuestionSet", menuName = "DreamOfRedMansion/Question Set")]
    public class QuestionSet : ScriptableObject
    {
        [Tooltip("┮Τノ肈ヘ ScriptableObject")]
        public List<QuestionData> questions = new List<QuestionData>();

        /// <summary>
        /// 繦诀┾﹚计秖肈ヘぃ狡
        /// </summary>
        public List<QuestionData> GetRandomQuestions(int count)
        {
            var available = questions
                .Where(q => q != null && q.enabled)
                .ToList();

            if (available.Count == 0)
            {
                Debug.LogWarning("[QuestionSet] 肈畐┪场窽ノ");
                return new List<QuestionData>();
            }

            if (available.Count <= count)
                return new List<QuestionData>(available);

            var selected = new List<QuestionData>();
            while (selected.Count < count)
            {
                var q = available[Random.Range(0, available.Count)];
                if (!selected.Contains(q))
                    selected.Add(q);
            }

            return selected;
        }

        /// <summary>
        /// ㄌ﹚计秖肈ヘ眖肈畐玡よ秨﹍
        /// </summary>
        public List<QuestionData> GetSequentialQuestions(int count)
        {
            var available = questions
                .Where(q => q != null && q.enabled)
                .ToList();

            if (available.Count == 0)
            {
                Debug.LogWarning("[QuestionSet] 肈畐┪场窽ノ");
                return new List<QuestionData>();
            }

            // 狦肈ヘぃì钡
            if (available.Count <= count)
                return new List<QuestionData>(available);

            return available.Take(count).ToList();
        }
    }
}
