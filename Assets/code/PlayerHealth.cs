using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public float health = 300f; 

    [Header("References")]
    public Slider healthBar;
    // public GameTimer gameTimer; 

    void Start()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = health; 
            healthBar.value = health;   
        }
    }

    public void TakeDamage(float amount) {
        health -= amount;
        
        if (healthBar != null) healthBar.value = health;
        
        if (health <= 0) {
            Debug.Log("Game Over!");
            if (GameManager.instance != null) 
            {
                GameManager.instance.GameOver();
            }
        }
    }

    // แก้ไขฟังก์ชันนี้: แยกบอสออกจากลูกน้อง
    private void OnTriggerEnter2D(Collider2D other) {
        
        // เช็คว่าเป็น "บอส" หรือไม่? 
        // (โดยดูว่าชื่อมีคำว่า Boss หรือ Tag เป็น Boss)
        bool isBoss = other.gameObject.name.Contains("Boss") || other.CompareTag("Boss");

        if (isBoss) 
        {
            // --- กรณีบอสชน ---
            TakeDamage(20f); // บอสชนแรงกว่า
            
            Debug.Log("👻 บอสชน! ดีดกลับไปเกิดใหม่");

            // สั่งย้ายบอสไปจุดใหม่ (Teleport) แทนการทำลาย
            // สุ่มตำแหน่งเป็นวงกลม รอบตัวผู้เล่น (ระยะ 10 เมตร)
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 newPos = transform.position + (Vector3)(randomDir * 10f); 
            
            other.transform.position = newPos;

            // บอสจะวิ่งเข้ามาหาเราใหม่เอง (เพราะ EnemyMovement สั่งเดินหา Player ตลอด)
            // และคำศัพท์จะยังคงเป็นคำเดิมที่เหลืออยู่
        }
        else if (other.CompareTag("Enemy")) 
        {
            // --- กรณีลูกน้องชน ---
            TakeDamage(10f); 
            Destroy(other.gameObject); // ลูกน้องตายทันที
        }
    }
}