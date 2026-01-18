using UnityEngine;
using TMPro;

public class WordDisplay : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;

    public void SetWord(string word) {
        textDisplay.text = word;
    }

    public void RemoveLetter() {
        textDisplay.text = textDisplay.text.Remove(0, 1);
        textDisplay.color = Color.green; // เปลี่ยนสีตัวที่พิมถูก
    }

    public void DestroyEnemy() {
        Destroy(gameObject);
    }
}
