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
                else
                {
                    if (activeWord.isBoss)
                    {
                        if(playerHealth != null) playerHealth.TakeDamage(20);
                        activeWord.TriggerWrongTyping();
                    }
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

            if (activeWord != null && activeWord.WordTyped())
            {
                // เช็คชนะ: ถ้าตัวที่พิมพ์จบคือ "Boss" -> ชนะทันที
                if (activeWord.isBoss)
                {
                    if (GameManager.instance != null) GameManager.instance.Victory();
                }

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