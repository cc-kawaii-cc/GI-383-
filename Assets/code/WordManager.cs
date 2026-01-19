using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public List<Word> words = new List<Word>();
    public GameObject bulletPrefab;
    public Transform shootPoint;
    
    // ✅ เพิ่ม Reference ไปหา PlayerHealth
    public PlayerHealth playerHealth; 

    private Word activeWord;

    void Update()
    {
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char letter in input)
        {
            if (activeWord != null)
            {
                // --- กรณีมีเป้าหมายแล้ว ---
                if (activeWord.GetNextLetter() == letter)
                {
                    // ✅ พิมพ์ถูก
                    activeWord.TypeLetter();
                    Shoot();
                }
                else
                {
                    // ❌ พิมพ์ผิด!!
                    if (activeWord.isBoss)
                    {
                        // ถ้าเป็นบอส -> โดนดาเมจ 20 + ตัวอักษรแดง
                        Debug.Log("พิมพ์ผิดใส่บอส! โดน -20 HP");
                        if(playerHealth != null) playerHealth.TakeDamage(20);
                        activeWord.TriggerWrongTyping();
                    }
                }
            }
            else
            {
                // --- กรณีหาเป้าหมายใหม่ ---
                foreach (Word word in words)
                {
                    if (word.GetNextLetter() == letter)
                    {
                        activeWord = word;
                        activeWord.TypeLetter();
                        Shoot();
                        break;
                    }
                }
            }

            // เช็คว่าพิมพ์จบหรือยัง
            if (activeWord != null && activeWord.WordTyped())
            {
                if (activeWord.isSpecial)
                {
                    ProgressManager pm = FindObjectOfType<ProgressManager>();
                    if (pm != null) pm.AddProgress();
                }
            
                activeWord.GetEnemyTransform().GetComponent<WordDisplay>().DestroyEnemy();
                words.Remove(activeWord);
                activeWord = null;
            }
        }
    }

    void Shoot() {
        GameObject b = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        b.GetComponent<Bullet>().Seek(activeWord.GetEnemyTransform());
    }

    public void AddWord(Word word) => words.Add(word);
}