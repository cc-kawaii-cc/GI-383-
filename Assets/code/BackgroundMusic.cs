using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic instance;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip backgroundClip;
    [Range(0f, 1f)] public float volume = 0.5f;

    void Awake()
    {
        // ระบบ Singleton: ป้องกันไม่ให้เพลงเกิดซ้ำซ้อน
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); // เปิดบรรทัดนี้ถ้าต้องการให้เพลงเล่นต่อแม้เปลี่ยน Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (backgroundClip != null)
        {
            audioSource.clip = backgroundClip;
            audioSource.loop = true; // ตั้งค่าให้เพลงวนลูป
            audioSource.volume = volume;
            audioSource.playOnAwake = true;
            audioSource.Play();
        }
    }

    // ฟังก์ชันสำหรับปรับความดังหรือหยุดเพลงจากสคริปต์อื่น
    public void SetVolume(float newVolume) => audioSource.volume = newVolume;
    public void StopMusic() => audioSource.Stop();
}