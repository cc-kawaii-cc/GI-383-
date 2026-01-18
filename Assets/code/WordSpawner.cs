using System.Collections.Generic;
using UnityEngine;
using TMPro; // เพิ่มอันนี้เพื่อแก้ Error 'TextMeshProUGUI'

public class WordSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public WordManager wordManager;
    public List<string> wordBank = new List<string>();
    public Transform[] spawnPoints; // สำหรับสุ่มจุดเกิด

    public float spawnDelay = 3f;
    private float nextSpawnTime = 0f;
    private int currentWordIndex = 0;

    void Update()
    {
        if (Time.time >= nextSpawnTime && currentWordIndex < wordBank.Count)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnDelay;
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || currentWordIndex >= wordBank.Count) return;

        // 1. ดึงคำศัพท์จาก List
        string rawWord = wordBank[currentWordIndex];
        bool isSpecial = false;

        // 2. ตรวจสอบว่าคำนี้เป็นคำพิเศษหรือไม่ (เช่น "นั่ง*")
        if (rawWord.EndsWith("*"))
        {
            isSpecial = true;
            // ตัดเครื่องหมาย * ออกก่อนส่งไปโชว์บนหัวผี เพื่อให้ผู้เล่นไม่รู้
            rawWord = rawWord.Replace("*", ""); 
        }

        // 3. สร้างผีตามปกติ
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];
        GameObject enemyObj = Instantiate(enemyPrefab, selectedPoint.position, Quaternion.identity);
    
        WordDisplay display = enemyObj.GetComponentInChildren<WordDisplay>();

        // 4. ส่งคำที่ตัดเครื่องหมายออกแล้วไปสร้าง Word
        Word newWord = new Word(rawWord, display, enemyObj.transform, isSpecial);
        wordManager.AddWord(newWord);
    
        currentWordIndex++;
        // ระบบ Timer: ลดเวลาเกิดลงเรื่อยๆ
        if (spawnDelay > 1f) 
        {
            spawnDelay -= 0.1f;
        }
    }
}