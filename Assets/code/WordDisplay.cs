using UnityEngine;
using TMPro;
using System.Collections; // ต้องใช้สำหรับ Coroutine

public class WordDisplay : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    private Color originalColor = Color.white; // สีปกติ (ขาว)

    public void SetWord(string word) {
        textDisplay.text = word;
        originalColor = textDisplay.color; // จำสีเดิมไว้
    }

    public void RemoveLetter() {
        textDisplay.text = textDisplay.text.Remove(0, 1);
        textDisplay.color = Color.green; // พิมพ์ถูกเป็นสีเขียว
    }

    public void DestroyEnemy() {
        Destroy(gameObject);
    }

    // ✅ ฟังก์ชันทำให้ตัวอักษรกระพริบสีแดง
    public void FlashRed() {
        StartCoroutine(FlashColorRoutine());
    }

    IEnumerator FlashColorRoutine() {
        textDisplay.color = Color.red; // เปลี่ยนเป็นแดง
        yield return new WaitForSeconds(0.15f); // ค้างไว้แป๊บนึง
        textDisplay.color = originalColor; // กลับมาสีเดิม
    }
}
