using UnityEngine;
using TMPro;

public class DeathEffect : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float duration = 1f;
    private TextMeshPro textMesh;
    private float timer;

    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        Destroy(gameObject, duration); // ทำลายตัวเองเมื่อครบเวลา
    }

    void Update()
    {
        // ลอยขึ้น
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        
        // ค่อยๆ จางหาย
        timer += Time.deltaTime;
        if (textMesh != null)
        {
            float alpha = Mathf.Lerp(1, 0, timer / duration);
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
        }
    }
}