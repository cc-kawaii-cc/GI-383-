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

    void Update()
    {
        // ระบบเดิน
        if (player != null && !isWaiting && type != EnemyType.Spitter)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }

        // ✅ ระบบเปลี่ยนสี RGB ตามจังหวะเพลง (จากรูป image_dafecd.png)
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