using DreamOfRedMansion;
using UnityEngine;

public class HandRaiseTester : MonoBehaviour
{
    public HandRaiseDetector detector;

    private void Update()
    {
        // 按下空白鍵模擬舉手成功
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[Tester] 模擬雙手舉高事件觸發");
            detector.OnHandsRaised?.Invoke();
        }
    }
}
