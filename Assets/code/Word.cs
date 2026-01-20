using UnityEngine;

[System.Serializable]
public class Word
{
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
        isBoss = _isBoss; //รับค่า
        display.SetWord(text);
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