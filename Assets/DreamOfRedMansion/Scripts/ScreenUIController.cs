using UnityEngine;
using UnityEngine.UI;
using System.Collections; // �� �s�W

namespace DreamOfRedMansion
{
    /// <summary>
    /// ����k�ù�����ܤ��e�A�ھڹC�����A��ܹ����e���C
    /// </summary>
    public class ScreenUIController : MonoBehaviour
    {
        [Header("���k�ù�����")]
        public GameObject leftScreen;
        public GameObject rightScreen;

        [Header("�ù��������O")]
        public GameObject idlePanel;
        public GameObject questionPanel;
        public GameObject resultPanel;

        //public GameObject scrollPanel;

        [Header("���G�e�� UI ����")]
        public Text resultNameText;
        public Text resultTitleText;
        public Text resultIntroductionText;
        public Text resultDescriptionText;
        public Image resultImage;

        public IdlePanelController idlePanelController;
        private void Awake()
        {
            // �Ұʮɥ���� Idle �e��
            ShowIdleScreen();
        }

        public void ShowIdleScreen()
        {
            SetActivePanel(idlePanel);
            idlePanelController?.OnIdleEnter();
            Debug.Log("[ScreenUI] ��� Idle �e��");
        }

        public void ShowQuestion(string questionText = null)
        {
            SetActivePanel(questionPanel);

            // �p�G���D�ؤ�r�A�L�X�]����UI��r�e�HLog�ܷN�^
            if (!string.IsNullOrEmpty(questionText))
                Debug.Log($"[ScreenUI] ����D�ءG{questionText}");
        }

        public void ShowResult(string resultName = null)
        {
            SetActivePanel(resultPanel);

            if (!string.IsNullOrEmpty(resultName))
                Debug.Log($"[ScreenUI] ��ܵ��G����G{resultName}");
        }

        /// <summary>
        /// ��s���G�e����ܪ���r�P�Ϥ����e
        /// </summary>
        public void UpdateResultContent(string name, string title, string introduction, string description, Sprite image)
        {
            if (resultNameText != null)
                resultNameText.text = name;

            if (resultTitleText != null)
                resultTitleText.text = title;

            if (resultIntroductionText != null)
                resultIntroductionText.text = introduction;

            if (resultDescriptionText != null)
                resultDescriptionText.text = description;

            if (resultImage != null)
            {
                resultImage.sprite = image;
                resultImage.gameObject.SetActive(image != null);
            }

            Debug.Log($"[ScreenUI] ��s���G���e �� {name}");
        }

        private void SetActivePanel(GameObject targetPanel)
        {
            if (idlePanel != null ) idlePanel.SetActive(false);
            if (questionPanel != null) questionPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
            //if (scrollPanel != null) scrollPanel.SetActive(false);

            if (targetPanel != null) targetPanel.SetActive(true);
            //if (targetPanel == questionPanel) resultPanel.SetActive(true);
        }
    }
}
