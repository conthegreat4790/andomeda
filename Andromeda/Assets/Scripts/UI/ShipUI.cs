using UnityEngine;
using TMPro;

public class ShipUI : MonoBehaviour
{
    public bool inShipMenu;
    public bool terrainGenStarted;
    public GameObject pixelationEffect;
    public GameObject shipUiObject;
    public GameObject shipUiPhysicalObject;
    public string bodyName;
    public TMP_Text starNameText;
    public MainMenu mainMenu;
    public ChunkManager chunkManager;
    public PlayerMovement playerMovement;

    void Update()
    {
        if (mainMenu.gameStarted)
        {
            if (inShipMenu)
            {
                pixelationEffect.SetActive(false);
                shipUiObject.SetActive(true);
                shipUiPhysicalObject.SetActive(true);

            }
            else
            {
                pixelationEffect.SetActive(true);
                shipUiObject.SetActive(false);
                shipUiPhysicalObject.SetActive(false);
            }
        }
    }

    public string GenerateStarName()
    {
        bool[] hasPrefixPool = { true, true, false, false, false };
        bool[] hasThreeLettersPool = { true, false };
        string[] alphabet = { "U", "D", "X", "C", "V", "K", "L", "H", "J", "S", "A", "P", "O", "Y", "R", "E", "W" };
        string[] prefixPool = { "Alpha", "Beta", "Omega" };
        bool hasPrefix = hasPrefixPool[Random.Range(0, hasPrefixPool.Length)];
        bool hasThreeLetters = hasThreeLettersPool[Random.Range(0, hasThreeLettersPool.Length)];


        string prefix = null;
        string letters = null;
        string endingNumber = $"0{Random.Range(1, 99)}";

        if (hasPrefix) { prefix = prefixPool[Random.Range(0, prefixPool.Length)] + "-"; } else { prefix = ""; }
        if (hasThreeLetters) { letters = alphabet[Random.Range(0, alphabet.Length)] + alphabet[Random.Range(0, alphabet.Length)] + "-"; } else { letters = alphabet[Random.Range(0, alphabet.Length)] + alphabet[Random.Range(0, alphabet.Length)] + alphabet[Random.Range(0, alphabet.Length)] + "-"; }

        return $"{prefix}{letters}{endingNumber}";
    }

    public string GeneratePlanetName()
    {
        bool[] hasPrefixPool = { true, true, false };
        string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H" };
        string[] prefixPool = { "Beta", "Kepler", "Lennon", "TESS", "Fermi" };
        bool hasPrefix = hasPrefixPool[Random.Range(0, hasPrefixPool.Length)];


        string prefix = null;
        string letter = alphabet[Random.Range(0, alphabet.Length)];
        string numbers = $"{Random.Range(1, 9999)}-";

        if (hasPrefix) { prefix = $"{prefixPool[Random.Range(0, prefixPool.Length)]}-"; } else { prefix = ""; }

        return $"{prefix}{numbers}{letter}";
    }

    [ContextMenu("Star Name")]
    void GetStarName()
    {
        bodyName = GenerateStarName();
    }

    [ContextMenu("Planet Name")]
    void GetPlanetName()
    {
        bodyName = GeneratePlanetName();
    }

    public void TravelToPlanet(int typee)
    {
        chunkManager.SetTerrainType(typee);
        chunkManager.StartGeneratingTerrain();

        inShipMenu = false;
        playerMovement.TeleportToTerrain();
    }

}