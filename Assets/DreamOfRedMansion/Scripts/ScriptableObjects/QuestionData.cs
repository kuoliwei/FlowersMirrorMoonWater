using UnityEngine;

namespace DreamOfRedMansion.Data
{
    [CreateAssetMenu(fileName = "QuestionData", menuName = "DreamOfRedMansion/Question Data")]
    public class QuestionData : ScriptableObject
    {
        [Header("標題")]
        [Tooltip("題目的標題")]
        //[TextArea(2, 3)]
        public string questionTitle;

        [Header("情境描述")]
        [Tooltip("題目前的情境文字")]
        [TextArea(3, 5)]
        public string contextText;

        [Header("題目設定")]
        [Tooltip("題目文字內容")]
        [TextArea(2, 3)]
        public string questionText;

        [Tooltip("圈選項文字（例如 是 / O）")]
        public string optionCircle = "是";

        [Tooltip("叉選項文字（例如 否 / X）")]
        public string optionCross = "否";

        [Header("選項詳細說明")]
        [TextArea(2, 3)]
        [Tooltip("回答「是」時的描述文字")]
        public string circleDescription;

        [TextArea(2, 3)]
        [Tooltip("回答「否」時的描述文字")]
        public string crossDescription;

        [Header("角色加權設定")]
        [Tooltip("回答圈時，對每個角色的分數加權")]
        public float[] circleWeights;

        [Tooltip("回答叉時，對每個角色的分數加權")]
        public float[] crossWeights;

        [Header("是否過場")]
        [Tooltip("是否為過場情境")]
        public bool isCutcene = false;

        [Header("記錄與否")]
        [Tooltip("答案是否要被記錄")]
        public bool isRecord = false;

        [Header("顯示設定")]
        [Tooltip("是否啟用此題目")]
        public bool enabled = true;
    }
}
