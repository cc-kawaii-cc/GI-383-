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
        CleanUpDeadWords();

        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char letter in input)
        {
            if (activeWord != null && activeWord.GetEnemyTransform() == null) activeWord = null;

            if (activeWord != null)
            {
                if (activeWord.GetNextLetter() == letter)
                {
                    activeWord.TypeLetter();
                    Shoot();
                }
                else
                {
                    Word newTarget = TryFindNewTarget(letter);
                    if (newTarget != null)
                    {
                        activeWord = newTarget;
                        activeWord.TypeLetter();
                        Shoot();
                    }
                    else
                    {
                        // พิมพ์ผิด -> Reset
                        activeWord.ResetWord(); 
                        if (activeWord.GetEnemyTransform() != null)
                        {
                            activeWord.GetEnemyTransform().GetComponent<WordDisplay>().SetWord(activeWord.text);
                        }
                        if (activeWord.isBoss && playerHealth != null) playerHealth.TakeDamage(10); 
                        activeWord.TriggerWrongTyping(); 
                    }
                }
            }
            else
            {
                activeWord = TryFindNewTarget(letter);
                if (activeWord != null)
                {
                    activeWord.TypeLetter();
                    Shoot();
                }
            }

            // เช็คว่าตายหรือยัง
            if (activeWord != null && activeWord.WordTyped())
            {
                activeWord.hp--; 

                if (activeWord.hp > 0)
                {
                    activeWord.ResetWord(); 
                    if (activeWord.GetEnemyTransform() != null)
                    {
                        activeWord.GetEnemyTransform().GetComponent<WordDisplay>().SetWord(activeWord.text);
                    }
                    activeWord = null; 
                }
                else
                {
                    // --- ตายจริง (HP=0) ---
                    
                    if (activeWord.isBoss && GameManager.instance != null) GameManager.instance.Victory();

                    if (activeWord.isSpecial)
                    {
                        ProgressManager pm = FindObjectOfType<ProgressManager>();
                        if (pm != null) pm.AddProgress();
                    }

                    if (activeWord.GetEnemyTransform() != null)
                    {
                        // --- เพิ่ม Logic: ตรวจสอบว่าเป็น Splitter หรือไม่ ---
                        EnemyMovement em = activeWord.GetEnemyTransform().GetComponent<EnemyMovement>();
                        if (em != null && em.type == EnemyMovement.EnemyType.Splitter)
                        {
                            // ถ้าเป็น Splitter ให้เสกลูกน้อง 2 ตัวตรงจุดตาย
                            WordSpawner spawner = FindObjectOfType<WordSpawner>();
                            if (spawner != null)
                            {
                                spawner.SpawnMinionAt(activeWord.GetEnemyTransform().position);
                                spawner.SpawnMinionAt(activeWord.GetEnemyTransform().position);
                            }
                        }
                        // ------------------------------------------------

                        WordDisplay display = activeWord.GetEnemyTransform().GetComponentInChildren<WordDisplay>();
                        if (display != null)
                        {
                            display.DestroyEnemy(); // ตัวนี้จะไปรัน Logic เกิดตัวเล็กให้เอง
                        }
                    }
                    
                    if (deathVFXPrefab != null && activeWord.GetEnemyTransform() != null)
                    {
                        Instantiate(deathVFXPrefab, activeWord.GetEnemyTransform().position, Quaternion.identity);
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
            if (word.GetEnemyTransform() != null && word.GetNextLetter() == letter) return word;
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
            if (words[i].GetEnemyTransform() == null) words.RemoveAt(i);
        }
    }
}