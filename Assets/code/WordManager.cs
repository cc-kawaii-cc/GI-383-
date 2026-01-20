using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public List<Word> words = new List<Word>();
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public PlayerHealth playerHealth; 

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
                        // ไม่เจอเป้าหมายใหม่เลย -> นับว่า "พิมพ์ผิดจริง"
                        // (ลงโทษเฉพาะถ้าเป็นบอส)
                        if (activeWord.isBoss)
                        {
                            Debug.Log("พิมพ์ผิดใส่บอส! โดน -20 HP");
                            if(playerHealth != null) playerHealth.TakeDamage(20);
                            activeWord.TriggerWrongTyping();
                        }
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
                
                words.Remove(activeWord);
                activeWord = null;
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