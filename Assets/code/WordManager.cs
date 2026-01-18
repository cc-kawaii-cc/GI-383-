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
        // รับค่าปุ่มที่กดมาทั้งหมดใน Frame นั้น
        string input = Input.inputString;

        if (string.IsNullOrEmpty(input)) return;

        foreach (char letter in input)
        {
            if (activeWord != null)
            {
                // ตรวจสอบตัวถัดไป (รวมถึงสระและวรรณยุกต์)
                if (activeWord.GetNextLetter() == letter)
                {
                    activeWord.TypeLetter();
                    Shoot();
                }
            }
            else
            {
                // ถ้ายังไม่มีคำที่พิมอยู่ ให้หาคำที่ขึ้นต้นด้วยตัวนั้น
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

            // เช็คว่าพิมครบคำหรือยัง
            if (activeWord != null && activeWord.WordTyped())
            {
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