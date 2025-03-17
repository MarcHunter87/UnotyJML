using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class EndManager : MonoBehaviour
{
    public static EndManager Instance { get; private set; }

    public Button PAgain;
    public Button Menu;

    private bool playerWon;
    public GameObject Lose;
    public GameObject Win;

    private int Green;
    private int Red;
    private int Blue;
    private int Yellow;
    private int Wild;

    public TextMeshProUGUI MPColor;
    public TextMeshProUGUI CP;
    public TextMeshProUGUI R;
    public TextMeshProUGUI DC;

    private int cCP;
    private int cR;
    private int cDC;

    private Vector3 mpColorOriginalSize;
    private Vector3 cpOriginalSize;
    private Vector3 rOriginalSize;
    private Vector3 dcOriginalSize;
    private Vector3 goOriginalSize;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerWon = ENDdata.Instance.playerWon;
        Green = ENDdata.Instance.Green;
        Red = ENDdata.Instance.Red;
        Blue = ENDdata.Instance.Blue;
        Yellow = ENDdata.Instance.Yellow;
        Wild = ENDdata.Instance.Wild;
        cCP = ENDdata.Instance.cCP;
        cR = ENDdata.Instance.cR;
        cDC = ENDdata.Instance.cDC;

        PAgain.onClick.AddListener(PlayAgain);
        Menu.onClick.AddListener(RegresoAlMenu);

        goOriginalSize = playerWon ? Win.transform.localScale : Lose.transform.localScale;
        mpColorOriginalSize = MPColor.transform.localScale;
        cpOriginalSize = CP.transform.localScale;
        rOriginalSize = R.transform.localScale;
        dcOriginalSize = DC.transform.localScale;

        Win.transform.localScale = Vector3.zero;
        Lose.transform.localScale = Vector3.zero;
        MPColor.transform.localScale = Vector3.zero;
        CP.transform.localScale = Vector3.zero;
        R.transform.localScale = Vector3.zero;
        DC.transform.localScale = Vector3.zero;

        StartCoroutine(SpawnTextoSecuencialmente());
    }

    private IEnumerator SpawnTextoSecuencialmente()
    {
        StartCoroutine(SpawnVD(playerWon ? Win : Lose, goOriginalSize));

        yield return new WaitForSeconds(0.3f);
        ActualizarTextoColorMasUsado();

        yield return new WaitForSeconds(0.2f);
        ActualizarCP();

        yield return new WaitForSeconds(0.2f);
        ActualizarR();

        yield return new WaitForSeconds(0.2f);
        ActualizarDC();
    }

    private IEnumerator SpawnVD(GameObject objeto, Vector3 escalaFinal)
    {
        float duracion = 0.2f;
        float tiempoTranscurrido = 0f;
        Vector3 escalaRebote = escalaFinal * 1.2f;

        AudioManager.Instance.PlaySound("leaderSound");

        while (tiempoTranscurrido < duracion * 0.7f)
        {
            float avanceCrecimiento = tiempoTranscurrido / (duracion * 0.7f);
            objeto.transform.localScale = Vector3.Lerp(Vector3.zero, escalaRebote, avanceCrecimiento);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        float tiempoRebote = 0f;
        float duracionRebote = duracion * 0.3f;

        while (tiempoRebote < duracionRebote)
        {
            float avance = tiempoRebote / duracionRebote;
            objeto.transform.localScale = Vector3.Lerp(escalaRebote, escalaFinal, avance);
            tiempoRebote += Time.deltaTime;
            yield return null;
        }

        objeto.transform.localScale = escalaFinal;
    }

    private void ActualizarTextoColorMasUsado()
    {
        int maxUsos = Mathf.Max(Green, Red, Blue, Yellow, Wild);

        if (maxUsos == Green)
        {
            MPColor.text = "GREEN";
            MPColor.color = Color.green;
        }
        else if (maxUsos == Red)
        {
            MPColor.text = "RED";
            MPColor.color = Color.red;
        }
        else if (maxUsos == Blue)
        {
            MPColor.text = "BLUE";
            MPColor.color = Color.blue;
        }
        else if (maxUsos == Yellow)
        {
            MPColor.text = "YELLOW";
            MPColor.color = Color.yellow;
        }
        else if (maxUsos == Wild)
        {
            MPColor.text = "<color=#FF0000>W</color><color=#00FF00>I</color><color=#0000FF>L</color><color=#FFFF00>D</color>";
        }

        StartCoroutine(SpawnTexto(MPColor, mpColorOriginalSize));
    }

    private void ActualizarCP()
    {
        CP.text = cCP.ToString().PadLeft(3, '0');
        StartCoroutine(SpawnTexto(CP, cpOriginalSize));
    }

    public void ActualizarR()
    {
        R.text = cR.ToString().PadLeft(3, '0');
        StartCoroutine(SpawnTexto(R, rOriginalSize));
    }

    private void ActualizarDC()
    {
        DC.text = cDC.ToString().PadLeft(3, '0');
        StartCoroutine(SpawnTexto(DC, dcOriginalSize));
    }

    private IEnumerator SpawnTexto(TextMeshProUGUI texto, Vector3 escalaFinal)
    {
        float duracion = 0.2f;
        float tiempoTranscurrido = 0f;
        Vector3 escalaRebote = escalaFinal * 1.2f;

        AudioManager.Instance.PlaySound("leaderSound");

        while (tiempoTranscurrido < duracion * 0.7f)
        {
            float avanceCrecimiento = tiempoTranscurrido / (duracion * 0.7f);
            texto.transform.localScale = Vector3.Lerp(Vector3.zero, escalaRebote, avanceCrecimiento);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        float tiempoRebote = 0f;
        float duracionRebote = duracion * 0.3f;

        while (tiempoRebote < duracionRebote)
        {
            float avanceRebote = tiempoRebote / duracionRebote;
            texto.transform.localScale = Vector3.Lerp(escalaRebote, escalaFinal, avanceRebote);
            tiempoRebote += Time.deltaTime;
            yield return null;
        }

        texto.transform.localScale = escalaFinal;
    }

    public void PlayAgain()
    {
        ENDdata.Instance.ResetearValores();
        StartCoroutine(CargarEscena("Juego"));
    }

    public void RegresoAlMenu()
    {
        ENDdata.Instance.ResetearValores();
        StartCoroutine(CargarEscena("Menu Principal"));
    }

    private IEnumerator CargarEscena(string nombreEscena)
    {
        yield return new WaitForSeconds(0.35f);
        SceneManager.LoadScene(nombreEscena);
    }
}