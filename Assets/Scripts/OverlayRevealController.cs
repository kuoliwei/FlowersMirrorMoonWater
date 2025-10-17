using UnityEngine;

public class OverlayRevealController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material overlayMat;   // 使用上面的 OverlayReveal.shader 的材質
    [SerializeField] private Camera uiCamera;       // 拍 Quad 的相機（或主相機）

    [Header("Reveal Settings")]
    [SerializeField] private float radius = 0.18f;
    [SerializeField] private float softness = 0.12f;

    [Header("Input")]
    [SerializeField] private bool useMouseInput = true;
    [SerializeField] private Transform handTransform; // 真實手座標(世界座標)，不用時可留空

    private static readonly int ID_HandPos = Shader.PropertyToID("_HandPos");
    private static readonly int ID_Radius = Shader.PropertyToID("_Radius");
    private static readonly int ID_Softness = Shader.PropertyToID("_Softness");

    void Reset()
    {
        uiCamera = Camera.main;
    }

    void Update()
    {
        if (overlayMat == null)
            return;

        Vector2 vp;
        if (useMouseInput)
        {
            vp = new Vector2(Input.mousePosition.x / Screen.width,
                             Input.mousePosition.y / Screen.height);
        }
        else if (handTransform != null && uiCamera != null)
        {
            Vector3 v3 = uiCamera.WorldToViewportPoint(handTransform.position);
            vp = new Vector2(v3.x, v3.y);
        }
        else
        {
            vp = new Vector2(0.5f, 0.5f);
        }

        overlayMat.SetVector(ID_HandPos, new Vector4(vp.x, vp.y, 0, 0));
        overlayMat.SetFloat(ID_Radius, radius);
        overlayMat.SetFloat(ID_Softness, softness);
    }
}
