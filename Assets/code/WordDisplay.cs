using System.Collections;
using UnityEngine;
using TMPro; // จำเป็นสำหรับการใช้ TextMeshPro

public class WordDisplay : MonoBehaviour
{
    public TMP_Text textDisplay; // ใช้ TMP_Text เพื่อรองรับทั้ง World และ UI

    // เปลี่ยนข้อความในจอ
    public void SetWord(string word)
    {
        if (textDisplay != null) textDisplay.text = word;
    }

    // ฟังก์ชันลบตัวอักษร (พิมพ์ถูก)
    public void RemoveLetter()
    {
        if (textDisplay != null && textDisplay.text.Length > 0)
        {
            textDisplay.text = textDisplay.text.Remove(0, 1); // ลบตัวอักษรตัวแรกออก
            
            // ✅ เปลี่ยนเป็นสีเขียวเมื่อพิมพ์ถูก
            textDisplay.color = Color.green;
            
            // สั่งให้รีเซ็ตกลับเป็นสีเดิมหลังจากแป๊บนึง
            StopAllCoroutines(); 
            StartCoroutine(ResetColorDelay());
        }
    }

    public void RemoveWord()
    {
        Destroy(gameObject);
    }
    
    // ฟังก์ชันแจ้งเตือนพิมพ์ผิด (จอแดง/ตัวแดง)
    public void FlashRed()
    {
        if (textDisplay != null)
        {
            // ถ้าเป็นบอส (ที่ตัวหนังสือแดงอยู่แล้ว) อาจจะให้กระพริบเป็นขาว หรือแดงเข้ม ก็ได้
            // แต่ในที่นี้สั่งเป็นแดงตามปกติ
            textDisplay.color = Color.red; 
            
            StopAllCoroutines();
            StartCoroutine(ResetColorDelay());
        }
    }

    // ✅ รวมมาไว้ที่นี่อันเดียว (แก้ Error ฟังก์ชันซ้ำ)
    IEnumerator ResetColorDelay()
    {
        yield return new WaitForSeconds(0.1f); // รอ 0.1 วินาที
        
        if (textDisplay != null) 
        {
            // เช็คว่าถ้าเป็น UI บอส (ที่ตั้ง Tag ไว้) ให้กลับเป็นสีแดงเหมือนเดิม
            if (gameObject.CompareTag("BossUI")) 
            {
                textDisplay.color = Color.red; 
            }
            else 
            {
                textDisplay.color = Color.white; // ตัวธรรมดาให้กลับเป็นสีขาว
            }
        }
    }

    // ฟังก์ชันสำหรับบอส (WordSpawner เรียกใช้)
    public void Setup(Word word)
    {
        SetWord(word.text);
    }
    
    public void DestroyEnemy()
    {
        // ถ้าเป็น UI ของบอส ให้ซ่อนแทนการทำลาย
        if (textDisplay != null && textDisplay.gameObject.CompareTag("BossUI"))
        {
            textDisplay.text = "";
            textDisplay.gameObject.SetActive(false);
        }
        Destroy(gameObject);
    }
}