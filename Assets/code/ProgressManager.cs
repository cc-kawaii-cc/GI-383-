using UnityEngine;
using TMPro;

public class ProgressManager : MonoBehaviour
{
    public TextMeshProUGUI progressText; // ลาก Text UI มาใส่ที่นี่
    public int totalSpecialWordsInLevel = 10; // กำหนดว่าด่านนี้มีคำพิเศษกี่คำ
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
        // คำนวณเป็น %
        float percent = ((float)currentSpecialCount / totalSpecialWordsInLevel) * 100f;
        progressText.text = "Progress: " + percent.ToString("F0") + "%";

        if (percent >= 100f)
        {
            Debug.Log("Mission Complete!"); // ใส่ Logic เมื่อครบ 100% ตรงนี้
        }
    }
}