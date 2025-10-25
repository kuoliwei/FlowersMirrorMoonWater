using UnityEngine;

[CreateAssetMenu(fileName = "SkeletonSetData", menuName = "Interactive/Skeleton Set Data")]
public class SkeletonSetData : ScriptableObject
{
    [Header("識別名稱（要與場景中 SkeletonGroup 的 key 對應）")]
    public string key;

    [Header("背景與遮罩共用的 Sprite")]
    public Sprite bgSprite;

    [Header("顯示時間（秒）")]
    public float displayDuration = 5f;
}
