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
    
    [Tooltip("ใส่ประโยคยาวๆ ที่จะให้บอสพูดตรงนี้")]
    [TextArea(3, 10)] 
    public string bossWord = "นะโมพุทธายะ สังคะโต อะระหัง (พิมพ์ยาวๆเพื่อปราบ)"; 

    [Header(" Enemy Prefabs")]
    public GameObject smallEnemyPrefab;   
    public GameObject mediumEnemyPrefab;  
    public GameObject bigEnemyPrefab;     

    [Header("🔗 References")]
    public WordManager wordManager;
    public List<string> wordBank = new List<string>();
    
    // ไม่ใช้ spawnPoints แบบเดิมแล้ว
    // public Transform[] spawnPoints; 

    [Header("⚡ Spawn Settings")]
    public float spawnDelay = 3f;
    [Range(5f, 20f)] 
    public float spawnRadius = 10f; // ปรับขนาดวงกลมตรงนี้
    
    // Internal Variables
    private float nextSpawnTime = 0f;
    private bool isBossActive = false;
    private int chanceSmall = 100;
    private int chanceMedium = 0;
    private int chanceBig = 0;
    
    private Transform playerTransform; 

    void Start()
    {
        // หาตัว Player อัตโนมัติ
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError(" ไม่เจอ Player! อย่าลืมติด Tag 'Player' ที่ตัวละครนะครับ");
        }

        if (testBossMode)
        {
            SpawnBoss();
        }
    }

    void Update()
    {
        if (isBossActive || testBossMode) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnDelay;
        }
    }

    // ฟังก์ชันวาดเส้น Radius ในหน้าจอ Scene (Gizmos)
    void OnDrawGizmosSelected()
    {
        // ถ้ามี Player ให้วาดรอบ Player, ถ้าไม่มีให้วาดรอบตัว Spawner เอง
        Vector3 center = Vector3.zero;
        
        if (Application.isPlaying && GameObject.FindGameObjectWithTag("Player") != null)
        {
            center = GameObject.FindGameObjectWithTag("Player").transform.position;
        }
        else
        {
            center = transform.position;
        }

        Gizmos.color = Color.yellow; // สีของเส้น
        Gizmos.DrawWireSphere(center, spawnRadius); // วาดเส้นวงกลม
    }

    Vector3 GetRandomSpawnPosition()
    {
        if (playerTransform == null) return transform.position;

        // สุ่มจุดบนขอบวงกลม
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = playerTransform.position + (Vector3)(randomDirection * spawnRadius);
        
        return spawnPos;
    }

    public void SpawnBoss()
    {
        if (isBossActive) return;

        Debug.Log("BOSS BATTLE START!");
        isBossActive = true;
        
        ClearAllEnemies();

        if (bossPrefab != null)
        {
            Vector3 bossPos = GetRandomSpawnPosition();
            GameObject bossObj = Instantiate(bossPrefab, bossPos, Quaternion.identity);
            WordDisplay display = bossObj.GetComponentInChildren<WordDisplay>();
            
            Word newWord = new Word(bossWord, display, bossObj.transform, true, true);
            wordManager.AddWord(newWord);
        }
    }

    void SpawnEnemy()
    {
        GameObject prefabToSpawn = smallEnemyPrefab;
        int roll = Random.Range(0, 100);
        
        if (roll < chanceSmall) prefabToSpawn = smallEnemyPrefab;
        else if (roll < chanceSmall + chanceMedium) prefabToSpawn = mediumEnemyPrefab;
        else prefabToSpawn = bigEnemyPrefab;

        if(prefabToSpawn == null) prefabToSpawn = smallEnemyPrefab;

        // ใช้ตำแหน่งสุ่มจากวงกลม
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject enemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        string word = "";
        if(wordBank.Count > 0) word = wordBank[Random.Range(0, wordBank.Count)];
        else word = "Test";

        bool isSpecial = word.EndsWith("*");
        if (isSpecial) word = word.Replace("*", "");

        WordDisplay display = enemyObj.GetComponentInChildren<WordDisplay>();
        wordManager.AddWord(new Word(word, display, enemyObj.transform, isSpecial, false));
    }

    void ClearAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies) Destroy(e);
    }

    public void SetSpawnRate(float delay) => spawnDelay = delay;
    
    public void SetEnemyTypeChance(int small, int med, int big)
    {
        chanceSmall = small;
        chanceMedium = med;
        chanceBig = big;
    }
}