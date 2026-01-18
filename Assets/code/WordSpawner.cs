using System.Collections.Generic;
using UnityEngine;

public class WordSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public WordManager wordManager;
    public List<string> wordBank = new List<string> { "น้อง", "นั่ง", "อยู่" }; // เรียงลำดับคำที่นี่
    private int currentWordIndex = 0;

    public Transform[] spawnPoints;
    public float spawnDelay = 3f; // เสกทุก 3 วินาที
    private float nextSpawnTime = 0f;

    void Update() {
        if (Time.time >= nextSpawnTime && currentWordIndex < wordBank.Count) {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnDelay;
        }
    }

    void SpawnEnemy() {
        // ตรวจสอบว่าได้ลากจุดเกิดมาใส่หรือยัง
        if (spawnPoints.Length == 0) {
            Debug.LogWarning("ลืมลากจุดเกิดใส่ในช่อง Spawn Points ครับ!");
            return;
        }

        // 1. สุ่มเลือกจุดเกิดจากรายการที่มี
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];

        // 2. สร้างผีที่ตำแหน่งของจุดที่สุ่มได้ (selectedPoint.position)
        GameObject enemyObj = Instantiate(enemyPrefab, selectedPoint.position, Quaternion.identity);
        
        WordDisplay display = enemyObj.GetComponentInChildren<WordDisplay>();
        Word newWord = new Word(wordBank[currentWordIndex], display, enemyObj.transform);
        wordManager.AddWord(newWord);
        currentWordIndex++;
    }
}
