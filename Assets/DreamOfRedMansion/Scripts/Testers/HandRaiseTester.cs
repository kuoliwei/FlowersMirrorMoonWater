using DreamOfRedMansion;
using UnityEngine;

public class HandRaiseTester : MonoBehaviour
{
    public HandRaiseDetector detector;

    private void Update()
    {
        // ���U�ť�������|�⦨�\
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[Tester] ���������|���ƥ�Ĳ�o");
            detector.OnHandsRaised?.Invoke();
        }
    }
}
