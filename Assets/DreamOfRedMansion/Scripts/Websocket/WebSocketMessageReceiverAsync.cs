using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Xml.Linq;

public class WebSocketMessageReceiverAsync : MonoBehaviour
{
    [Header("�s�� UI ����")]
    [SerializeField] private WebSocketConnectUI webSocketConnectUI;
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private ReconnectPanelController reconnectUI;

    [Header("WebSocket �Ȥ��")]
    [SerializeField] private WebSocketClient webSocketClient;

    [Header("�O�_���\�B�z Pose ���")]
    public bool CanReceivePoseMessage = true;

    [System.Serializable]
    public class PoseFrameEvent : UnityEvent<PoseTypes.FrameSample> { }

    [Header("���� Pose ��ƨƥ�")]
    public PoseFrameEvent OnPoseFrameReceived = new();

    private readonly ConcurrentQueue<PoseTypes.FrameSample> poseMainThreadQueue = new();

    private float messageTimer = 0f;

    private void Start()
    {
        if (webSocketClient != null)
        {
            webSocketClient.OnMessageReceive.AddListener(message =>
            {
                ReceiveSkeletonMessage(message); // �ȱ������[���
            });
            webSocketClient.OnConnected.AddListener(OnWebSocketConnected);
            webSocketClient.OnConnectionError.AddListener(() =>
            {
                webSocketConnectUI.OnConnectionFaild("�s�u����");
            });
            webSocketClient.OnDisconnected.AddListener(OnWebSocketDisconnected);
        }
    }

    private void Update()
    {
        // �B�z���[ Pose ��C
        int processedPose = 0;
        while (poseMainThreadQueue.TryDequeue(out var frame))
        {
            OnPoseFrameReceived.Invoke(frame);
            processedPose++;
        }

        // �C��ʱ��i�}�Ұ������
        messageTimer += Time.deltaTime;
        if (messageTimer >= 1f)
        {
            // Debug.Log($"[�ʱ�] �C��B�z {processedPose} �հ��[��ơCQueue �Ѿl�G{poseMainThreadQueue.Count}");
            messageTimer = 0f;
        }
    }

    private void ReceiveSkeletonMessage(string messageContent)
    {
        //Debug.Log($"receive pose message�G{messageContent}");
        if (!CanReceivePoseMessage)
        {
            Debug.LogWarning("�����T���GCanReceivePoseMessage �� false");
            return;
        }

        try
        {
            // �榡: { "<frameIndex>": [ [ [x,y,z,conf],...17�I ],  [ ...�H��1... ] ] }
            var root = JObject.Parse(messageContent);

            var enumerator = root.Properties().GetEnumerator();
            if (!enumerator.MoveNext())
            {
                Debug.LogWarning("[ReceiveMessage] JSON �ѪR���\���S������ frame key");
                return;
            }

            var frameProp = enumerator.Current;
            if (!int.TryParse(frameProp.Name, out int frameIndex))
            {
                Debug.LogError($"[ReceiveMessage] frame key �L�k�ন int: {frameProp.Name}");
                return;
            }

            var personsArray = frameProp.Value as JArray;
            if (personsArray == null)
            {
                Debug.LogError("[ReceiveMessage] frame value ���O�}�C (persons)");
                return;
            }

            var frame = new PoseTypes.FrameSample(frameIndex);
            frame.recvTime = Time.realtimeSinceStartup;

            for (int personId = 0; personId < personsArray.Count; personId++)
            {
                var personJoints = personsArray[personId] as JArray;
                if (personJoints == null)
                {
                    Debug.LogWarning($"[ReceiveMessage] �H�� {personId} joints ���O�}�C�A���L");
                    continue;
                }

                var person = new PoseTypes.PersonSkeleton();
                int jointCount = Math.Min(personJoints.Count, PoseTypes.PoseSchema.JointCount);
                for (int j = 0; j < jointCount; j++)
                {
                    var jArr = personJoints[j] as JArray;
                    if (jArr == null || jArr.Count < 4)
                        continue;

                    float x = jArr[0]!.Value<float>();
                    float y = jArr[1]!.Value<float>();
                    float z = jArr[2]!.Value<float>();
                    float conf = jArr[3]!.Value<float>();

                    person.joints[j] = new PoseTypes.Joint(x, y, z, conf);
                }

                frame.persons.Add(person);
            }

            // ���D������ƥ��C
            poseMainThreadQueue.Enqueue(frame);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ReceiveMessage] �ѪR Pose JSON ���ѡCError: {e.Message}\n���e: {messageContent}");
        }
    }

    private void OnWebSocketDisconnected()
    {
        Debug.Log("�I�s OnWebSocketDisconnected()");
        if (!connectPanel.activeSelf)
        {
            reconnectUI?.ShowFlicker();
            webSocketClient.allowReconnect = true;
            webSocketClient.isReconnectAttempt = true;
            Debug.Log("���u���A�۰ʱҥέ��s����");
        }
        else
        {
            Debug.Log("ConnectPanel �}�Ҥ��A���۰ʭ��s");
        }
    }

    private void OnWebSocketConnected()
    {
        reconnectUI?.ShowSuccessAndHide();
        webSocketConnectUI?.OnConnectionSucceeded();
        webSocketClient.allowReconnect = false;

        if (webSocketClient.isReconnectAttempt)
        {
            Debug.Log("���s�s�u���\");
            webSocketClient.isReconnectAttempt = false;
        }
    }

    public void ConnectToServer(string ip, string port)
    {
        string address = $"ws://{ip}:{port}";
        webSocketClient.CloseConnection();
        webSocketClient.StartConnection(address);
    }
}
