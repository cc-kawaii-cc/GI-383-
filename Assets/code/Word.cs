using UnityEngine;

[System.Serializable]
public class Word
{
    public string text;
    private int typeIndex;
    private WordDisplay display;
    private Transform enemyTransform; // เก็บตำแหน่งผีไว้ให้ลูกกระสุน

    public Word(string _text, WordDisplay _display, Transform _enemyTransform) {
        text = _text;
        display = _display;
        enemyTransform = _enemyTransform;
        display.SetWord(text);
    }

    public char GetNextLetter() => text[typeIndex];
    public void TypeLetter() {
        typeIndex++;
        display.RemoveLetter();
    }
    public bool WordTyped() => (typeIndex >= text.Length);
    public Transform GetEnemyTransform() => enemyTransform;
}