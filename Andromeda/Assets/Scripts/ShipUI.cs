using UnityEngine;
using TMPro;

public class ShipUI : MonoBehaviour
{
    public string starName;
    public TMP_Text starNameText;

    [ContextMenu("Generate name")]
    void GenerateStarName()
    {
        bool[] hasPrefixPool = { true, true, false, false, false };
        bool[] hasThreeLettersPool = { true, false };
        string[] alphabet = { "U", "D", "X", "C", "V", "K", "L", "H", "J", "S", "A", "P", "O", "Y", "R", "E", "W" };
        string[] prefixPool = { "Alpha", "Beta", "Kepler", "Lennon", "Mega", "Omega" };
        bool hasPrefix = hasPrefixPool[Random.Range(0, hasPrefixPool.Length)];
        bool hasThreeLetters = hasThreeLettersPool[Random.Range(0, hasThreeLettersPool.Length)];


        string prefix = null;
        string letters = null;
        string endingNumber = $"0{Random.Range(1, 99)}";

        if (hasPrefix) { prefix = prefixPool[Random.Range(0, prefixPool.Length)] + "-"; } else { prefix = ""; }
        if (hasThreeLetters) { letters = alphabet[Random.Range(0, alphabet.Length)] + alphabet[Random.Range(0, alphabet.Length)] + "-"; } else { letters = alphabet[Random.Range(0, alphabet.Length)] + alphabet[Random.Range(0, alphabet.Length)] + alphabet[Random.Range(0, alphabet.Length)] + "-"; }

        starName = $"{prefix}{letters}{endingNumber}";
        starNameText.text = $"Star: {starName}";
    }
}