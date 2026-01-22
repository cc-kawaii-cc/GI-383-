using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public enum EnemyType
    {
        Easy,
        Medium,
        Hard,
        Boss
    }

    [Header("Enemy Settings")] public EnemyType type;
    public float moveSpeed = 1.5f;
    public float damage = 10f;
    public float stopDistance = 0.1f;
    
    [Header("Medium Type: Invisible Settings")]
    public float invisibleDuration = 1f;

    public float visibleDuration = 2f;
    public GameObject wordCanvas;
    private SpriteRenderer spriteRenderer;

    private Transform player;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // ถ้าเป็น Easy ให้วิ่งเร็วขึ้น (เช่น x2)
        if (type == EnemyType.Easy) moveSpeed *= 2f;

        // ถ้าเป็น Medium ให้เริ่มทำงานระบบหายตัว
        if (type == EnemyType.Medium)
        {
            // หากไม่ได้ลากใส่ใน Inspector ให้ลองหาอัตโนมัติจากในตัวมันเอง
            if (wordCanvas == null)
            {
                wordCanvas = GetComponentInChildren<Canvas>()?.gameObject;
            }

            StartCoroutine(InvisibilityRoutine());
        }

    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                transform.position =
                    Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
    }

    IEnumerator InvisibilityRoutine()
    {
        // 1. สุ่มจังหวะเริ่มต้น (Offset) เพื่อไม่ให้ทุกตัวเริ่มหายพร้อมกัน
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        while (true)
        {
            // --- หายตัว ---
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (wordCanvas != null) wordCanvas.SetActive(false);
        
            // 2. สุ่มระยะเวลาที่หายไป (เช่น ระหว่าง 0.5 ถึง 1.5 วินาที)
            float randomInvisibleTime = Random.Range(invisibleDuration * 0.5f, invisibleDuration * 1.5f);
            yield return new WaitForSeconds(randomInvisibleTime);

            // --- ปรากฏตัว ---
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            if (wordCanvas != null) wordCanvas.SetActive(true);
        
            // 3. สุ่มระยะเวลาที่โชว์ตัว (เช่น ระหว่าง 1 ถึง 3 วินาที)
            float randomVisibleTime = Random.Range(visibleDuration * 0.5f, visibleDuration * 1.5f);
            yield return new WaitForSeconds(randomVisibleTime);
        }
    }
}