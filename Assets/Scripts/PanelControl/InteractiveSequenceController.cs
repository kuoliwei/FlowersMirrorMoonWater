using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorWater
{
    public enum GameState
    {
        Idle,
        Question,
        Result,
        Interactive,
        VideoPlaying
    }

    [System.Serializable]
    public class SkeletonGroup
    {
        public string key;
        public CanvasGroup canvasGroup;
    }

    public class InteractiveSequenceController : MonoBehaviour
    {
        [Header("背景（Image）")]
        [SerializeField] private Image bgImage;

        [Header("遮罩（RawImage，使用 SinEdgeHole_MultiHole 材質）")]
        [SerializeField] private RawImage maskImage;

        [Header("輪播資料（ScriptableObjects）")]
        [SerializeField] private List<SkeletonSetData> sequenceDataList;

        [Header("場景中的角色群組對應")]
        [SerializeField] private List<SkeletonGroup> skeletonGroups;

        [Header("淡入淡出速度（僅用於 Mask）")]
        [SerializeField] private CanvasGroup maskCanvasGroup;
        [SerializeField] private float fadeDuration = 1f;

        private Dictionary<string, CanvasGroup> groupMap = new Dictionary<string, CanvasGroup>();
        private Coroutine sequenceRoutine;
        private Material runtimeMaskMaterial;
        private bool isPlaying = false;

        private void Awake()
        {
            foreach (var g in skeletonGroups)
            {
                if (!groupMap.ContainsKey(g.key))
                    groupMap.Add(g.key, g.canvasGroup);
            }

            foreach (var g in skeletonGroups)
                g.canvasGroup.alpha = 0f;

            if (maskImage != null && maskImage.material != null)
            {
                runtimeMaskMaterial = new Material(maskImage.material);
                maskImage.material = runtimeMaskMaterial;
            }
        }

        public void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Interactive:
                    StartSequence();
                    break;
                default:
                    StopSequence();
                    break;
            }
        }

        public void StartSequence()
        {
            if (sequenceRoutine != null)
                StopCoroutine(sequenceRoutine);

            isPlaying = true;
            sequenceRoutine = StartCoroutine(PlaySequenceLoop());
        }

        public void StopSequence()
        {
            isPlaying = false;

            if (sequenceRoutine != null)
            {
                StopCoroutine(sequenceRoutine);
                sequenceRoutine = null;
            }

            foreach (var g in skeletonGroups)
                g.canvasGroup.alpha = 0f;

            if (bgImage != null)
            {
                var c = bgImage.color;
                c.a = 0f;
                bgImage.color = c;
            }

            //if (maskImage != null)
            //{
            //    var c = maskImage.color;
            //    c.a = 0f;
            //    maskImage.color = c;
            //}

            Debug.Log("[InteractiveSequenceController] 停止輪播並清空畫面");
        }

        private IEnumerator PlaySequenceLoop()
        {
            if (sequenceDataList == null || sequenceDataList.Count == 0)
            {
                Debug.LogWarning("[InteractiveSequenceController] 沒有設定任何輪播資料");
                yield break;
            }

            Debug.Log("[InteractiveSequenceController] 開始無限循環輪播");

            while (isPlaying)
            {
                for (int i = 0; i < sequenceDataList.Count; i++)
                {
                    if (!isPlaying) yield break;

                    var data = sequenceDataList[i];
                    if (data == null)
                        continue;

                    // 換背景圖片
                    if (bgImage != null)
                        bgImage.sprite = data.bgSprite;

                    // 更新 Mask 材質
                    if (runtimeMaskMaterial != null && data.bgSprite != null)
                    {
                        Texture tex = data.bgSprite.texture;
                        runtimeMaskMaterial.SetTexture("_MainTex", tex);
                        maskImage.canvasRenderer.SetMaterial(runtimeMaskMaterial, tex);
                        maskImage.SetMaterialDirty();
                    }

                    // 取得角色群組
                    if (!groupMap.TryGetValue(data.key, out var group))
                    {
                        Debug.LogWarning($"[InteractiveSequenceController] 找不到對應的 CanvasGroup: {data.key}");
                        continue;
                    }

                    // -------------------
                    //   Fade-In
                    // -------------------
                    yield return StartCoroutine(FadeMask(1f));

                    if (bgImage != null)
                    {
                        var c = bgImage.color;
                        c.a = 1f;
                        bgImage.color = c;
                    }

                    group.alpha = 1f;

                    yield return new WaitForSeconds(data.displayDuration);

                    // -------------------
                    //   Fade-Out
                    // -------------------
                    if (bgImage != null)
                    {
                        var c = bgImage.color;
                        c.a = 0f;
                        bgImage.color = c;
                    }

                    group.alpha = 0f;

                    yield return StartCoroutine(FadeMask(0f));
                }
            }

            Debug.Log("[InteractiveSequenceController] 結束無限循環輪播");
        }

        private IEnumerator FadeMask(float targetAlpha)
        {
            if (maskCanvasGroup == null)
                yield break;

            float startAlpha = maskCanvasGroup.alpha;
            float timer = 0f;

            while (timer < fadeDuration)
            {
                float t = Mathf.Clamp01(timer / fadeDuration);
                maskCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                timer += Time.deltaTime;
                yield return null;
            }

            // 確保最後一幀精準設定
            maskCanvasGroup.alpha = targetAlpha;
        }

    }
}
