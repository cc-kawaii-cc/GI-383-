using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    private Transform target;

    public void Seek(Transform _target) => target = _target;

    void Update() {
        if (target == null) { Destroy(gameObject); return; }
        
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame) {
            // เมื่อถึงตัวผี ไม่ต้อง Destroy ผีทันที (ให้ WordDisplay จัดการ)
            Destroy(gameObject);
        }
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }
}