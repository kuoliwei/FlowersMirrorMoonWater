using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 啟用所有連接的顯示器（Display 0 以外的 Display 1, 2, ...）
/// 放到任何常駐物件（例如 GameManager）即可。
/// </summary>
public class MultiDisplayActivator : MonoBehaviour
{
    [SerializeField] private InputField mainCameraTargetDisplay;
    [SerializeField] private InputField mainCanvasTargetDisplay;
    //[SerializeField] private InputField leftCanvasTargetDisplay;
    //[SerializeField] private InputField rightCanvasTargetDisplay;
    //[SerializeField] private InputField groundCanvasTargetDisplay;
    [SerializeField] private InputField leftCameraTargetDisplay;
    [SerializeField] private InputField rightCameraTargetDisplay;
    [SerializeField] private InputField groundCameraTargetDisplay;
    [SerializeField] private Button changeGroundCameraRotation;

    [SerializeField] private Text currentMainCameraTargetDisplay;
    [SerializeField] private Text currentMainCanvasTargetDisplay;
    //[SerializeField] private Text currentLeftCanvasTargetDisplay;
    //[SerializeField] private Text currentRightCanvasTargetDisplay;
    //[SerializeField] private Text currentGroundCanvasTargetDisplay;
    [SerializeField] private Text currentLeftCameraTargetDisplay;
    [SerializeField] private Text currentRightCameraTargetDisplay;
    [SerializeField] private Text currentGroundCameraTargetDisplay;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas mainCanvas;
    //[SerializeField] private Canvas leftCanvas;
    //[SerializeField] private Canvas rightCanvas;
    //[SerializeField] private Canvas groundCanvas;
    [SerializeField] private Camera leftCamera;
    [SerializeField] private Camera rightCamera;
    [SerializeField] private Camera groundCamera;
    [SerializeField] private Transform cornerGroundCameraTransform;

    [SerializeField] string jsonPath = "DisplayIndexData";
    [SerializeField] string jsonName = "DisplayIndexData.json";

    [SerializeField] GameObject displayControlPanel;

    private int displayLength;
    private void Awake()
    {
#if !UNITY_EDITOR
        // Display 0 是主顯示器，其他需要手動啟用
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            Debug.Log($"已啟用 Display {i}");
        }
        displayLength = Display.displays.Length;
        Debug.Log($"多顯示器啟動完成，共偵測到 {Display.displays.Length} 個顯示器。");
#endif

#if UNITY_EDITOR
        displayLength = 4;
#endif
        //currentMainCameraTargetDisplay.text = mainCamera.targetDisplay.ToString();
        //currentMainCanvasTargetDisplay.text = mainCanvas.targetDisplay.ToString();
        ////currentLeftCanvasTargetDisplay.text = leftCanvas.targetDisplay.ToString();
        ////currentRightCanvasTargetDisplay.text = rightCanvas.targetDisplay.ToString();
        ////currentGroundCanvasTargetDisplay.text = groundCanvas.targetDisplay.ToString();
        //currentLeftCameraTargetDisplay.text = leftCamera.targetDisplay.ToString();
        //currentRightCameraTargetDisplay.text = rightCamera.targetDisplay.ToString();
        //currentGroundCameraTargetDisplay.text = groundCamera.targetDisplay.ToString();
    }
    private void Start()
    {
        init();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            displayControlPanel.SetActive(!displayControlPanel.activeSelf);
        }
    }
    async void init()
    {
        DisplayIndex displayIndex = await LoadDisplayIndex(jsonPath, jsonName);
        currentMainCameraTargetDisplay.text = displayIndex.cornerScreenCamsIndex[0].ToString();
        currentMainCanvasTargetDisplay.text = displayIndex.uiIndex.ToString();
        currentLeftCameraTargetDisplay.text = displayIndex.cornerScreenCamsIndex[1].ToString();
        currentRightCameraTargetDisplay.text = displayIndex.cornerScreenCamsIndex[2].ToString();
        currentGroundCameraTargetDisplay.text = displayIndex.cornerScreenCamsIndex[3].ToString();
        mainCamera.targetDisplay = displayIndex.cornerScreenCamsIndex[0];
        mainCanvas.targetDisplay = displayIndex.uiIndex;
        leftCamera.targetDisplay = displayIndex.cornerScreenCamsIndex[1];
        rightCamera.targetDisplay = displayIndex.cornerScreenCamsIndex[2];
        groundCamera.targetDisplay = displayIndex.cornerScreenCamsIndex[3];
        cornerGroundCameraTransform.transform.localEulerAngles = new Vector3(0, 0, displayIndex.groundDir);
    }
    public async Task<DisplayIndex> LoadDisplayIndex(string jsonPath, string jsonName)
    {
        string savePath = Path.Combine(Application.dataPath, jsonPath);
        string fullPath = Path.Combine(savePath, jsonName);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[LoadDisplayIndex] File does not exist: {fullPath}");
            await SaveDisplayIndexData(jsonPath, jsonName);
        }
        var loaded = await JsonFileUtility.LoadFromFileAsync<DisplayIndex>(savePath, jsonName);

        if (!loaded.HasValue)
        {
            Debug.LogWarning("Failed to load calibration file.");
            return new DisplayIndex();
        }

        return loaded.Value;
    }
    async Task SaveDisplayIndexData(string jsonPath, string jsonName)
    {
        DisplayIndex displayIndex = new DisplayIndex
        {
            cornerScreenCamsIndex = new[] { mainCamera.targetDisplay, leftCamera.targetDisplay, rightCamera.targetDisplay, groundCamera.targetDisplay },
            uiIndex = mainCanvas.targetDisplay,
            groundDir = (int)cornerGroundCameraTransform.transform.localEulerAngles.z
        };
        string savePath = Path.Combine(Application.dataPath, jsonPath);
        // 儲存
        await JsonFileUtility.SaveToFileAsync(displayIndex, savePath, jsonName);
    }
    void OnApplicationQuit()
    {
        SaveDisplayIndexData(jsonPath, jsonName);
    }
    public void SetMainCameraTargetDisplay()
    {
        Debug.Log(displayLength);
        if (int.TryParse(mainCameraTargetDisplay.text, out int target) && target < displayLength)
        {
            mainCamera.targetDisplay = target;
            currentMainCameraTargetDisplay.text = mainCamera.targetDisplay.ToString();
            mainCameraTargetDisplay.text = "";
        }
        else
        {
            mainCameraTargetDisplay.text = "輸入錯誤";
        }
    }
    public void SetMainCanvasTargetDisplay()
    {
        Debug.Log(displayLength);
        if (int.TryParse(mainCanvasTargetDisplay.text, out int target) && target < displayLength)
        {
            mainCanvas.targetDisplay = target;
            currentMainCanvasTargetDisplay.text = mainCanvas.targetDisplay.ToString();
            mainCanvasTargetDisplay.text = "";
        }
        else
        {
            mainCanvasTargetDisplay.text = "輸入錯誤";
        }
    }
    //public void SetLeftCanvasTargetDisplay()
    //{
    //    Debug.Log(displayLength);
    //    if (int.TryParse(leftCanvasTargetDisplay.text, out int target) && target < displayLength)
    //    {
    //        leftCanvas.targetDisplay = target;
    //        currentLeftCanvasTargetDisplay.text = leftCanvas.targetDisplay.ToString();
    //        leftCanvasTargetDisplay.text = "";
    //    }
    //    else
    //    {
    //        leftCanvasTargetDisplay.text = "輸入錯誤";
    //    }
    //}
    //public void SetRightCanvasTargetDisplay()
    //{
    //    Debug.Log(displayLength);
    //    if (int.TryParse(rightCanvasTargetDisplay.text, out int target) && target < displayLength)
    //    {
    //        rightCanvas.targetDisplay = target;
    //        currentRightCanvasTargetDisplay.text = rightCanvas.targetDisplay.ToString();
    //        rightCanvasTargetDisplay.text = "";
    //    }
    //    else
    //    {
    //        rightCanvasTargetDisplay.text = "輸入錯誤";
    //    }
    //}
    //public void SetGroundCanvasTargetDisplay()
    //{
    //    Debug.Log(displayLength);
    //    if (int.TryParse(groundCanvasTargetDisplay.text, out int target) && target < displayLength)
    //    {
    //        groundCanvas.targetDisplay = target;
    //        currentGroundCanvasTargetDisplay.text = groundCanvas.targetDisplay.ToString();
    //        groundCanvasTargetDisplay.text = "";
    //    }
    //    else
    //    {
    //        groundCanvasTargetDisplay.text = "輸入錯誤";
    //    }
    //}
    public void SetLeftCameraTargetDisplay()
    {
        Debug.Log(displayLength);
        if (int.TryParse(leftCameraTargetDisplay.text, out int target) && target < displayLength)
        {
            leftCamera.targetDisplay = target;
            currentLeftCameraTargetDisplay.text = leftCamera.targetDisplay.ToString();
            leftCameraTargetDisplay.text = "";
        }
        else
        {
            leftCameraTargetDisplay.text = "輸入錯誤";
        }
    }
    public void SetRightCameraTargetDisplay()
    {
        Debug.Log(displayLength);
        if (int.TryParse(rightCameraTargetDisplay.text, out int target) && target < displayLength)
        {
            rightCamera.targetDisplay = target;
            currentRightCameraTargetDisplay.text = rightCamera.targetDisplay.ToString();
            rightCameraTargetDisplay.text = "";
        }
        else
        {
            rightCameraTargetDisplay.text = "輸入錯誤";
        }
    }
    public void SetGroundCameraTargetDisplay()
    {
        Debug.Log(displayLength);
        if (int.TryParse(groundCameraTargetDisplay.text, out int target) && target < displayLength)
        {
            groundCamera.targetDisplay = target;
            currentGroundCameraTargetDisplay.text = groundCamera.targetDisplay.ToString();
            groundCameraTargetDisplay.text = "";
        }
        else
        {
            groundCameraTargetDisplay.text = "輸入錯誤";
        }
    }
    public void ChangeGroundCameraRotation()
    {
        Vector3 currentLocalEulerAngles = new Vector3(0, 0, (int)cornerGroundCameraTransform.localEulerAngles.z + 90);

        cornerGroundCameraTransform.localEulerAngles = currentLocalEulerAngles;
    }
}
