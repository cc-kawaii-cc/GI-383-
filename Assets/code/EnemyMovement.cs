using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{
    // เพิ่ม Type ใหม่: Splitter, Teleporter
    public enum EnemyType { Easy, Medium, Hard, Boss, GhostMom, KillMe, Spitter, ThaiMusicGhost, Splitter, Teleporter }

    [Header("Enemy Settings")]
    public EnemyType type;
    public float moveSpeed = 1.5f;
    public float damage = 10f;
    public float stopDistance = 0.1f;

    [Header("Spitter Settings")]
    public GameObject vomitPrefab;   
    public float shootInterval = 3f;
    private Vector3 wanderTarget;
    private float wanderTimer;

    [Header("Thai Music Ghost (Buffer)")]
    public AudioSource musicSource; 
    public AudioClip thaiSong;
    public float colorRotationSpeed = 1f; 
    public float sensitivity = 50f;       
    private float hue = 0f;
    public float buffRadius = 5f; 

    [Header("Teleporter Settings")]
    public float teleportInterval = 3f;

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

    [Header("Boss New Skills")]
    [Tooltip("ลากรูปผี (Sprite) มาใส่ช่องนี้")]
    public Sprite bossJumpscareSprite; 
    [Tooltip("ลากไฟล์เสียงกรีดร้องมาใส่ช่องนี้")]
    public AudioClip screamSound; // <--- เช็คช่องนี้ด้วยนะครับว่าใส่ไฟล์เสียงหรือยัง
    public float teleportRadius = 6f;
    
    private GameObject currentBlindInstance; 

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        wordCanvasGroup = GetComponentInChildren<CanvasGroup>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (type == EnemyType.Medium) StartCoroutine(InvisibilityRoutine());
        if (type == EnemyType.GhostMom) StartCoroutine(GhostMomRoutine());
        if (type == EnemyType.Spitter) StartCoroutine(SpitRoutine());
        if (type == EnemyType.Teleporter) StartCoroutine(TeleportRoutine());
        if (type == EnemyType.ThaiMusicGhost)
        {
            // ถ้าลืมใส่ AudioSource จะสร้างให้เอง
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            
            if (thaiSong != null)
            {
                musicSource.clip = thaiSong;
                musicSource.loop = true; 
                musicSource.spatialBlend = 0.8f; // เป็น 3D หน่อยๆ
                musicSource.Play();
            }
            StartCoroutine(DancerBuffRoutine());
        }
        if (type == EnemyType.Spitter)
        {
            StartCoroutine(SpitRoutine());
        }

        if (type == EnemyType.Boss) 
        {
            // StartCoroutine(BossCastDarkness()); // เทสเสร็จแล้วลบออกได้
            StartCoroutine(BossRoutine());
        }
    }

    void Update()
    {
        if (player == null || isWaiting) return;

        if (type == EnemyType.Boss)
        {
            Vector3 targetPos = player.position + (Vector3.up * bossHoverHeight);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
        else if (type == EnemyType.Spitter)
        {
            HandleSpitterWander();
        }
        else 
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }

        if (type == EnemyType.ThaiMusicGhost && musicSource != null && musicSource.isPlaying) HandleRGBSync();
    }

    // ---------------------------------------------------------
    // Logic: Boss Skills
    // ---------------------------------------------------------

    IEnumerator BossRoutine()
    {
        stopDistance = 5.0f;
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(4f, 6f));
            
            // ลดจำนวนสกิลสุ่มเหลือ 4 ท่า (0-3) เพราะเราตัด Jumpscare ออกไปไว้ที่การพิมพ์ผิดแล้ว
            int skill = Random.Range(0, 4); 
            
            switch (skill)
            {
                case 0: SummonMinions(); break;
                case 1: ShootVomit(); break;
                case 2: BossTeleport(); break;
                // case 3 เดิมคือ Jumpscare เราลบออก
                case 3: StartCoroutine(BossRapidSpit()); break; // เลื่อน RapidSpit มาแทนที่
            }
        }
    }

    // เพิ่มฟังก์ชันนี้: เพื่อให้ WordManager เรียกใช้เมื่อพิมพ์ผิด
    public void TriggerBossJumpscare()
    {
        // เรียกใช้ Coroutine เดิมที่มีอยู่แล้ว
        StartCoroutine(BossCastDarkness());
    }

    IEnumerator BossCastDarkness()
    {
        Debug.Log("👻 Boss uses Jumpscare!");

        // 1. จัดการเรื่องเสียง (แก้ใหม่: สร้างลำโพงให้เอง ถ้าไม่มี)
        if (screamSound != null)
        {
            if (musicSource == null)
            {
                // พยายามหา AudioSource ในตัวก่อน
                musicSource = GetComponent<AudioSource>();
                
                // ถ้ายังไม่มีอีก สร้างใหม่เลย!
                if (musicSource == null) 
                {
                    musicSource = gameObject.AddComponent<AudioSource>();
                    musicSource.spatialBlend = 0f; // ตั้งเป็น 2D (ดังเต็มหู ไม่สนระยะ)
                }
            }
            
            musicSource.PlayOneShot(screamSound);
        }
        else
        {
            Debug.LogWarning("⚠️ ลืมใส่ไฟล์เสียงในช่อง Scream Sound ของบอสครับ!");
        }

        // 2. จัดการเรื่องภาพ (เหมือนเดิม)
        if (currentBlindInstance == null)
        {
            GameObject canvasObj = GameObject.Find("JumpscareCanvas_System");
            Canvas canvas;
            if (canvasObj == null)
            {
                canvasObj = new GameObject("JumpscareCanvas_System");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; 
                canvas.sortingOrder = 999; 
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else canvas = canvasObj.GetComponent<Canvas>();

            GameObject panelObj = new GameObject("JumpscareImage_Final");
            panelObj.transform.SetParent(canvasObj.transform, false);
            Image img = panelObj.AddComponent<Image>();
            
            if (bossJumpscareSprite != null) 
            { 
                img.sprite = bossJumpscareSprite; 
                img.color = Color.white; 
                img.preserveAspect = false; 
            }
            else { img.color = Color.black; }

            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; 
            rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero; 
            currentBlindInstance = panelObj;
        }

        if (currentBlindInstance != null)
        {
            currentBlindInstance.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            currentBlindInstance.SetActive(false); 
        }
    }

    // ---------------------------------------------------------
    // Logic: New Monsters (ข้อ 2)
    // ---------------------------------------------------------
    
    void HandleSpitterWander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            wanderTarget = transform.position + (Vector3)(Random.insideUnitCircle * 3f);
            wanderTimer = Random.Range(1f, 3f);
        }
        transform.position = Vector2.MoveTowards(transform.position, wanderTarget, (moveSpeed * 0.5f) * Time.deltaTime);
    }

    IEnumerator TeleportRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(teleportInterval);
            if (player != null)
            {
                Vector2 randomPos = Random.insideUnitCircle.normalized * Random.Range(3f, 6f);
                Vector3 targetPos = player.position + new Vector3(randomPos.x, randomPos.y, 0);
                transform.position = targetPos;
            }
        }
    }

    IEnumerator DancerBuffRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, buffRadius);
            foreach(var hit in hits)
            {
                EnemyMovement em = hit.GetComponent<EnemyMovement>();
                if (em != null && em != this && em.type != EnemyType.Boss)
                {
                    if (em.moveSpeed < 3.0f) em.moveSpeed += 0.2f; 
                }
            }
        }
    }
    

    void SummonMinions()
    {
        WordSpawner spawner = FindObjectOfType<WordSpawner>();
        if (spawner != null && player != null)
        {
            float radius = spawner.spawnRadius; 
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f;
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.up;
                spawner.SpawnMinionAt(player.position + (dir * radius));
            }
        }
    }

    void BossTeleport()
    {
        if (player == null) return;
        Vector2 randomPos = Random.insideUnitCircle.normalized * teleportRadius;
        transform.position = player.position + new Vector3(randomPos.x, randomPos.y, 0);
    }

    IEnumerator BossRapidSpit()
    {
        for(int i=0; i<3; i++) { ShootVomit(); yield return new WaitForSeconds(0.2f); }
    }

    void ShootVomit()
    {
        if (vomitPrefab != null)
        {
            GameObject p = Instantiate(vomitPrefab, transform.position, Quaternion.identity);
            VomitProjectile v = p.GetComponent<VomitProjectile>();
            if (v != null && player != null) v.Setup(player);
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
    
    
    IEnumerator SpitRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);
        
            // เช็คระยะก่อนยิง (ถ้าอยากให้ยิงเฉพาะตอนเห็นผู้เล่น)
            if (player != null)
            {
                ShootVomit();
            }
        }
    }
    IEnumerator GhostMomRoutine()
    {
        isWaiting = true;
        yield return new WaitForSeconds(Random.Range(3f, 5f));
        WordSpawner s = FindObjectOfType<WordSpawner>();
        if (s != null) { s.SpawnMinionAt(transform.position); s.SpawnMinionAt(transform.position); }
        isWaiting = false;
    }
    IEnumerator InvisibilityRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(Fade(1f, 0f)); yield return new WaitForSeconds(invisibleDuration);
            yield return StartCoroutine(Fade(0f, 1f)); yield return new WaitForSeconds(visibleDuration);
        }
    }
    IEnumerator Fade(float start, float end)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            float a = Mathf.Lerp(start, end, t);
            if (spriteRenderer != null) { Color c = spriteRenderer.color; c.a = a; spriteRenderer.color = c; }
            if (wordCanvasGroup != null) wordCanvasGroup.alpha = a;
            yield return null;
        }
    }
    // เพิ่มฟังก์ชันนี้ลงใน EnemyMovement.cs
    public void OnDeath()
    {
        // 1. ถ้าเป็นบอสตาย -> ชนะเกม
        if (type == EnemyType.Boss)
        {
            if (GameManager.instance != null) GameManager.instance.Victory();
        }
        // 2. ถ้าเป็นตัว Splitter -> เสกตัวลูก 2 ตัว
        else if (type == EnemyType.Splitter)
        {
            WordSpawner spawner = FindObjectOfType<WordSpawner>();
            if (spawner != null)
            {
                spawner.SpawnMinionAt(transform.position);
                spawner.SpawnMinionAt(transform.position);
            }
        }
        
        // 3. ถ้าเป็นตัวอื่นๆ (เช่น KillMe, Medium) -> ก็ให้จบการทำงานตรงนี้ (เตรียมตัวโดนทำลาย)
        // (Logic การ Destroy object จะถูกทำต่อใน WordManager หรือ WordDisplay เอง)
    }
}