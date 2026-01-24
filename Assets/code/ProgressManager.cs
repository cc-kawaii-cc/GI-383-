using UnityEngine;
using TMPro;

public class ProgressManager : MonoBehaviour
{
    public TextMeshProUGUI progressText;
    public int totalSpecialWordsInLevel = 10; 
    private int currentSpecialCount = 0;

    void Start()
    {
        UpdateProgressUI();
    }

    public void AddProgress()
    {
        currentSpecialCount++;
        UpdateProgressUI();
    }

    void UpdateProgressUI()
    {
        // คำนวณ %
        float percent = ((float)currentSpecialCount / totalSpecialWordsInLevel) * 100f;
        
        // อัปเดต UI
        if(progressText != null) 
            progressText.text = "Story Found: " + percent.ToString("F0") + "%";

        // --- ส่วนที่เพิ่ม: บันทึกข้อมูลลงเครื่อง (Save System) ---
        // 1. โหลดค่าเก่ามาก่อน (ถ้าไม่มีให้เป็น 0)
        int currentSavedHighscore = PlayerPrefs.GetInt("StoryProgress", 0);
        
        // 2. ถ้า % ที่เล่นได้รอบนี้ "มากกว่า" ของเดิม ให้บันทึกทับ
        if ((int)percent > currentSavedHighscore)
        {
            PlayerPrefs.SetInt("StoryProgress", (int)percent);
            PlayerPrefs.Save(); // สั่งบันทึกทันที
            Debug.Log("Progress Saved: " + percent + "%");
        }
        // ---------------------------------------------------

        if (percent >= 100f)
        {
            Debug.Log("Mission Complete! All Story Unlocked.");
        }
    }
}