using UnityEngine;

public class ENDdata : MonoBehaviour
{
    public static ENDdata Instance { get; private set; }

    public bool playerWon = false;

    public int Green = 0;
    public int Red = 0;
    public int Blue = 0;
    public int Yellow = 0;
    public int Wild = 0;

    public int cCP = 0;
    public int cR = 0;
    public int cDC = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResetearValores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetearValores()
    {
        playerWon = false;
        Green = 0;
        Red = 0;
        Blue = 0;
        Yellow = 0;
        Wild = 0;
        cCP = 0;
        cR = 0;
        cDC = 0;
    }
}
