using UnityEngine;

namespace DreamOfRedMansion.Data
{
    [CreateAssetMenu(fileName = "ResultEntry", menuName = "DreamOfRedMansion/Result Entry")]
    public class ResultEntrySO : ScriptableObject
    {
        [Header("��������")]
        [Tooltip("���ײզX�A�Ҧp 1100�]�O �O �_ �_�^")]
        public string answerPattern;

        [Header("�����T")]
        [Tooltip("����W��")]
        public string characterName;

        [Tooltip("�ٸ�")]
        [TextArea(1, 2)]
        public string title;

        [Tooltip("²��")]
        [TextArea(1, 2)]
        public string Introduction;

        [Tooltip("�y�z")]
        [TextArea(2, 5)]
        public string description;

        [Tooltip("����Ϥ��]�i��^")]
        public Sprite resultImage;
    }
}
