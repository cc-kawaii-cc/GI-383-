using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordSpawner : MonoBehaviour
{
    [Header("Developer Mode")] 
    [Tooltip("ติ๊กถูกเพื่อทดสอบบอสทันที (ข้ามเล่นปกติ)")]
    public bool testBossMode = false;

    [Header("Boss Settings")] 
    public GameObject bossPrefab;
    [TextArea(3, 10)] public string bossWord = "นะโมพุทธายะ สังคะโต อะระหัง (พิมพ์ยาวๆเพื่อปราบ)";
    [Header("Boss UI Reference")] 
    public TextMeshProUGUI bossUIText;

    [Header("Enemy Prefabs")] 
    public GameObject smallEnemyPrefab;
    public GameObject mediumEnemyPrefab;
    public GameObject bigEnemyPrefab;
    public GameObject ghostMomPrefab;
    public GameObject killMePrefab;
    public GameObject spitterPrefab;
    public GameObject thaiMusicGhostPrefab;

    [Header("Word Banks (แยกความยาก)")] 
    public List<string> easyWords = new List<string>() { "กา", "ไก่", "งู", "ปู", "มด" };
    public List<string> mediumWords = new List<string>() { "วิญญาณ", "ความตาย", "ปีศาจ", "อาฆาต" };
    public List<string> hardWords = new List<string>() { "สัมภเวสี", "กุศลผลบุญ", "อสุรกาย", "มหานคร" };

    [Header("References")] 
    public WordManager wordManager;
    public Transform[] spawnPoints;

    [Header("Spawn Settings")] 
    public float spawnDelay = 2f;
    [Range(5f, 20f)] public float spawnRadius = 12f;
    
    [Header("Square Spawn Settings")]
    public float spawnWidth = 20f;  // ความกว้างของพื้นที่เกิด (แกน X)
    public float spawnHeight = 10f; // ความสูงของพื้นที่เกิด (แกน Y)
    
    [Header("Limit Settings")]
    public int maxEnemies = 15; // กำหนดจำนวนมอนสเตอร์สูงสุดในแมพ
    [Header("Camera Spawn Settings")]
    [Tooltip("ระยะห่างจากขอบจอออกไป (ยิ่งเยอะยิ่งไกล)")]
    public float spawnMargin = 2f;

    
    // Internal Variables
    private float nextSpawnTime = 0f;
    private bool isBossActive = false;
    private float timeSinceStart = 0f;
    
    // Flag สำหรับเช็คว่ากำลังรอเคลียร์มอนสเตอร์ให้หมดก่อนเจอบอส
    private bool isWaitingForClear = false; 

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

        if (bossUIText != null)
        {
            bossUIText.text = ""; 
            bossUIText.gameObject.SetActive(false);
        }

        if (testBossMode) SpawnBoss();
    }

    void Update()
    {
        if (isBossActive || testBossMode) return;
        if (isWaitingForClear)
        {
            if (wordManager.words.Count == 0)
            {
                SpawnBoss();
                isWaitingForClear = false;
            }
            return;
        }

        // ✅ เพิ่มการเช็ค: ถ้ามอนสเตอร์ในแมพถึงขีดจำกัด ให้หยุดเกิดชั่วคราว
        if (wordManager.words.Count >= maxEnemies) 
        {
            // อัปเดต nextSpawnTime ไว้เรื่อยๆ เพื่อไม่ให้มันเกิดทันทีที่มอนสเตอร์ลดลง (ป้องกันการ Overload)
            nextSpawnTime = Time.time + spawnDelay;
            return; 
        }

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
        // --- ปรับปรุง Rate การเกิดตาม Feedback (30 วิแรกต้องโหดขึ้น) ---

        if (time < 10f) 
        { 
            // 0-10 วิ: อุ่นเครื่อง (เริ่มมี Medium นิดหน่อย)
            SetEnemyTypeChance(small:80, med:10, big:10, mom:0, kill:0, spit:0, thai:0);
            spawnDelay = Random.Range(2.0f, 3.0f); 
        }
        else if (time >= 10f && time < 30f) 
        { 
            // 10-30 วิ: เริ่มปล่อยของ (Medium เยอะขึ้น, เริ่มมี Big/Hard)
            SetEnemyTypeChance(small:30, med:30, big:10, mom:10, kill:10, spit:5, thai:5); 
            spawnDelay = Random.Range(1.5f, 2.5f); 
        }
        else if (time >= 30f && time < 60f) 
        { 
            // 30-60 วิ: ความยากระดับกลาง (เพิ่ม Spitter/Thai)
            SetEnemyTypeChance(small:20, med:30, big:20, mom:5, kill:10, spit:10, thai:5); 
            spawnDelay = Random.Range(1.5f, 2.0f); 
        }
        else if (time >= 60f && time < 180f)
        {
            // 1 นาทีขึ้นไป: จัดเต็ม (ลดความเร็ว Spawn ลงนิดหน่อยเพื่อให้ผู้เล่นหายใจตามบรีฟ)
            SetEnemyTypeChance(small:10, med:20, big:30, mom:10, kill:10, spit:10, thai:10); 
            spawnDelay = Random.Range(1.8f, 2.2f); // ปรับให้ช้าลงนิดนึงช่วงกลางเกม
        }
        else if (time >= 180f) // ครบ 3 นาที
        { 
            // เข้าสู่โหมด "รอเคลียร์จอ" (ยังไม่เสกบอสทันที)
            isWaitingForClear = true;
        }
    }

    void SpawnEnemy()
    {
        GameObject prefabToSpawn = smallEnemyPrefab;
        List<string> selectedWordBank = easyWords; 

        int roll = Random.Range(0, 100); 
    
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

        string word = GetUniqueWord(selectedWordBank); 

        WordDisplay display = enemyObj.GetComponentInChildren<WordDisplay>();
        if (display != null)
        {
            wordManager.AddWord(new Word(word, display, enemyObj.transform, false, false));
        }
    }

    public void SpawnBoss()
    {
        if (isBossActive && !testBossMode) return; 

        Debug.Log("BOSS BATTLE START!");
        isBossActive = true;
        
        // ** ลบ ClearAllEnemies() ออกแล้ว ** // เพราะเราเคลียร์ไปตั้งแต่ช่วง isWaitingForClear แล้ว

        if (bossPrefab != null)
        {
            Vector3 bossPos = GetRandomSpawnPosition();
            GameObject bossObj = Instantiate(bossPrefab, bossPos, Quaternion.identity);
            WordDisplay display = bossObj.GetComponentInChildren<WordDisplay>();

            if (bossUIText != null && display != null)
            {
                bossUIText.gameObject.SetActive(true);
                if (display.textDisplay != null && display.textDisplay != bossUIText)
                {
                    display.textDisplay.gameObject.SetActive(false);
                }
                display.textDisplay = bossUIText;
            }

            Word newWord = new Word(bossWord, display, bossObj.transform, true, true);
            wordManager.AddWord(newWord);
            display.Setup(newWord); 
        }
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

    // ... (ส่วน Import และตัวแปรอื่นๆ เหมือนเดิม)

    // ✅ แก้ไขฟังก์ชันนี้
    Vector3 GetRandomSpawnPosition()
    {
        if (playerTransform == null) return transform.position;

        Camera cam = Camera.main;
        if (cam == null) return transform.position;

        float screenHeight = 2f * cam.orthographicSize;
        float screenWidth = screenHeight * cam.aspect;

        int side = Random.Range(0, 4);
        Vector3 spawnOffset = Vector3.zero;

        switch (side)
        {
            case 0: // บน
                spawnOffset = new Vector3(Random.Range(-screenWidth / 2, screenWidth / 2), (screenHeight / 2) + spawnMargin, 0);
                break;
            case 1: // ล่าง
                spawnOffset = new Vector3(Random.Range(-screenWidth / 2, screenWidth / 2), -(screenHeight / 2) - spawnMargin, 0);
                break;
            case 2: // ซ้าย
                spawnOffset = new Vector3(-(screenWidth / 2) - spawnMargin, Random.Range(-screenHeight / 2, screenHeight / 2), 0);
                break;
            case 3: // ขวา
                spawnOffset = new Vector3((screenWidth / 2) + spawnMargin, Random.Range(-screenHeight / 2, screenHeight / 2), 0);
                break;
        }

        Vector3 finalPos = cam.transform.position + spawnOffset;
        finalPos.z = 0;
        return finalPos;
    }
    
// ... (ส่วนอื่นๆ เหมือนเดิม)
    void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // คำนวณขนาดจอ
        float screenHeight = 2f * cam.orthographicSize;
        float screenWidth = screenHeight * cam.aspect;

        Vector3 camPos = cam.transform.position;
        camPos.z = 0; // ให้เส้นอยู่ในระนาบ 2D เดียวกับเกม

        // 1. วาดกรอบสีเขียว = ขอบจอที่ผู้เล่นมองเห็น
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(camPos, new Vector3(screenWidth, screenHeight, 0));

        // 2. วาดกรอบสีแดง = จุดที่มอนสเตอร์จะเกิด (Spawn Boundary)
        Gizmos.color = Color.red;
        float outerWidth = screenWidth + (spawnMargin * 2);
        float outerHeight = screenHeight + (spawnMargin * 2);
        Gizmos.DrawWireCube(camPos, new Vector3(outerWidth, outerHeight, 0));
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
        List<string> availableWords = new List<string>(wordBank);

        while (availableWords.Count > 0)
        {
            int randomIndex = Random.Range(0, availableWords.Count);
            string selectedWord = availableWords[randomIndex];
            bool isFirstLetterDuplicate = false;

            foreach (var activeWord in wordManager.words)
            {
                if (activeWord.text.Length > 0 && selectedWord.Length > 0)
                {
                    if (char.ToLower(activeWord.text[0]) == char.ToLower(selectedWord[0]))
                    {
                        isFirstLetterDuplicate = true;
                        break;
                    }
                }
            }

            if (!isFirstLetterDuplicate) return selectedWord;
            else availableWords.RemoveAt(randomIndex);
        }
        return wordBank[Random.Range(0, wordBank.Count)];
    }
}