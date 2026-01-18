using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    public Slider healthBar;

    public void TakeDamage(float amount) {
        health -= amount;
        if (healthBar != null) healthBar.value = health;
        if (health <= 0) Debug.Log("Game Over!");
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            TakeDamage(10f);
            Destroy(other.gameObject);
        }
    }
}
