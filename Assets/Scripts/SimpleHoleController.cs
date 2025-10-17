using UnityEngine;

public class SimpleHoleController : MonoBehaviour
{
    [SerializeField] private Material mat;

    void Update()
    {
        // ���o�ƹ���m�]�ù��y�С^
        Vector3 mousePos = Input.mousePosition;

        // �ഫ�� 0~1 ���e��UV�y��
        Vector2 uv = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

        // �ǵ� Shader
        mat.SetVector("_HoleCenter", new Vector4(uv.x, uv.y, 0, 0));
    }
}
