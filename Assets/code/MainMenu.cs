using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI progressText; 
    public GameObject languageSelectionPanel; // 1. ลาก Panel เลือกภาษามาใส่ตรงนี้

    void Start()
    {
        UpdateProgressUI();
        
        // ซ่อนหน้าต่างเลือกภาษาไว้ก่อน (กันพลาด)
        if (languageSelectionPanel != null) 
            languageSelectionPanel.SetActive(false);
    }

    // --- แก้ไข: ปุ่ม Start Game เดิม ให้เปิดหน้าต่างเลือกภาษาแทน ---
    public void OnStartGameClicked()
    {
        if (languageSelectionPanel != null)
        {
            languageSelectionPanel.SetActive(true); // โชว์หน้าต่างเลือกภาษา
        }
        else
        {
            Debug.LogError("ลืมลาก Language Panel ใส่ใน Script ครับ!");
        }
    }

    // --- ปุ่มกากบาท (Back) ในหน้าเลือกภาษา ---
    public void CloseLanguagePanel()
    {
        if (languageSelectionPanel != null) 
            languageSelectionPanel.SetActive(false);
    }

    // --- ปุ่มเลือกภาษา THAI (กดแล้วเข้า scene ไทย) ---
    public void PlayGameThai()
    {
        SceneManager.LoadScene("GameScene_TH"); // ตรวจสอบชื่อ Scene ให้ตรงเป๊ะๆ
    }

    // --- ปุ่มเลือกภาษา ENGLISH (กดแล้วเข้า scene อังกฤษ) ---
    public void PlayGameEnglish()
    {
        SceneManager.LoadScene("GameScene_EN"); // ตรวจสอบชื่อ Scene ให้ตรงเป๊ะๆ
    }

    public void QuitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll(); 
        UpdateProgressUI();
        Debug.Log("Save Data Cleared!");
    }

    void UpdateProgressUI()
    {
        if (progressText != null)
        {
            int progress = PlayerPrefs.GetInt("StoryProgress", 0); 
            progressText.text = "Story Unlocked: " + progress + "%";
        }
    }
}