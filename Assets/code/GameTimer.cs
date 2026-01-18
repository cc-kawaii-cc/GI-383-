using UnityEngine;
using TMPro; // อย่าลืมใส่เพื่อให้ใช้ TextMeshPro ได้

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // ลาก Text UI มาใส่ที่นี่
    private float startTime;
    private bool isGameOver = false;

    void Start()
    {
        // เริ่มนับเวลาจาก 0
        startTime = Time.time;
    }

    void Update()
    {
        if (isGameOver) return;

        // คำนวณเวลาที่ผ่านไป
        float t = Time.time - startTime;

        // แปลงเป็น นาที และ วินาที
        string minutes = ((int)t / 60).ToString("00");
        string seconds = (t % 60).ToString("00");

        // อัปเดตตัวเลขบนหน้าจอ
        timerText.text = minutes + ":" + seconds;
    }

    // ฟังก์ชันสำหรับสั่งหยุดเวลาเมื่อเกมจบ
    public void StopTimer()
    {
        isGameOver = true;
    }
}
