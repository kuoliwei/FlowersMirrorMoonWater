using UnityEngine;

namespace DreamOfRedMansion.Data
{
    [CreateAssetMenu(fileName = "QuestionData", menuName = "DreamOfRedMansion/Question Data")]
    public class QuestionData : ScriptableObject
    {
        [Header("���D")]
        [Tooltip("�D�ت����D")]
        //[TextArea(2, 3)]
        public string questionTitle;

        [Header("���Ҵy�z")]
        [Tooltip("�D�ثe�����Ҥ�r")]
        [TextArea(3, 5)]
        public string contextText;

        [Header("�D�س]�w")]
        [Tooltip("�D�ؤ�r���e")]
        [TextArea(2, 3)]
        public string questionText;

        [Tooltip("��ﶵ��r�]�Ҧp �O / O�^")]
        public string optionCircle = "�O";

        [Tooltip("�e�ﶵ��r�]�Ҧp �_ / X�^")]
        public string optionCross = "�_";

        [Header("�ﶵ�Բӻ���")]
        [TextArea(2, 3)]
        [Tooltip("�^���u�O�v�ɪ��y�z��r")]
        public string circleDescription;

        [TextArea(2, 3)]
        [Tooltip("�^���u�_�v�ɪ��y�z��r")]
        public string crossDescription;

        [Header("����[�v�]�w")]
        [Tooltip("�^����ɡA��C�Ө��⪺���ƥ[�v")]
        public float[] circleWeights;

        [Tooltip("�^���e�ɡA��C�Ө��⪺���ƥ[�v")]
        public float[] crossWeights;

        [Header("�O�_�L��")]
        [Tooltip("�O�_���L������")]
        public bool isCutcene = false;

        [Header("�O���P�_")]
        [Tooltip("���׬O�_�n�Q�O��")]
        public bool isRecord = false;

        [Header("��ܳ]�w")]
        [Tooltip("�O�_�ҥΦ��D��")]
        public bool enabled = true;
    }
}
