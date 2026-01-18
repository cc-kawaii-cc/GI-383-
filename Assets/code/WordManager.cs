using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public List<Word> words = new List<Word>();
    public GameObject bulletPrefab;
    public Transform shootPoint;
    private Word activeWord;

    void Update()
    {
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char letter in input)
        {
            if (activeWord != null)
            {
                if (activeWord.GetNextLetter() == letter)
                {
                    activeWord.TypeLetter();
                    Shoot();
                }
            }
            else
            {
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

            // --- จุดที่ต้องแก้ไข: เช็คพิมครบถ้วนที่นี่ ---
            if (activeWord != null && activeWord.WordTyped())
            {
                // ตรวจสอบว่าคำพิเศษหรือไม่
                if (activeWord.isSpecial)
                {
                    // ตรวจสอบก่อนว่ามี ProgressManager ใน Scene หรือไม่เพื่อป้องกัน Error
                    ProgressManager pm = FindObjectOfType<ProgressManager>();
                    if (pm != null) pm.AddProgress();
                }
            
                // ทำลายผี
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