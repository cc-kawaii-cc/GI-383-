using UnityEngine;
using TMPro;
using System.Collections; 

public class WordDisplay : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    private Color originalColor = Color.white; 
    
    // เก็บตัวแปร Coroutine ไว้เพื่อสั่งหยุดถ้ามีการพิมพ์ซ้อนกันเร็วๆ (กันสีเพี้ยน)
    private Coroutine colorCoroutine; 

    public void SetWord(string word) {
        textDisplay.text = word;
        // เช็คว่า textDisplay มีค่าไหม กัน Error
        if (textDisplay != null)
        {
            originalColor = textDisplay.color; 
        }
    }

    public void RemoveLetter() {
        if (textDisplay == null) return;

        textDisplay.text = textDisplay.text.Remove(0, 1);
        
        // เปลี่ยนจากเปลี่ยนสีเฉยๆ เป็นสั่งกระพริบเขียว
        FlashColor(Color.green);
    }

    public void DestroyEnemy() {
        Destroy(gameObject);
    }

    public void FlashRed() {
        // สั่งกระพริบแดง
        FlashColor(Color.red);
    }

    // ฟังก์ชันกลางสำหรับจัดการเปลี่ยนสี (ใช้ร่วมกันทั้งแดงและเขียว)
    void FlashColor(Color colorToFlash) {
        if (textDisplay == null) return;

        // ถ้ากำลังกระพริบสีอื่นอยู่ ให้หยุดก่อน แล้วเริ่มสีใหม่ทันที (พิมพ์รัวจะได้ไม่หน่วง)
        if (colorCoroutine != null) StopCoroutine(colorCoroutine);
        
        colorCoroutine = StartCoroutine(FlashRoutine(colorToFlash));
    }

    IEnumerator FlashRoutine(Color colorToFlash) {
        textDisplay.color = colorToFlash;       // เปลี่ยนสีตามที่สั่ง (เขียว/แดง)
        yield return new WaitForSeconds(0.1f);  // ค้างไว้ 0.1 วินาที (ปรับได้)
        textDisplay.color = originalColor;      // กลับมาสีเดิม
    }
}