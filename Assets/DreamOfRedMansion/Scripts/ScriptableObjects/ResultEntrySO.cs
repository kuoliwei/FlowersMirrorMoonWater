using UnityEngine;

namespace DreamOfRedMansion.Data
{
    [CreateAssetMenu(fileName = "ResultEntry", menuName = "DreamOfRedMansion/Result Entry")]
    public class ResultEntrySO : ScriptableObject
    {
        [Header("對應條件")]
        [Tooltip("答案組合，例如 1100（是 是 否 否）")]
        public string answerPattern;

        [Header("角色資訊")]
        [Tooltip("角色名稱")]
        public string characterName;

        [Tooltip("稱號")]
        [TextArea(1, 2)]
        public string title;

        [Tooltip("簡介")]
        [TextArea(1, 2)]
        public string Introduction;

        [Tooltip("描述")]
        [TextArea(2, 5)]
        public string description;

        [Tooltip("角色圖片（可選）")]
        public Sprite resultImage;
    }
}
