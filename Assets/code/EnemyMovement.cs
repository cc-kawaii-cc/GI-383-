using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f; // ความเร็วของผี
    private Transform player;

    void Start()
    {
        // ค้นหาตำแหน่งของ Player ในฉาก (ต้องแน่ใจว่า Object ผู้เล่นชื่อ "Player")
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            // คำนวณทิศทางไปหาผู้เล่น
            Vector3 direction = (player.position - transform.position).normalized;
            // เคลื่อนที่ไปตามทิศทางนั้น
            transform.position += direction * speed * Time.deltaTime;
        }
    }
}