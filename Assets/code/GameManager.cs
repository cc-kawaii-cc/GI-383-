using UnityEngine;
using UnityEngine.SceneManagement; // ใช้สำหรับโหลดฉากใหม่
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton ให้เรียกใช้ง่ายๆ

    [Header("UI Panels")]
    public GameObject gameOverPanel; // ลาก Panel "Game Over" มาใส่
    public GameObject victoryPanel;  // ลาก Panel "Victory" มาใส่
    public TextMeshProUGUI scoreText; // (Optional) เอาไว้โชว์คะแนนตอนจบ

    [Header("Game State")]
    public bool isGameEnded = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void GameOver()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        Debug.Log("YOU DIED!");
        
        // เปิดหน้าจอ Game Over
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        // หยุดเกม
        Time.timeScale = 0f; 
    }

    public void Victory()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        Debug.Log("MISSION COMPLETE!");

        // เปิดหน้าจอ Victory
        if (victoryPanel != null) victoryPanel.SetActive(true);

        // หยุดเกม
        Time.timeScale = 0f;
    }

    // ฟังก์ชันสำหรับปุ่ม Restart (ผูกกับปุ่มใน Unity)
    public void RestartGame()
    {
        Time.timeScale = 1f; // คืนค่าเวลาให้เดินปกติ
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // โหลดฉากเดิมซ้ำ
    }

    // ฟังก์ชันสำหรับปุ่ม Quit (ผูกกับปุ่มใน Unity)
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
