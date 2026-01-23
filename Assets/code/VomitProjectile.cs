using UnityEngine;

public class VomitProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    private Transform target;

    public void Setup(Transform _target) { target = _target; }

    void Update()
    {
        if (target == null) { Destroy(gameObject); return; }

        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            // ทำดาเมจผู้เล่น
            PlayerHealth ph = target.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
            
            Destroy(gameObject); // ชนแล้วหายไป
        }
    }
}