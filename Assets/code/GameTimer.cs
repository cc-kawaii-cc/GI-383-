using UnityEngine;
using TMPro; // อย่าลืมใส่บรรทัดนี้เพื่อใช้ TextMeshPro

public class GameTimer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText; // ลาก Text TMP มาใส่ตรงนี้

    private float elapsedTime = 0f;
    private bool isRunning = true;

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime; // นับเวลาเพิ่มขึ้นเรื่อยๆ

        // คำนวณ นาที : วินาที
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        // แสดงผลในรูปแบบ 00:00
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}
