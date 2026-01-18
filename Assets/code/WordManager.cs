using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public List<Word> words = new List<Word>();
    public GameObject bulletPrefab;
    public Transform shootPoint;
    private Word activeWord;

    void Update() {
        if (string.IsNullOrEmpty(Input.inputString)) return; // ถ้าไม่ได้กดอะไรเลยให้ข้ามไป

        foreach (char letter in Input.inputString) {
            if (activeWord != null) {
                if (activeWord.GetNextLetter() == letter) {
                    activeWord.TypeLetter();
                    Shoot();
                }
            } else {
                // ค้นหาคำที่มีอยู่
                for (int i = 0; i < words.Count; i++) {
                    if (words[i].GetNextLetter() == letter) {
                        activeWord = words[i];
                        activeWord.TypeLetter();
                        Shoot();
                        break;
                    }
                }
            }

            // ตรวจสอบว่าพิมพ์จบคำหรือยัง
            if (activeWord != null && activeWord.WordTyped()) {
                activeWord.GetEnemyTransform().GetComponent<WordDisplay>().DestroyEnemy(); // สั่งผีตาย
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