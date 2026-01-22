using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public List<Word> words = new List<Word>();
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public PlayerHealth playerHealth; 
    public GameObject deathVFXPrefab;

    private Word activeWord;

    void Update()
    {
        // 1. ล้างศัตรูที่ตายแล้วออกจาก List (ป้องกัน Error)
        CleanUpDeadWords();

        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char letter in input)
        {
            // เช็คกันเหนียว: ถ้า activeWord มีอยู่ แต่ตัวศัตรูหายไปแล้ว ให้ปลดล็อคทันที
            if (activeWord != null && activeWord.GetEnemyTransform() == null)
            {
                activeWord = null;
            }

            if (activeWord != null)
            {
                // --- กรณีมีเป้าหมายแล้ว ---
                if (activeWord.GetNextLetter() == letter)
                {
                    // พิมพ์ถูก (เป้าหมายเดิม)
                    activeWord.TypeLetter();
                    Shoot();
                }
                else
                {
                    // พิมพ์ไม่ตรงเป้าหมายเดิม -> ลองหาเป้าหมายใหม่ (Switch Target)
                    Word newTarget = TryFindNewTarget(letter);
                    
                    if (newTarget != null)
                    {
                        // เจอกรณี "เปลี่ยนใจ" -> ย้ายเป้าหมายทันที
                        Debug.Log($"Switching target to: {newTarget.text}");
                        activeWord = newTarget;
                        activeWord.TypeLetter(); // พิมพ์ตัวแรกของคำใหม่เลย
                        Shoot();
                    }
                    else
                    {
                        // --- แก้ไขจุดนี้: ถ้าพิมพ์ผิดให้ Reset ทันที ---
                    
                        // 1. สั่ง Reset ตำแหน่งการพิมพ์ใน Word.cs
                        activeWord.ResetWord(); 

                        // 2. สั่งให้ WordDisplay คืนค่าตัวหนังสือเต็มคำ
                        if (activeWord.GetEnemyTransform() != null)
                        {
                            activeWord.GetEnemyTransform().GetComponent<WordDisplay>().SetWord(activeWord.text);
                        }

                        // 3. ลงโทษ/แสดงผล (จอแดง)
                        if (activeWord.isBoss)
                        {
                            if(playerHealth != null) playerHealth.TakeDamage(10); 
                        }
                        activeWord.TriggerWrongTyping(); 
                    
                        Debug.Log("พิมพ์ผิด! ระบบ Reset คำศัพท์ให้เริ่มใหม่");
                    }
                
                }
            }
            else
            {
                // --- กรณีหาเป้าหมายใหม่ (ไม่มีเป้า) ---
                activeWord = TryFindNewTarget(letter);
                if (activeWord != null)
                {
                    activeWord.TypeLetter();
                    Shoot();
                }
            }

            // เช็คว่าพิมพ์จบหรือยัง
            if (activeWord != null && activeWord.WordTyped())
            {
                activeWord.hp--; // ลด HP ของคำลง

                if (activeWord.hp > 0)
                {
                    // กรณี HP ยังไม่หมด (เช่น Hard Word รอบแรก)
                    activeWord.ResetWord(); // รีเซ็ตตัวชี้ตำแหน่งการพิมพ์

                    // สั่งให้ Display แสดงคำเต็มใหม่อีกครั้ง
                    if (activeWord.GetEnemyTransform() != null)
                    {
                        activeWord.GetEnemyTransform().GetComponent<WordDisplay>().SetWord(activeWord.text);
                    }

                    activeWord = null; // ปลดล็อคเพื่อให้เลือกเป้าหมายใหม่ได้
                }
                else
                {
                    // กรณี HP หมดแล้ว (ตายจริง)
                    if (activeWord.isBoss)
                    {
                        if (GameManager.instance != null) GameManager.instance.Victory();
                    }

                    if (activeWord.isSpecial)
                    {
                        ProgressManager pm = FindObjectOfType<ProgressManager>();
                        if (pm != null) pm.AddProgress();
                    }

                    if (activeWord.GetEnemyTransform() != null)
                    {
                        activeWord.GetEnemyTransform().GetComponent<WordDisplay>().DestroyEnemy();
                    }
                    if (deathVFXPrefab != null && activeWord.GetEnemyTransform() != null)
                    {
                        Instantiate(deathVFXPrefab, activeWord.GetEnemyTransform().position, Quaternion.identity);
                    }

                    // ทำลายศัตรู
                    if (activeWord.GetEnemyTransform() != null)
                    {
                        activeWord.GetEnemyTransform().GetComponent<WordDisplay>().DestroyEnemy();
                    }

                    words.Remove(activeWord);
                    activeWord = null;
                }
            }
        }
    }
    
    Word TryFindNewTarget(char letter)
    {
        foreach (Word word in words)
        {
            if (word == activeWord) continue;
            if (word.GetEnemyTransform() != null && word.GetNextLetter() == letter)
            {
                return word;
            }
        }
        return null;
    }

    void Shoot() {
        if (activeWord == null || activeWord.GetEnemyTransform() == null) return;

        GameObject b = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        b.GetComponent<Bullet>().Seek(activeWord.GetEnemyTransform());
    }

    public void AddWord(Word word) => words.Add(word);

    void CleanUpDeadWords()
    {
        for (int i = words.Count - 1; i >= 0; i--)
        {
            if (words[i].GetEnemyTransform() == null)
            {
                words.RemoveAt(i);
            }
        }
    }
}