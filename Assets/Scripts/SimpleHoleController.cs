using UnityEngine;

public class SimpleHoleController : MonoBehaviour
{
    [SerializeField] private Material mat;

    void Update()
    {
        // 取得滑鼠位置（螢幕座標）
        Vector3 mousePos = Input.mousePosition;

        // 轉換成 0~1 的畫面UV座標
        Vector2 uv = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

        // 傳給 Shader
        mat.SetVector("_HoleCenter", new Vector4(uv.x, uv.y, 0, 0));
    }
}
