using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordSpawner : MonoBehaviour
{
    [Header(" Developer Mode")] [Tooltip("ติ๊กถูกเพื่อทดสอบบอสทันที (ข้ามเล่นปกติ)")]
    public bool testBossMode = false;

    [Header(" Boss Settings")] public GameObject bossPrefab;
    [TextArea(3, 10)] public string bossWord = "นะโมพุทธายะ สังคะโต อะระหัง (พิมพ์ยาวๆเพื่อปราบ)";

    //  เพิ่มช่องนี้สำหรับลาก Text บนหน้าจอมาใส่
    [Header(" Boss UI Reference")] public TextMeshProUGUI bossUIText;

    [Header(" Enemy Prefabs")] 
    public GameObject smallEnemyPrefab;
    public GameObject mediumEnemyPrefab;
    public GameObject bigEnemyPrefab;
    public GameObject ghostMomPrefab;
    public GameObject killMePrefab;
    public GameObject spitterPrefab;
    public GameObject thaiMusicGhostPrefab;

    [Header(" Word Banks (แยกความยาก)")] public List<string> easyWords = new List<string>()
        { "กา", "ไก่", "งู", "ปู", "มด" };

    public List<string> mediumWords = new List<string>() { "วิญญาณ", "ความตาย", "ปีศาจ", "อาฆาต" };
    public List<string> hardWords = new List<string>() { "สัมภเวสี", "กุศลผลบุญ", "อสุรกาย", "มหานคร" };

    [Header(" References")] public WordManager wordManager;
    public Transform[] spawnPoints;

    [Header(" Spawn Settings")] public float spawnDelay = 2f;
    [Range(5f, 20f)] public float spawnRadius = 12f;

    // Internal Variables
    private float nextSpawnTime = 0f;
    private bool isBossActive = false;
    private float timeSinceStart = 0f;

    private int chanceSmall = 100;
    private int chanceMedium = 0;
    private int chanceBig = 0;
    private int chanceGhostMom = 0;
    private int chancekillme= 0;
    private int chanceSpitter = 0;
    private int chanceThai = 0;
    private Transform playerTransform;

    void Start()
    {
        timeSinceStart = Time.time;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // แก้ไข: ให้แน่ใจว่าปิดตัว Object ทั้งยวง ไม่ใช่แค่ตัวหนังสือ
        if (bossUIText != null)
        {
            bossUIText.text = ""; // ล้างข้อความ "BossTextUI" ออก
            bossUIText.gameObject.SetActive(false);
        }

        if (testBossMode) SpawnBoss();
    }

    void Update()
    {
        if (isBossActive || testBossMode) return;

        float timeAlive = Time.time - timeSinceStart;
        UpdateGamePhase(timeAlive);

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnDelay;
        }
    }

    void UpdateGamePhase(float time)
    {
        if (time < 5f) 
        { 
            SetEnemyTypeChance(0, 0, 0, 0,kill:0,spit:0,thai:100);
            spawnDelay = Random.Range(1.0f, 4.0f); 
        }
        else if (time >= 5f && time < 15f) 
        { 
            SetEnemyTypeChance(45, 35, 0,0,kill:20,spit:0,thai:0); 
            spawnDelay = Random.Range(1.0f, 3.0f); 
        }
        else if (time >= 15f && time < 30f) 
        { 
            SetEnemyTypeChance(40, 30, 20,0,kill:0,spit:10,thai:0); 
            spawnDelay = Random.Range(1.5f, 2.0f); 
        }
        else if (time >= 30f && time < 180f)
        {
            SetEnemyTypeChance(30, 30, 25, 15,kill:0,spit:0,thai:0); 
            spawnDelay = 1.5f; 
        }
        else if (time >= 180f) 
        { 
            SpawnBoss(); 
        }
    }

    void SpawnEnemy()
    {
        GameObject prefabToSpawn = smallEnemyPrefab;
        List<string> selectedWordBank = easyWords; 

        int roll = Random.Range(0, 100); 
    
        // ตรรกะการสุ่มแบบเรียงลำดับ (Cumulative Probability)
        if (roll < chanceSmall) { prefabToSpawn = smallEnemyPrefab; selectedWordBank = easyWords; }
        else if (roll < (chanceSmall + chanceMedium)) { prefabToSpawn = mediumEnemyPrefab; selectedWordBank = mediumWords; }
        else if (roll < (chanceSmall + chanceMedium + chanceBig)) { prefabToSpawn = bigEnemyPrefab; selectedWordBank = hardWords; }
        else if (roll < (chanceSmall + chanceMedium + chanceBig + chanceGhostMom)) { prefabToSpawn = ghostMomPrefab; selectedWordBank = mediumWords; }
        else if (roll < (chanceSmall + chanceMedium + chanceBig + chanceGhostMom + chancekillme)) { prefabToSpawn = killMePrefab; selectedWordBank = easyWords; }
        else if (roll < (chanceSmall + chanceMedium + chanceBig + chanceGhostMom + chancekillme + chanceSpitter)) { prefabToSpawn = spitterPrefab; selectedWordBank = mediumWords; }
        else { prefabToSpawn = thaiMusicGhostPrefab; selectedWordBank = mediumWords; }
        if (prefabToSpawn == null) return;

        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject enemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // ใช้ระบบป้องกันคำซ้ำที่สร้างไว้
        string word = GetUniqueWord(selectedWordBank); 

        WordDisplay display = enemyObj.GetComponentInChildren<WordDisplay>();
        if (display != null)
        {
            wordManager.AddWord(new Word(word, display, enemyObj.transform, false, false));
        }
    }

    public void SpawnBoss()
    {
        if (isBossActive && !testBossMode) return; // กันการ Spawn ซ้ำ

        Debug.Log("BOSS BATTLE START!");
        isBossActive = true;
        ClearAllEnemies();

        if (bossPrefab != null)
        {
            Vector3 bossPos = GetRandomSpawnPosition();
            GameObject bossObj = Instantiate(bossPrefab, bossPos, Quaternion.identity);

            WordDisplay display = bossObj.GetComponentInChildren<WordDisplay>();

            if (bossUIText != null && display != null)
            {
                // 1. เปิด UI ก่อน
                bossUIText.gameObject.SetActive(true);

                // 2. ซ่อน Text เดิมที่ติดมากับตัว Boss (ถ้ามี)
                if (display.textDisplay != null && display.textDisplay != bossUIText)
                {
                    display.textDisplay.gameObject.SetActive(false);
                }

                // 3. เชื่อมต่อ UI กลางเข้ากับระบบ Word ของ Boss
                display.textDisplay = bossUIText;

                // 4. บังคับให้ WordDisplay รีเฟรชข้อความทันที
                // (สมมติว่าใน WordDisplay มีฟังก์ชันสรุปการแสดงผล)
                // display.SetWord(bossWord); 
            }

            // ส่งข้อมูลให้ WordManager
            Word newWord = new Word(bossWord, display, bossObj.transform, true, true);
            wordManager.AddWord(newWord);

            // สำคัญ: ต้องสั่งให้แสดงผลครั้งแรกทันที
            display.Setup(newWord); // ถ้าคุณมีฟังก์ชัน Setup ใน WordDisplay ให้เรียกตรงนี้
        }
    }

    void ClearAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies) Destroy(e);
    }

    public void SetEnemyTypeChance(int small, int med, int big, int mom , int kill,int spit, int thai) 
    { 
        chanceSmall = small; 
        chanceMedium = med; 
        chanceBig = big; 
        chanceGhostMom = mom; 
        chancekillme = kill;
        chanceSpitter = spit;
        chanceThai = thai;
    }

    Vector3 GetRandomSpawnPosition()
    {
        if (playerTransform == null) return transform.position;
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        return playerTransform.position + (Vector3)(randomDirection * spawnRadius);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        if (Application.isPlaying && GameObject.FindGameObjectWithTag("Player") != null)
            center = GameObject.FindGameObjectWithTag("Player").transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, spawnRadius);

    }
    public void SpawnMinionAt(Vector3 position)
    {
        Vector3 spawnPos = position + (Vector3)Random.insideUnitCircle * 2f; 
        GameObject minion = Instantiate(smallEnemyPrefab, spawnPos, Quaternion.identity);
    
        string word = GetUniqueWord(easyWords);
        WordDisplay display = minion.GetComponentInChildren<WordDisplay>();
        if (display != null)
        {
            wordManager.AddWord(new Word(word, display, minion.transform, false, false));
        }
    }
    string GetUniqueWord(List<string> wordBank)
    {
        if (wordBank == null || wordBank.Count == 0) return "Ghost";

        // ก๊อปปี้รายการคำศัพท์ออกมาเพื่อเตรียมสุ่ม
        List<string> availableWords = new List<string>(wordBank);

        while (availableWords.Count > 0)
        {
            int randomIndex = Random.Range(0, availableWords.Count);
            string selectedWord = availableWords[randomIndex];

            bool isFirstLetterDuplicate = false;

            // เช็คคำทั้งหมดที่อยู่บน Map ตอนนี้
            foreach (var activeWord in wordManager.words)
            {
                // เช็คว่าตัวอักษรตัวแรก (Index 0) ซ้ำกันหรือไม่
                if (activeWord.text.Length > 0 && selectedWord.Length > 0)
                {
                    if (char.ToLower(activeWord.text[0]) == char.ToLower(selectedWord[0]))
                    {
                        isFirstLetterDuplicate = true;
                        break;
                    }
                }
            }

            if (!isFirstLetterDuplicate)
            {
                return selectedWord; // คืนค่าคำที่ตัวแรกไม่ซ้ำกับใครเลยบนจอ
            }
            else
            {
                // ถ้าตัวแรกซ้ำ ให้ลบคำนี้ออกจากรายการสุ่มชั่วคราวในรอบนี้
                availableWords.RemoveAt(randomIndex);
            }
        }

        // กรณีถ้าคำในคลัง "ตัวแรกซ้ำกันหมดแล้วจริงๆ" ให้สุ่มคำที่ตัวแรกไม่ซ้ำกับเป้าหมายปัจจุบัน (ActiveWord)
        return wordBank[Random.Range(0, wordBank.Count)];
    }
}