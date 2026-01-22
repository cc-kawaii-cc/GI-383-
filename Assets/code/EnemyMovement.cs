using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public enum EnemyType
    {
        Easy,
        Medium,
        Hard,
        Boss,
        GhostMom
        
    }
    private bool isWaiting = false;

    [Header("Enemy Settings")] public EnemyType type;
    public float moveSpeed = 1.5f;
    public float damage = 10f;
    public float stopDistance = 0.1f;
    
    [Header("Medium Type: Invisible Settings")]
    public float invisibleDuration = 1f;
    public float visibleDuration = 2f;
    public GameObject wordCanvas;
    public float fadeSpeed = 2f; // ความเร็วในการจางหาย/ปรากฏ
    private CanvasGroup wordCanvasGroup;
    private SpriteRenderer spriteRenderer;

    private Transform player;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        wordCanvasGroup = GetComponentInChildren<CanvasGroup>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // ถ้าเป็น Easy ให้วิ่งเร็วขึ้น (เช่น x2)
        if (type == EnemyType.Easy) moveSpeed *= 2f;

        // ถ้าเป็น Medium ให้เริ่มทำงานระบบหายตัว
        if (type == EnemyType.Medium) 
        {
            StartCoroutine(InvisibilityRoutine());
        }
        if (type == EnemyType.GhostMom)
        {
            StartCoroutine(GhostMomRoutine());
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
    IEnumerator GhostMomRoutine()
    {
        isWaiting = true;
    
        // 1. ยืนนิ่งๆ รอ 3-5 วินาที
        yield return new WaitForSeconds(Random.Range(3f, 5f));
    
        // 2. เรียกม่อนออกมา 2 ตัว
        SummonMinions();
    
        // 3. เริ่มเดินเข้าหาผู้เล่น
        isWaiting = false;
    }

    void SummonMinions()
    {
        WordSpawner spawner = FindObjectOfType<WordSpawner>();
        if (spawner != null)
        {
            spawner.SpawnMinionAt(transform.position); // ตัวที่ 1
            spawner.SpawnMinionAt(transform.position); // ตัวที่ 2
            Debug.Log("Ghost Mom Summoned Minions!");
        }
    }
    IEnumerator InvisibilityRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        while (true)
        {
            // --- ค่อยๆ จางหาย (Fade Out) ---
            yield return StartCoroutine(Fade(1f, 0f)); 
            yield return new WaitForSeconds(invisibleDuration);

            // --- ค่อยๆ ปรากฏตัว (Fade In) ---
            yield return StartCoroutine(Fade(0f, 1f)); 
            yield return new WaitForSeconds(visibleDuration);
        }
    }

// ฟังก์ชันสำหรับจัดการการจาง
    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime);

            // ปรับที่ตัวมอนสเตอร์
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = currentAlpha;
                spriteRenderer.color = c;
            }

            // ปรับที่ตัวหนังสือ (ผ่าน Canvas Group)
            if (wordCanvasGroup != null)
            {
                wordCanvasGroup.alpha = currentAlpha;
            }

            yield return null;
        }
    }
}