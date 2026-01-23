using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public enum EnemyType { Easy, Medium, Hard, Boss, GhostMom, KillMe, Spitter, ThaiMusicGhost }

    [Header("Enemy Settings")]
    public EnemyType type;
    public float moveSpeed = 1.5f;
    public float damage = 10f;
    public float stopDistance = 0.1f;

    // ✅ แก้ Error: Cannot resolve symbol 'vomitPrefab' และ 'shootInterval'
    [Header("Spitter Settings")]
    public GameObject vomitPrefab;   
    public float shootInterval = 3f;

    // ✅ แก้ Error: Cannot resolve symbol 'hue', 'colorRotationSpeed', 'sensitivity'
    [Header("Thai Music Ghost (RGB Sync)")]
    public AudioSource musicSource; 
    public AudioClip thaiSong;
    public float colorRotationSpeed = 1f; 
    public float sensitivity = 50f;       
    private float hue = 0f; // ตัวแปรเก็บค่าสีรุ้ง

    [Header("Medium Type: Invisible Settings")]
    public float invisibleDuration = 1f;
    public float visibleDuration = 2f;
    public float fadeSpeed = 2f; 
    private CanvasGroup wordCanvasGroup;
    private SpriteRenderer spriteRenderer;

    private Transform player;
    private bool isWaiting = false;
    
    [Header("Boss Movement")]
    public float bossHoverHeight = 3.5f;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        wordCanvasGroup = GetComponentInChildren<CanvasGroup>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // เริ่มระบบตามประเภท
        if (type == EnemyType.Medium) StartCoroutine(InvisibilityRoutine());
        if (type == EnemyType.GhostMom) StartCoroutine(GhostMomRoutine());
        if (type == EnemyType.Spitter) StartCoroutine(SpitRoutine());
        if (type == EnemyType.Boss) StartCoroutine(BossRoutine());
        if (type == EnemyType.ThaiMusicGhost)
        {
            if (musicSource != null && thaiSong != null)
            {
                musicSource.clip = thaiSong;
                musicSource.loop = true; 
                musicSource.Play();
            }
        }
    }
    IEnumerator BossRoutine()
    {
        // 1. บอสจะเดินมาหยุดที่ระยะปลอดภัย (ตามที่ตั้งไว้ใน bossHoverHeight)
        stopDistance = 5.0f; 
        
        while (true)
        {
            yield return new WaitForSeconds(4f); // รอ 4 วินาที (Cooldown สกิล)

            // สุ่มสกิล: 0 = เสกลูกน้อง, 1 = ยิงอ้วกใส่
            int skill = Random.Range(0, 2);

            if (skill == 0)
            {
                // --- [แก้ใหม่] Skill: เรียกสมุนที่เส้น Spawn Radius ---
                WordSpawner spawner = FindObjectOfType<WordSpawner>();
                if (spawner != null && player != null)
                {
                    Debug.Log("Boss: Summon Minions at Spawn Radius!");
                    
                    // ✅ ดึงค่า spawnRadius มาจาก Spawner โดยตรง (ค่าเส้นวงกลมเหลือง)
                    float radius = spawner.spawnRadius; 
                    
                    int minionCount = 3; // จำนวนลูกน้องที่จะเสก

                    for (int i = 0; i < minionCount; i++)
                    {
                        // คำนวณมุมให้กระจายเป็นวงกลม 360 องศา
                        float angle = i * (360f / minionCount);
                        
                        // แปลงมุมเป็นทิศทาง
                        Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.up;
                        
                        // ✅ คำนวณจุดเกิด: ผู้เล่น + (ทิศทาง * รัศมีวงกลม)
                        Vector3 spawnPos = player.position + (dir * radius);
                        
                        // สั่งเสกตรงจุดนั้น
                        spawner.SpawnMinionAt(spawnPos);
                    }
                }
            }
            else
            {
                // Skill: ยิงโจมตี (เหมือน Spitter)
                Debug.Log("Boss: Attack!");
                ShootVomit(); 
            }
        }
    }
    void Update()
    {
        // ระบบเดิน
        if (player != null && !isWaiting)
        {
            if (type == EnemyType.Boss)
            {
                // --- (ของใหม่) บอส: ล็อกเป้าไปที่ "จุดเหนือหัวผู้เล่น" ---
                // เอาตำแหน่งผู้เล่น + ความสูง (Vector3.up)
                Vector3 targetPos = player.position + (Vector3.up * bossHoverHeight);
                
                // สั่งให้เดินไปที่จุดนั้น
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            else if (type != EnemyType.Spitter) 
            {
                // (ของเก่า) มอนทั่วไป: เดินเข้าหาตัวผู้เล่นตรงๆ และหยุดเมื่อถึงระยะ
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance > stopDistance)
                {
                    transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                }
            }
        }

        // ... (ส่วน RGB Sync ของผีนางรำ ปล่อยไว้เหมือนเดิม) ...
        if (type == EnemyType.ThaiMusicGhost && musicSource != null && musicSource.isPlaying)
        {
            HandleRGBSync();
        }
    }

    void HandleRGBSync()
    {
        float[] samples = new float[256];
        musicSource.GetOutputData(samples, 0);
        float sum = 0;
        foreach (float s in samples) sum += s * s;
        float rms = Mathf.Sqrt(sum / 256);

        hue += Time.deltaTime * colorRotationSpeed;
        if (hue > 1) hue -= 1;

        if (spriteRenderer != null)
        {
            float brightness = 0.5f + (rms * sensitivity); 
            spriteRenderer.color = Color.HSVToRGB(hue, 1f, Mathf.Clamp(brightness, 0.5f, 1f));
        }
    }

    // --- Coroutines สำหรับความสามารถพิเศษ ---
    IEnumerator SpitRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);
            if (player != null) ShootVomit();
        }
    }

    void ShootVomit()
    {
        if (vomitPrefab != null)
        {
            GameObject projectile = Instantiate(vomitPrefab, transform.position, Quaternion.identity);
            VomitProjectile v = projectile.GetComponent<VomitProjectile>();
            if (v != null) v.Setup(player);
        }
    }

    IEnumerator GhostMomRoutine()
    {
        isWaiting = true;
        yield return new WaitForSeconds(Random.Range(3f, 5f));
        WordSpawner spawner = FindObjectOfType<WordSpawner>();
        if (spawner != null)
        {
            spawner.SpawnMinionAt(transform.position);
            spawner.SpawnMinionAt(transform.position);
        }
        isWaiting = false;
    }

    IEnumerator InvisibilityRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(Fade(1f, 0f)); 
            yield return new WaitForSeconds(invisibleDuration);
            yield return StartCoroutine(Fade(0f, 1f)); 
            yield return new WaitForSeconds(visibleDuration);
        }
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime);
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = currentAlpha;
                spriteRenderer.color = c;
            }
            if (wordCanvasGroup != null) wordCanvasGroup.alpha = currentAlpha;
            yield return null;
        }
    }
}