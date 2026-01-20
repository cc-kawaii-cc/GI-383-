using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 1.5f; 
    public float damage = 10f; // เพิ่มตัวแปรนี้ (ปรับใน Unity ได้เลย)
    
    public float stopDistance = 0.1f; 
    private Transform player;

    void Start()
    {
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
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
    }
}