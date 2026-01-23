using UnityEngine;
using TMPro;
using System.Collections; 

public class WordDisplay : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    private Color originalColor = Color.white; 
    private Coroutine colorCoroutine; 

    void Start()
    {
        if (textDisplay != null)
        {
            originalColor = textDisplay.color; 
        }
    }
    public void Setup(Word word)
    {
        if (textDisplay != null)
        {
            textDisplay.text = word.text; // กำหนดข้อความที่จะแสดง
        }
    }

    public void SetWord(string word) {
        if (textDisplay != null) 
        {
            textDisplay.text = word;
            // อัปเดตสีตั้งต้นใหม่ เผื่อกรณีสลับไปใช้ UI Text
            originalColor = textDisplay.color; 
        }
    }

    public void RemoveLetter() {
        if (textDisplay == null) return;

        textDisplay.text = textDisplay.text.Remove(0, 1);
        FlashColor(Color.green);
    }

    public void DestroyEnemy() 
    {
        // 1. ตรวจสอบว่าเป็น KillMe หรือไม่ ก่อนจะถูกทำลาย
        EnemyMovement moveScript = GetComponentInParent<EnemyMovement>();
        if (moveScript != null && moveScript.type == EnemyMovement.EnemyType.KillMe)
        {
            WordSpawner spawner = FindObjectOfType<WordSpawner>();
            if (spawner != null)
            {
                // ✅ สั่งให้เกิดตัวเล็กทันทีที่ตำแหน่งนี้
                spawner.SpawnMinionAt(transform.position); 
            }
        }

        // 2. ทำลายมอนสเตอร์ทั้งตัว (Parent ของ Canvas)
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FlashRed() {
        FlashColor(Color.red);
    }

    void FlashColor(Color colorToFlash) {
        if (textDisplay == null) return;
        if (colorCoroutine != null) StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(FlashRoutine(colorToFlash));
    }

    IEnumerator FlashRoutine(Color colorToFlash) {
        textDisplay.color = colorToFlash;       
        yield return new WaitForSeconds(0.1f);  
        textDisplay.color = originalColor;      
    }

    // ✅ เพิ่มฟังก์ชันนี้: เมื่อมอนสเตอร์ตัวนี้ถูกทำลาย (ตาย/ชนะ)
    private void OnDestroy()
    {
        EnemyMovement moveScript = GetComponentInParent<EnemyMovement>();
    
        if (moveScript != null && moveScript.type == EnemyMovement.EnemyType.KillMe)
        {
            // เรียก WordSpawner ให้เกิดตัวง่าย 1 ตัวที่ตำแหน่งปัจจุบัน
            WordSpawner spawner = FindObjectOfType<WordSpawner>();
            if (spawner != null)
            {
                spawner.SpawnMinionAt(transform.position); // เรียกตัวเล็กออกมา
            }
        }
        // เช็คว่า TextDisplay ยังอยู่ไหม (กัน Error)
        if (textDisplay != null)
        {
            // เช็คว่า Text นี้เป็น "UI บนหน้าจอ" หรือไม่?
            // (ถ้า Text ไม่ได้เป็นลูกน้องของตัวมอนสเตอร์ แปลว่าเป็น UI ที่เราลากมาใส่)
            if (textDisplay != null && textDisplay.transform.parent != transform)
            {
                textDisplay.text = "";
                textDisplay.gameObject.SetActive(false);
            }
        }
    }
}