using DreamOfRedMansion.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace DreamOfRedMansion
{
    /// <summary>
    /// ����a�O�S����ܡA�ثe�H�󴫶K�ϩ��C�⬰�D�C
    /// ����i�X�R�� Shader / VFXGraph�C
    /// </summary>
    public class GroundEffectController : MonoBehaviour
    {
        [Header("�a�O Mesh �� Renderer")]
        public Renderer groundRenderer;

        [Header("���P���q�ϥΪ��C��")]
        public Color idleColor = Color.black;
        public Color questionColor = Color.blue;
        public Color resultColor = Color.red;

        [Header("�a�O �O �� �_ ���A�����Ϊ���")]
        public GameObject circle_black_bg;
        public GameObject cross_black_bg;
        public GameObject BlackGround;
        //public GameObject selectCircle_positive;
        //public GameObject selectCircle_negative;

        [Tooltip("�O�_��X�����T��")]
        public bool debugLog = true;

        private bool _isActive = false;
        /// <summary>
        /// �ѥ~���]�Ҧp GameFlowController�^�I�s�A�Ω�P�B�C�����A�C
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
                Debug.Log($"[GroundEffectController] ���A���� �� {_isActive}");
        }
        public void SetIdle()
        {
            SetGroundColor(idleColor);
            Debug.Log("[GroundEffect] ������ Idle ���A");
        }

        public void SetQuestion()
        {
            SetGroundColor(questionColor);
            Debug.Log("[GroundEffect] ����D�ؤ��ʶ��q�S��");
        }

        public void SetResult()
        {
            SetGroundColor(resultColor);
            Debug.Log("[GroundEffect] ��ܵ��G���q�S��");
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
