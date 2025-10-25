using UnityEngine;

[CreateAssetMenu(fileName = "SkeletonSetData", menuName = "Interactive/Skeleton Set Data")]
public class SkeletonSetData : ScriptableObject
{
    [Header("�ѧO�W�١]�n�P������ SkeletonGroup �� key �����^")]
    public string key;

    [Header("�I���P�B�n�@�Ϊ� Sprite")]
    public Sprite bgSprite;

    [Header("��ܮɶ��]��^")]
    public float displayDuration = 5f;
}
