using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public float health = 300f; 

    [Header("References")]
    public Slider healthBar;
    public GameTimer gameTimer; 
    // public GameManager gameManager; // (ถ้าใช้ GameManager แทน GameTimer ให้เปิดอันนี้)

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
            
            // ถ้าใช้ GameTimer
            if(gameTimer != null) gameTimer.StopTimer();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            // ถ้าชนมอนสเตอร์ ให้ลดเลือด (ปรับค่าความแรงได้ตรงนี้)
            TakeDamage(10f); 
            Destroy(other.gameObject);
        }
    }
}