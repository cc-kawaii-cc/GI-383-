using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordSpawner : MonoBehaviour
{
    [Header("Developer Mode")]
    [Tooltip("ติ๊กถูกเพื่อทดสอบบอสทันที (ข้ามเล่นปกติ)")]
    public bool testBossMode = false; 

    [Header("Boss Settings")]
    public GameObject bossPrefab;         // ลาก Prefab บอสมาใส่ช่องนี้
    
    [Tooltip("ใส่ประโยคยาวๆ ที่จะให้บอสพูดตรงนี้")]
    [TextArea(3, 10)] //ทำให้ช่องพิมพ์ใหญ่ขึ้น พิมพ์ได้หลายบรรทัด
    public string bossWord = "นะโมพุทธายะ สังคะโต อะระหัง (พิมพ์ยาวๆเพื่อปราบ)"; 

    [Header("Enemy Prefabs")]
    public GameObject smallEnemyPrefab;   
    public GameObject mediumEnemyPrefab;  
    public GameObject bigEnemyPrefab;     

    [Header("References")]
    public WordManager wordManager;
    public List<string> wordBank = new List<string>();
    public Transform[] spawnPoints;

    [Header("Spawn Settings")]
    public float spawnDelay = 3f;
    
    // Internal Variables
    private float nextSpawnTime = 0f;
    private bool isBossActive = false;
    private int chanceSmall = 100;
    private int chanceMedium = 0;
    private int chanceBig = 0;

    void Start()
    {
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

    // --- ฟังก์ชันเรียกบอส ---
    public void SpawnBoss()
    {
        if (isBossActive) return;

        Debug.Log("BOSS BATTLE START!");
        isBossActive = true;
        
        ClearAllEnemies();

        if (bossPrefab != null && spawnPoints.Length > 0)
        {
            GameObject bossObj = Instantiate(bossPrefab, spawnPoints[0].position, Quaternion.identity);
            WordDisplay display = bossObj.GetComponentInChildren<WordDisplay>();
            
            //ใช้ตัวแปร bossWord ที่ประกาศไว้ข้างบน (Public) แทน
            Word newWord = new Word(bossWord, display, bossObj.transform, true, true);
            
            wordManager.AddWord(newWord);
        }
        else
        {
            Debug.LogError("ลืมใส่ Boss Prefab หรือ Spawn Points ใน Inspector ครับ!");
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        GameObject prefabToSpawn = smallEnemyPrefab;
        int roll = Random.Range(0, 100);
        
        if (roll < chanceSmall) prefabToSpawn = smallEnemyPrefab;
        else if (roll < chanceSmall + chanceMedium) prefabToSpawn = mediumEnemyPrefab;
        else prefabToSpawn = bigEnemyPrefab;

        if(prefabToSpawn == null) prefabToSpawn = smallEnemyPrefab;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemyObj = Instantiate(prefabToSpawn, point.position, Quaternion.identity);

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