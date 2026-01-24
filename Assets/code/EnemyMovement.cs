using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{
    public enum EnemyType { Easy, Medium, Hard, Boss, GhostMom, KillMe, Spitter, ThaiMusicGhost }

    [Header("Enemy Settings")]
    public EnemyType type;
    public float moveSpeed = 1.5f;
    public float damage = 10f;
    public float stopDistance = 0.1f;

    [Header("Spitter Settings")]
    public GameObject vomitPrefab;   
    public float shootInterval = 3f;

    [Header("Thai Music Ghost (RGB Sync)")]
    public AudioSource musicSource; 
    public AudioClip thaiSong;
    public float colorRotationSpeed = 1f; 
    public float sensitivity = 50f;       
    private float hue = 0f;

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
    public AudioClip screamSound;
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
        
        if (type == EnemyType.ThaiMusicGhost && musicSource != null && thaiSong != null)
        {
            musicSource.clip = thaiSong;
            musicSource.loop = true; 
            musicSource.Play();
        }

        if (type == EnemyType.Boss) 
        {
            // บังคับเทส 1 ครั้งตอนเริ่มเกม (ถ้าพอใจแล้วให้ลบบรรทัดนี้ทิ้ง)
            StartCoroutine(BossCastDarkness()); 
            
            StartCoroutine(BossRoutine());
        }
    }

    void Update()
    {
        if (player != null && !isWaiting)
        {
            if (type == EnemyType.Boss)
            {
                Vector3 targetPos = player.position + (Vector3.up * bossHoverHeight);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            else if (type != EnemyType.Spitter) 
            {
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance > stopDistance)
                {
                    transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                }
            }
        }
        if (type == EnemyType.ThaiMusicGhost && musicSource != null && musicSource.isPlaying) HandleRGBSync();
    }

    IEnumerator BossRoutine()
    {
        stopDistance = 5.0f;
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(4f, 6f));
            int skill = Random.Range(0, 5); 
            
            switch (skill)
            {
                case 0: SummonMinions(); break;
                case 1: ShootVomit(); break;
                case 2: BossTeleport(); break;
                case 3: StartCoroutine(BossCastDarkness()); break;
                case 4: StartCoroutine(BossRapidSpit()); break;
            }
        }
    }

    // --- Skill Jumpscare (แก้ใหม่: ตู้มเดียว ไม่กระพริบ) ---
    IEnumerator BossCastDarkness()
    {
        Debug.Log("👻 Boss uses Jumpscare (One Shot)!");

        if (currentBlindInstance == null)
        {
            GameObject canvasObj = GameObject.Find("JumpscareCanvas_System");
            Canvas canvas;

            if (canvasObj == null)
            {
                canvasObj = new GameObject("JumpscareCanvas_System");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; 
                canvas.sortingOrder = 999; // ทับทุกอย่าง
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvas = canvasObj.GetComponent<Canvas>();
            }

            GameObject panelObj = new GameObject("JumpscareImage_Final");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            Image img = panelObj.AddComponent<Image>();
            
            if (bossJumpscareSprite != null)
            {
                img.sprite = bossJumpscareSprite; 
                img.color = Color.white;
                img.preserveAspect = false; 
            }
            else
            {
                img.color = Color.black; 
            }

            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; 
            rt.anchorMax = Vector2.one;  
            rt.sizeDelta = Vector2.zero; 
            rt.anchoredPosition = Vector2.zero; 
            
            currentBlindInstance = panelObj;
        }

        if (musicSource != null && screamSound != null) musicSource.PlayOneShot(screamSound);

        // --- ส่วนที่แก้: โชว์ปุ๊บ ค้างเลย (ตัดส่วนกระพริบออก) ---
        if (currentBlindInstance != null)
        {
            currentBlindInstance.SetActive(true); // 1. เปิดทันที ตู้ม!
            
            // 2. แช่ค้างไว้นิ่งๆ 2.5 วินาที
            yield return new WaitForSeconds(2.5f);
            
            currentBlindInstance.SetActive(false); // 3. ปิด
        }
    }

    // --- Skills อื่นๆ คงเดิม ---
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
        for(int i=0; i<3; i++)
        {
            ShootVomit();
            yield return new WaitForSeconds(0.2f);
        }
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
        while (true) { yield return new WaitForSeconds(shootInterval); if (player != null) ShootVomit(); }
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
}