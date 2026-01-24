using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // อย่าลืมใส่บรรทัดนี้เพื่อใช้ TextMeshPro

public class MainMenu : MonoBehaviour
{
    [Header("Settings")]
    public string gameplaySceneName = "SampleScene"; // ชื่อ Scene เกมหลักของคุณ (แก้ให้ตรง)

    [Header("UI References")]
    public TextMeshProUGUI progressText; // ลาก Text มาใส่เพื่อโชว์ % เนื้อเรื่อง

    void Start()
    {
        // โหลดข้อมูลความคืบหน้า (ถ้าไม่มีให้เป็น 0)
        UpdateProgressUI();
    }

    // --- ปุ่ม Start Game ---
    public void PlayGame()
    {
        // โหลด Scene เกม
        SceneManager.LoadScene(gameplaySceneName);
    }

    // --- ปุ่ม Quit Game ---
    public void QuitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    // --- ปุ่ม Reset Progress (เอาไว้เทส) ---
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll(); // ลบเซฟทั้งหมด
        UpdateProgressUI();
        Debug.Log("Save Data Cleared!");
    }

    void UpdateProgressUI()
    {
        if (progressText != null)
        {
            // ดึงค่าจาก ProgressManager ที่เราทำไว้ (Key ต้องตรงกัน)
            // สมมติใน ProgressManager คุณใช้ PlayerPrefs key ชื่อ "StoryProgress" หรือคล้ายกัน
            // ถ้ายังไม่ได้ทำเซฟ ใช้ key มาตรฐานไปก่อน
            int progress = PlayerPrefs.GetInt("StoryProgress", 0); 
            progressText.text = "Story Unlocked: " + progress + "%";
        }
    }
}