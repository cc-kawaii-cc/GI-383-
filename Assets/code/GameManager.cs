using UnityEngine;
using UnityEngine.SceneManagement; // ใช้สำหรับโหลดฉากใหม่
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; 

    [Header("UI Panels")]
    public GameObject gameOverPanel; 
    public GameObject victoryPanel;  
    public TextMeshProUGUI scoreText; 

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
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void Victory()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        Debug.Log("MISSION COMPLETE!");
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // --- เพิ่มฟังก์ชันนี้สำหรับปุ่ม "Main Menu" ---
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // คืนค่าเวลาก่อนเปลี่ยนฉาก
        SceneManager.LoadScene("MainMenu"); // ต้องตั้งชื่อ Scene เมนูว่า "MainMenu"
    }
}