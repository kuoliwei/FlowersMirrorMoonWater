using UnityEngine;
using Newtonsoft.Json.Linq; // �ΨӳB�z JSON ���c

public class RollerSimulator : MonoBehaviour
{
    public WebSocketServerManager manager;

    [Header("�ǰe����")]
    [Tooltip("�C��̦h�ǰe�X�����")]
    public int sendRatePerSecond = 15;

    private float sendInterval;
    private float sendTimer = 0f;
    private bool allowSending = false;

    // �����}���G�O�_�n���ܤⳡ����
    private bool modifyHandHeight = false;

    // �����}���G�O�_�n���ܻ�l�y��
    private bool modifyNoseCoordinate = false;
    [SerializeField] private Vector2 newNoseCoordinate;

    private void Start()
    {
        sendInterval = 1f / Mathf.Max(1, sendRatePerSecond);
    }

    private void Update()
    {
        // ���U P ��}�� Pose �ǰe
        if (Input.GetKeyDown(KeyCode.P))
        {
            allowSending = !allowSending;
            Debug.Log($"[Simulator] Pose �ǰe {(allowSending ? "�Ұ�" : "����")}");
        }

        // ���U U ������ⳡ���׭ק�Ҧ�
        if (Input.GetKeyDown(KeyCode.U))
        {
            modifyHandHeight = !modifyHandHeight;
            Debug.Log($"[Simulator] �ⳡ���׭ק� {(modifyHandHeight ? "�ҥ�" : "����")}");
        }

        // ���U N �������l�y�Эק�Ҧ�
        if (Input.GetKeyDown(KeyCode.N))
        {
            modifyNoseCoordinate = !modifyNoseCoordinate;
            Debug.Log($"[Simulator] ��l�y�Эק� {(modifyNoseCoordinate ? "�ҥ�" : "����")}");
        }

        // �w�ɰe�X
        if (allowSending)
        {
            sendTimer += Time.deltaTime;
            if (sendTimer >= sendInterval)
            {
                SendModifiedPose();
                sendTimer = 0f;
            }
        }
    }

    /// <summary>
    /// �ھ� modifyHandHeight ���A�ק� poseJsonRaw�A�M��e�X
    /// </summary>
    private void SendModifiedPose()
    {
        if (string.IsNullOrWhiteSpace(manager.poseJsonRaw))
            return;

        try
        {
            var root = JObject.Parse(manager.poseJsonRaw);

            // ���X frame key�]�Ҧp "0"�^
            var enumerator = root.Properties().GetEnumerator();
            if (!enumerator.MoveNext())
            {
                Debug.LogWarning("[Simulator] JSON ���c���S�� frame ���");
                return;
            }

            var frameProp = enumerator.Current;

            // �����i�w���Ǯ榡: frameProp.Value �N�O�H���}�C
            var personsArray = frameProp.Value as JArray;
            if (personsArray == null || personsArray.Count == 0)
            {
                Debug.LogWarning("[Simulator] �L�H�����");
                return;
            }

            // �Ĥ@�ӤH
            var person = personsArray[0] as JArray;
            if (person == null)
                return;

            const int LeftShoulder = 5;
            const int RightShoulder = 6;
            const int LeftWrist = 9;
            const int RightWrist = 10;
            const int Nose = 0;

            if (person.Count > RightWrist)
            {
                float lShoulderZ = (float)person[LeftShoulder][2];
                float rShoulderZ = (float)person[RightShoulder][2];
                float lWristZ_before = (float)person[LeftWrist][2];
                float rWristZ_before = (float)person[RightWrist][2];
                Vector2 noseCoordinate_before = new Vector2((float)person[Nose][0], (float)person[Nose][1]);
                if (modifyHandHeight)
                {
                    float delta = 0.3f;
                    person[LeftWrist][2] = lShoulderZ + delta;
                    person[RightWrist][2] = rShoulderZ + delta;
                }

                if (modifyNoseCoordinate)
                {
                    person[Nose][0] = newNoseCoordinate.x;
                    person[Nose][1] = newNoseCoordinate.y;
                }

                //float lWristZ_after = (float)person[LeftWrist][2];
                //float rWristZ_after = (float)person[RightWrist][2];

                //Debug.Log(
                //    $"[Simulator] U={modifyHandHeight} | " +
                //    $"LShoulderZ={lShoulderZ:F3}, LWristZ(before)={lWristZ_before:F3}��(after)={lWristZ_after:F3} | " +
                //    $"RShoulderZ={rShoulderZ:F3}, RWristZ(before)={rWristZ_before:F3}��(after)={rWristZ_after:F3}"
                //);
            }

            string modifiedJson = root.ToString();
            manager.server.SendMessageToClient(modifiedJson);
            //Debug.Log($"[Simulator] �w�e�X���[��ơ]U�Ҧ�:{modifyHandHeight}�^");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Simulator] �ק� Pose JSON ���ѡG{e.Message}");
            manager.SendPoseOnce();
        }
    }
}
