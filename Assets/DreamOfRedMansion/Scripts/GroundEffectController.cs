using DreamOfRedMansion.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace DreamOfRedMansion
{
    /// <summary>
    /// 控制地板特效顯示，目前以更換貼圖或顏色為主。
    /// 後續可擴充成 Shader / VFXGraph。
    /// </summary>
    public class GroundEffectController : MonoBehaviour
    {
        [Header("地板 Mesh 或 Renderer")]
        public Renderer groundRenderer;

        [Header("不同階段使用的顏色")]
        public Color idleColor = Color.black;
        public Color questionColor = Color.blue;
        public Color resultColor = Color.red;

        [Header("地板 是 或 否 狀態切換用物件")]
        public GameObject circle_black_bg;
        public GameObject cross_black_bg;
        public GameObject BlackGround;
        //public GameObject selectCircle_positive;
        //public GameObject selectCircle_negative;

        [Tooltip("是否輸出除錯訊息")]
        public bool debugLog = true;

        private bool _isActive = false;
        /// <summary>
        /// 由外部（例如 GameFlowController）呼叫，用於同步遊戲狀態。
        /// </summary>
        public void OnGameStateChanged(GameState newState)
        {
            _isActive = (newState == GameState.Question);
            if (!_isActive)
            {
                if(newState == GameState.Idle)
                {
                    BlackGround.SetActive(true);
                }
                circle_black_bg.SetActive(false);
                cross_black_bg.SetActive(false);
                //selectCircle_positive.SetActive(false);
                //selectCircle_negative.SetActive(false);
            }
            else
            {
                BlackGround.SetActive(false);
            }

            if (debugLog)
                Debug.Log($"[GroundEffectController] 狀態切換 → {_isActive}");
        }
        public void SetIdle()
        {
            SetGroundColor(idleColor);
            Debug.Log("[GroundEffect] 切換到 Idle 狀態");
        }

        public void SetQuestion()
        {
            SetGroundColor(questionColor);
            Debug.Log("[GroundEffect] 顯示題目互動階段特效");
        }

        public void SetResult()
        {
            SetGroundColor(resultColor);
            Debug.Log("[GroundEffect] 顯示結果階段特效");
        }

        private void SetGroundColor(Color color)
        {
            if (groundRenderer != null && groundRenderer.material != null)
                groundRenderer.material.color = color;
        }

        public void selectCircle()
        {
            if (_isActive)
            {
                circle_black_bg.SetActive(true);
                cross_black_bg.SetActive(false);
                //selectCircle_positive.SetActive(true);
                //selectCircle_negative.SetActive(false);
            }
        }
        public void selectCross()
        {
            if (_isActive)
            {
                circle_black_bg.SetActive(false);
                cross_black_bg.SetActive(true);
                //selectCircle_positive.SetActive(false);
                //selectCircle_negative.SetActive(true);
            }
        }
        public void SetAnswer(bool answer)
        {
            if (answer)
            {
                if (_isActive)
                {
                    circle_black_bg.SetActive(true);
                    cross_black_bg.SetActive(false);
                    //selectCircle_positive.SetActive(true);
                    //selectCircle_negative.SetActive(false);
                }
            }
            else
            {
                if (_isActive)
                {
                    circle_black_bg.SetActive(false);
                    cross_black_bg.SetActive(true);
                    //selectCircle_positive.SetActive(false);
                    //selectCircle_negative.SetActive(true);
                }
            }
        }
        public void SetAllActiveFalse()
        {
            circle_black_bg.SetActive(false);
            cross_black_bg.SetActive(false);
            //selectCircle_positive.SetActive(false);
            //selectCircle_negative.SetActive(false);
        }
    }
}
