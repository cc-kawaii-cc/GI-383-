using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordSpawner : MonoBehaviour
{
    [Header(" Developer Mode")]
    [Tooltip("ติ๊กถูกเพื่อทดสอบบอสทันที (ข้ามเล่นปกติ)")]
    public bool testBossMode = false; 

    [Header(" Boss Settings")]
    public GameObject bossPrefab;         
    [TextArea(3, 10)] 
    public string bossWord = "นะโมพุทธายะ สังคะโต อะระหัง (พิมพ์ยาวๆเพื่อปราบ)"; 
    
    //  เพิ่มช่องนี้สำหรับลาก Text บนหน้าจอมาใส่
    [Header(" Boss UI Reference")]
    public TextMeshProUGUI bossUIText; 

    [Header(" Enemy Prefabs")]
    public GameObject smallEnemyPrefab;   
    public GameObject mediumEnemyPrefab;  
    public GameObject bigEnemyPrefab;     

    [Header(" Word Banks (แยกความยาก)")]
    public List<string> easyWords = new List<string>() { "กา", "ไก่", "งู", "ปู", "มด" };
    public List<string> mediumWords = new List<string>() { "วิญญาณ", "ความตาย", "ปีศาจ", "อาฆาต" };
    public List<string> hardWords = new List<string>() { "สัมภเวสี", "กุศลผลบุญ", "อสุรกาย", "มหานคร" };

    [Header(" References")]
    public WordManager wordManager;
    public Transform[] spawnPoints; 

    [Header(" Spawn Settings")]
    public float spawnDelay = 2f;
    [Range(5f, 20f)] public float spawnRadius = 12f; 
    
    // Internal Variables
    private float nextSpawnTime = 0f;
    private bool isBossActive = false;
    private float timeSinceStart = 0f; 

    private int chanceSmall = 100;
    private int chanceMedium = 0;
    private int chanceBig = 0;
    
    private Transform playerTransform; 

    void Start()
    {
        timeSinceStart = Time.time; 

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // แก้ไข: ให้แน่ใจว่าปิดตัว Object ทั้งยวง ไม่ใช่แค่ตัวหนังสือ
        if (bossUIText != null) 
        {
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
        if (time < 15f) { SetEnemyTypeChance(100, 0, 0); spawnDelay = 5.0f; }
        else if (time >= 15f && time < 30f) { SetEnemyTypeChance(70, 30, 0); spawnDelay = 4f; }
        else if (time >= 30f && time < 60f) { SetEnemyTypeChance(50, 50, 0); spawnDelay = 3f; }
        else if (time >= 60f && time < 90f) { SetEnemyTypeChance(40, 40, 20); spawnDelay = 2f; }
        else if (time >= 90f) { SpawnBoss(); }
    }

    void SpawnEnemy()
    {
        GameObject prefabToSpawn = smallEnemyPrefab;
        List<string> selectedWordBank = easyWords; 

        int roll = Random.Range(0, 100); 
        
        if (roll < chanceSmall) { prefabToSpawn = smallEnemyPrefab; selectedWordBank = easyWords; }
        else if (roll < chanceSmall + chanceMedium) { prefabToSpawn = mediumEnemyPrefab; selectedWordBank = mediumWords; }
        else { prefabToSpawn = bigEnemyPrefab; selectedWordBank = hardWords; }

        if(prefabToSpawn == null) prefabToSpawn = smallEnemyPrefab;

        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject enemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        string word = "Test";
        if (selectedWordBank.Count > 0) word = selectedWordBank[Random.Range(0, selectedWordBank.Count)];

        bool isSpecial = word.EndsWith("*");
        if (isSpecial) word = word.Replace("*", "");

        WordDisplay display = enemyObj.GetComponentInChildren<WordDisplay>();
        wordManager.AddWord(new Word(word, display, enemyObj.transform, isSpecial, false));
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
    
    public void SetEnemyTypeChance(int small, int med, int big) { chanceSmall = small; chanceMedium = med; chanceBig = big; }
    Vector3 GetRandomSpawnPosition() {
        if (playerTransform == null) return transform.position; 
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        return playerTransform.position + (Vector3)(randomDirection * spawnRadius);
    }
    void OnDrawGizmosSelected() {
        Vector3 center = transform.position;
        if (Application.isPlaying && GameObject.FindGameObjectWithTag("Player") != null)
            center = GameObject.FindGameObjectWithTag("Player").transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}