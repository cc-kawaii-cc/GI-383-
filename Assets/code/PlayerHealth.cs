using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public float health = 300f; 

    [Header("References")]
    public Slider healthBar;
    public GameTimer gameTimer; 
    // public GameManager gameManager; // ไม่ต้องใช้ตัวแปรนี้แล้ว เพราะเราเรียกผ่าน instance

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
        
        // อัปเดตหลอดเลือด
        if (healthBar != null) healthBar.value = health;
        
        // เช็คตาย
        if (health <= 0) {
            Debug.Log("Game Over!");
            
            if (GameManager.instance != null) 
            {
                GameManager.instance.GameOver();
            }
            else
            {
                Debug.LogError("หา GameManager ไม่เจอ! อย่าลืมสร้าง GameManager ในฉากครับ");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            TakeDamage(10f); 
            Destroy(other.gameObject);
        }
    }
}