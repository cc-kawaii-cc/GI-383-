using UnityEngine;

[System.Serializable]
public class Word
{
    public int hp = 1;
    public string text;
    public bool isSpecial;
    public bool isBoss; //เพิ่มตัวนี้: เช็คว่าเป็นบอสหรือไม่
    
    private int typeIndex;
    private WordDisplay display;
    private Transform enemyTransform;

    // เพิ่ม Parameter isBoss เข้าไปใน Constructor
    public Word(string _text, WordDisplay _display, Transform _enemyTransform, bool _isSpecial, bool _isBoss) {
        text = _text;
        display = _display;
        enemyTransform = _enemyTransform;
        isSpecial = _isSpecial;
        isBoss = _isBoss;
    
        // ถ้าเป็น Hard ให้มี HP 2 (ต้องพิมพ์ 2 รอบ)
        EnemyMovement movement = enemyTransform.GetComponent<EnemyMovement>();
        if (movement != null && movement.type == EnemyMovement.EnemyType.Hard) {
            hp = 2;
        }

        display.SetWord(text);
    }
    
    public void ResetWord() {
        typeIndex = 0; // รีเซ็ตตำแหน่งการพิมพ์กลับไปตัวแรก
    }

    public char GetNextLetter() => text[typeIndex];

    public void TypeLetter() {
        typeIndex++;
        display.RemoveLetter();
    }

    public bool WordTyped() => (typeIndex >= text.Length);
    
    public Transform GetEnemyTransform() => enemyTransform;

    //ฟังก์ชันเรียกเอฟเฟกต์ตัวแดง
    public void TriggerWrongTyping() {
        if(display != null) display.FlashRed();
    }
}