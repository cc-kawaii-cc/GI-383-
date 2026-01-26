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

    [Header("Boss Jumpscare Settings")]
    [Tooltip("จำนวนครั้งที่พิมพ์ผิดได้ฟรีๆ โดยไม่คิดเปอร์เซ็นต์ (กันคนมือลั่น)")]
    public int safeMistakes = 3; //  แนะนำตั้งไว้ 5-10 
    
    [Range(0, 100)] public float startChance = 0f;  
    [Range(0, 100)] public float chanceStep = 5f;   //  ปรับลดลงเหลือ 5% พอผิดครบโควต้าค่อยๆ ขึ้นทีละนิด
    
    private float currentJumpscareChance;
    private int currentMistakeCount = 0; //  ตัวนับจำนวนครั้งที่ผิด

    void Start()
    {
        currentJumpscareChance = startChance;
        currentMistakeCount = 0;
    }

    void Update()
{
    CleanUpDeadWords();

    string input = Input.inputString;
    if (string.IsNullOrEmpty(input)) return;

    foreach (char letter in input)
    {
        // 1. ตรวจสอบว่าเป้าหมายเดิมยังอยู่ไหม
        if (activeWord != null && activeWord.GetEnemyTransform() == null) activeWord = null;

        if (activeWord != null)
        {
            if (activeWord.GetNextLetter() == letter)
            {
                // --- กรณีพิมพ์ถูก ---
                activeWord.TypeLetter();
                Shoot();
                
                // (Optional) ถ้าอยากให้พิมพ์ถูกแล้วลดความเสี่ยง
                // if (currentMistakeCount > 0) currentMistakeCount--; 
            }
            else
            {
                // --- กรณีพิมพ์ผิด ---
                HandleWrongInput(); // แยกออกมาเป็นฟังก์ชันด้านล่างเพื่อไม่ให้โค้ดรก
            }
        }
        else
        {
            // --- กรณีไม่มีเป้าหมาย (หาตัวใหม่) ---
            activeWord = TryFindNewTarget(letter);
            if (activeWord != null)
            {
                activeWord.TypeLetter();
                Shoot();
            }
        }

        // --- ส่วนเช็คเลือด / เช็คตาย (ทำหลังจากการพิมพ์ทุกลูกอักษร) ---
        if (activeWord != null && activeWord.WordTyped())
        {
            HandleEnemyDefeat(); // แยกออกมาเพื่อความสะอาดของโค้ด
        }
    }
}

// --- ฟังก์ชันเสริมที่แยกออกมา (เอาไปวางนอก Update ใน Class เดียวกัน) ---

void HandleWrongInput()
{
    activeWord.ResetWord(); 
    if (activeWord.GetEnemyTransform() != null)
    {
        activeWord.GetEnemyTransform().GetComponent<WordDisplay>().SetWord(activeWord.text);
    }
    
    // Logic พิเศษสำหรับบอส
    if (activeWord.isBoss) 
    {
        if (playerHealth != null) playerHealth.TakeDamage(10); 

        currentMistakeCount++;

        if (currentMistakeCount > safeMistakes)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < currentJumpscareChance)
            {
                if (activeWord.GetEnemyTransform() != null)
                {
                    EnemyMovement bossMove = activeWord.GetEnemyTransform().GetComponent<EnemyMovement>();
                    if (bossMove != null) bossMove.TriggerBossJumpscare();
                }
                currentJumpscareChance = startChance;
                currentMistakeCount = 0; 
            }
            else
            {
                currentJumpscareChance += chanceStep;
                if (currentJumpscareChance > 100f) currentJumpscareChance = 100f;
            }
        }
    }
    
    activeWord.TriggerWrongTyping(); 
}

void HandleEnemyDefeat()
{
    activeWord.hp--; 

    if (activeWord.hp > 0)
    {
        // กรณีศัตรูมีหลาย HP (เช่นตัว Hard) ให้รีเซ็ตคำให้พิมพ์ใหม่
        activeWord.ResetWord(); 
        if (activeWord.GetEnemyTransform() != null)
            activeWord.GetEnemyTransform().GetComponent<WordDisplay>().SetWord(activeWord.text);
        activeWord = null; // ปลดล็อคเป้าหมายเพื่อให้เลือกตัวอื่นได้ หรือต้องกดตัวแรกใหม่
    }
    else
    {
        // กรณีตายจริง
        if (activeWord.GetEnemyTransform() != null)
        {
            EnemyMovement em = activeWord.GetEnemyTransform().GetComponent<EnemyMovement>();
            if (em != null) em.OnDeath(); 
            
            WordDisplay display = activeWord.GetEnemyTransform().GetComponentInChildren<WordDisplay>();
            if (display != null) display.DestroyEnemy(); 
        }
        
        if (deathVFXPrefab != null && activeWord.GetEnemyTransform() != null)
            Instantiate(deathVFXPrefab, activeWord.GetEnemyTransform().position, Quaternion.identity);
        
        words.Remove(activeWord);
        activeWord = null;
    }
}
    
    // ... (ฟังก์ชันอื่นๆ เหมือนเดิม) ...
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