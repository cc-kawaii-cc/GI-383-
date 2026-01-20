using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1.5f; // ความเร็วปกติ
    public float stopDistance = 0.1f; // ระยะหยุดก่อนชน (เผื่อไว้)

    private Transform player;

    void Start()
    {
        // หาตัวผู้เล่นอัตโนมัติ
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            // คำนวณระยะห่าง
            float distance = Vector2.Distance(transform.position, player.position);

            // ถ้ายังไม่ถึงตัวผู้เล่น ให้เดินต่อ
            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    // ฟังก์ชันสำหรับปรับความเร็วตอนเกิด (เผื่อ WordSpawner อยากสั่ง)
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}