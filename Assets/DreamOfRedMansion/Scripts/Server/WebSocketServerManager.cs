using System.Collections;
using UnityEngine;

public class WebSocketServerManager : MonoBehaviour
{
    public WebSocketServer.WebSocketServer server;

    // ====== �H�鰩�[���ժ� ======
    [Header("Pose JSON�]��˭��e�^")]
    [TextArea(6, 18)]
    public string poseJsonRaw; // �� 3DPOSE_Output.txt �����e��q�K�i��

    [Header("Pose JSON �ӷ� (Resources)")]
    [Tooltip("��b Assets/Resources/ �U���ɦW�]���t���ɦW�^")]
    public string poseResourceName = "3DPOSE_Output";

    [Tooltip("�Ұʮɦ۰ʱq Resources ���J")]
    public bool autoLoadPoseFromResources = true;

    [Header("�e�X�W�v")]
    public float poseFps = 15f;

    private Coroutine poseLoopCo;

    private void Awake()
    {
        if (autoLoadPoseFromResources)
            LoadPoseFromResources();
    }

    /// <summary>
    /// �q Assets/Resources/{poseResourceName}.txt ���J�ö�J poseJsonRaw
    /// </summary>
    public void LoadPoseFromResources()
    {
        var ta = Resources.Load<TextAsset>(poseResourceName);
        if (ta == null)
        {
            Debug.LogError($"[Server] �䤣�� Resources/{poseResourceName}.txt �� .json");
            return;
        }
        poseJsonRaw = ta.text;
        Debug.Log($"[Server] �w���J Pose JSON�]{poseResourceName}�A{poseJsonRaw.Length} chars�^");
    }

    /// <summary>
    /// �榸�e�X�]��ˡ^
    /// </summary>
    public void SendPoseOnce()
    {
        if (string.IsNullOrWhiteSpace(poseJsonRaw))
        {
            Debug.LogWarning("[Server] poseJsonRaw �O�Ū��A�L�k�e�X");
            return;
        }
        server.SendMessageToClient(poseJsonRaw);
    }

    /// <summary>
    /// �s��e�X�]��ˡ^
    /// </summary>
    public void StartPoseLoop()
    {
        if (poseLoopCo != null) StopCoroutine(poseLoopCo);
        poseLoopCo = StartCoroutine(CoPoseLoop());
    }

    public void StopPoseLoop()
    {
        if (poseLoopCo != null) StopCoroutine(poseLoopCo);
        poseLoopCo = null;
    }

    private IEnumerator CoPoseLoop()
    {
        if (string.IsNullOrWhiteSpace(poseJsonRaw))
        {
            Debug.LogWarning("[Server] poseJsonRaw �O�Ū��A�L�k�Ұʳs��e�X");
            yield break;
        }

        var wait = new WaitForSeconds(1f / Mathf.Max(1f, poseFps));
        while (true)
        {
            server.SendMessageToClient(poseJsonRaw);
            yield return wait;
        }
    }
}
