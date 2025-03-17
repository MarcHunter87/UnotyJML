using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject[] fondos;
    public bool playerTurn = true;
    public bool enemyTurn = false;
    public bool gameOver = false;
    public GameObject[] botonesSeleccionarColor;
    public GameObject baseBotones;
    public string colorUltimaCarta;
    public string valorUltimaCarta;
    public TextMeshProUGUI whoText;
    public TextMeshProUGUI turnText;
    public GameObject baseTurno;
    private Vector3 whoTextOriginalScale;
    private Vector3 turnTextOriginalScale;
    private Vector3 baseTurnoOriginalScale;
    private bool primeraAnimacionTurno = true;

    private Vector3[] posicionesOriginalesBotones;
    private Quaternion[] rotacionesOriginalesBotones;
    private Vector3[] escalasOriginalesBotones;
    private Vector3 posicionOriginalBase;
    private Quaternion rotacionOriginalBase;
    private Vector3 escalaOriginalBase;

    public static float zUltimaCartaLanzada = 0f;

    public bool seleccionandoColor = false;

    public int totalCartasRobar = 0;

    public int ultimaPosicion = -1;

    public GameObject[] cardThrowVariables;

    private bool primeraVez = true;

    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (gameOver == true)
        {
            SceneManager.LoadScene("END");
        }
    }

    void Start()
    {
        GuardarPosicionesBotonesColor();
        DesactivarBotonesColor();

        if (whoText != null)
        {
            whoTextOriginalScale = whoText.transform.localScale;
            whoText.transform.localScale = Vector3.zero;
        }
        if (turnText != null)
        {
            turnTextOriginalScale = turnText.transform.localScale;
            turnText.transform.localScale = Vector3.zero;
        }
        if (baseTurno != null)
        {
            baseTurnoOriginalScale = baseTurno.transform.localScale;
            baseTurno.transform.localScale = Vector3.zero;
        }

        int turnoInicial = Random.Range(0, 2);
        if (turnoInicial == 0)
        {
            playerTurn = true;
            enemyTurn = false;
        }
        else
        {
            playerTurn = false;
            enemyTurn = false;
            StartCoroutine(EsperarPrimerTurnoEnemigo());
        }
        StartCoroutine(ActualizarTextosPrimerTurno(turnoInicial == 0 ? "PLAYER" : "ENEMY"));
    }

    private IEnumerator EsperarPrimerTurnoEnemigo()
    {
        yield return new WaitForSeconds(1f);
        enemyTurn = true;
    }

    public void SeleccionarPrimerBotonColor()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.idSeleccionado != -1)
        {
            PlayerManager.Instance.DeseleccionarCarta(PlayerManager.Instance.idSeleccionado);
            PlayerManager.Instance.idSeleccionado = -1;
        }

        EventSystem.current.SetSelectedGameObject(botonesSeleccionarColor[1]);
    }

    public void SeleccionarColor(int colorElegido)
    {
        AudioManager.Instance.PlaySound("buttonPress");
        StartCoroutine(EfectoSeleccionColor(colorElegido));
    }

    private IEnumerator EfectoSeleccionColor(int colorElegido)
    {
        GameObject botonSeleccionado = botonesSeleccionarColor[colorElegido];

        EventSystem.current.SetSelectedGameObject(botonSeleccionado);

        Vector3 escalaOriginal = botonSeleccionado.transform.localScale;
        float duracion = 0.4f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            botonSeleccionado.transform.localScale = Vector3.Lerp(escalaOriginal * 1.2f, escalaOriginal, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        botonSeleccionado.transform.localScale = escalaOriginal;

        switch (colorElegido)
        {
            case 0:
                colorUltimaCarta = "Blue";
                break;
            case 1:
                colorUltimaCarta = "Green";
                break;
            case 2:
                colorUltimaCarta = "Red";
                break;
            case 3:
                colorUltimaCarta = "Yellow";
                break;
        }

        print("Ultima carta seleccionada: " + colorUltimaCarta);
        ActualizarFondo(colorUltimaCarta);
        PlayerManager.Instance.ReorganizarCartasPlayer();

        yield return StartCoroutine(ReducirBotonesYBase());
        seleccionandoColor = false;

        playerTurn = false;
        enemyTurn = true;
        
        //PROVISIONAL
        CambiarTextoTurno(true);
    }

    private IEnumerator ReducirBotonesYBase()
    {
        float duracion = 0.3f;
        float tiempo = 0f;

        Vector3[] escalasIniciales = new Vector3[botonesSeleccionarColor.Length];
        for (int i = 0; i < botonesSeleccionarColor.Length; i++)
        {
            escalasIniciales[i] = botonesSeleccionarColor[i].transform.localScale;
        }

        while (tiempo < duracion)
        {
            for (int i = 0; i < botonesSeleccionarColor.Length; i++)
            {
                botonesSeleccionarColor[i].transform.localScale =
                    Vector3.Lerp(escalasIniciales[i], Vector3.zero, tiempo / duracion);
            }
            tiempo += Time.deltaTime;
            yield return null;
        }

        tiempo = 0f;
        Vector3 escalaInicialBase = baseBotones.transform.localScale;

        while (tiempo < duracion)
        {
            baseBotones.transform.localScale =
                Vector3.Lerp(escalaInicialBase, Vector3.zero, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        foreach (GameObject boton in botonesSeleccionarColor)
        {
            boton.SetActive(false);
        }
        baseBotones.SetActive(false);
    }

    public void ActivarBotonesColor()
    {
        seleccionandoColor = true;
        StartCoroutine(MostrarBotonesConAnimacion());

        foreach (GameObject boton in botonesSeleccionarColor)
        {
            if (!boton.GetComponent<ColorButtonSound>())
            {
                boton.AddComponent<ColorButtonSound>();
            }
        }
    }

    public void DesactivarBotonesColor()
    {
        foreach (GameObject boton in botonesSeleccionarColor)
        {
            boton.transform.localScale = Vector3.zero;
            boton.SetActive(false);
        }
        baseBotones.transform.localScale = Vector3.zero;
        baseBotones.SetActive(false);
    }

    private IEnumerator MostrarBotonesConAnimacion()
    {
        baseBotones.SetActive(true);
        foreach (GameObject boton in botonesSeleccionarColor)
        {
            boton.SetActive(true);
        }

        float duracion = 0.3f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            baseBotones.transform.localScale = Vector3.Lerp(Vector3.zero, escalaOriginalBase, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }
        baseBotones.transform.localScale = escalaOriginalBase;

        tiempo = 0f;
        Vector3[] escalasFinales = new Vector3[botonesSeleccionarColor.Length];

        for (int i = 0; i < botonesSeleccionarColor.Length; i++)
        {
            escalasFinales[i] = escalasOriginalesBotones[i];
        }

        while (tiempo < duracion)
        {
            float fraccion = tiempo / duracion;

            for (int i = 0; i < botonesSeleccionarColor.Length; i++)
            {
                botonesSeleccionarColor[i].transform.localScale = Vector3.Lerp(Vector3.zero, escalasFinales[i], fraccion);
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < botonesSeleccionarColor.Length; i++)
        {
            botonesSeleccionarColor[i].transform.localPosition = posicionesOriginalesBotones[i];
            botonesSeleccionarColor[i].transform.localRotation = rotacionesOriginalesBotones[i];
        }
        baseBotones.transform.localPosition = posicionOriginalBase;
        baseBotones.transform.localRotation = rotacionOriginalBase;
    }

    private void GuardarPosicionesBotonesColor()
    {
        posicionesOriginalesBotones = new Vector3[botonesSeleccionarColor.Length];
        rotacionesOriginalesBotones = new Quaternion[botonesSeleccionarColor.Length];
        escalasOriginalesBotones = new Vector3[botonesSeleccionarColor.Length];

        for (int i = 0; i < botonesSeleccionarColor.Length; i++)
        {
            posicionesOriginalesBotones[i] = botonesSeleccionarColor[i].transform.localPosition;
            rotacionesOriginalesBotones[i] = botonesSeleccionarColor[i].transform.localRotation;
            escalasOriginalesBotones[i] = botonesSeleccionarColor[i].transform.localScale;
        }

        posicionOriginalBase = baseBotones.transform.localPosition;
        rotacionOriginalBase = baseBotones.transform.localRotation;
        escalaOriginalBase = baseBotones.transform.localScale;
    }

    public void EfectosSpecialCards()
    {
        GameObject ultimaCarta = SpawnManager.Instance.ultimaCartaUsada;
        if (ultimaCarta == null) return;

        Renderer renderer = ultimaCarta.GetComponent<Renderer>();
        string[] caracteristicas = renderer.material.name
            .Replace(" (Instance)", "")
            .Split('_');

        string colorUltimaCarta = caracteristicas[0];
        string valorUltimaCarta = (caracteristicas.Length > 1) ? caracteristicas[1] : "";

        if (valorUltimaCarta == "Draw")
        {
            DrawCard(colorUltimaCarta);
        }
        else
        {
            totalCartasRobar = 0;
        }

        if (valorUltimaCarta == "Reverse" || valorUltimaCarta == "Skip")
        {
            ReverseAndSkipCard();
        }
    }

    private void DrawCard(string colorCarta)
    {
        int vecesRobar = (colorCarta == "Wild") ? 4 : 2;
        totalCartasRobar += vecesRobar;

        bool oponenteTieneDraw = playerTurn ? 
            EnemyManager.Instance.TieneCartaDraw() : 
            PlayerManager.Instance.TieneCartaDraw();

        if (!oponenteTieneDraw)
        {
            if (playerTurn)
            {
                for (int i = 0; i < totalCartasRobar; i++)
                    EnemyManager.Instance.RobarCartaEnemigo();
            }
            else
            {
                for (int i = 0; i < totalCartasRobar; i++)
                    PlayerManager.Instance.DrawPlayer();
            }
            totalCartasRobar = 0;
        }
    }

    private void ReverseAndSkipCard()
    {
        if (playerTurn)
        {
            playerTurn = true;
            enemyTurn = false;
        }
        else
        {
            playerTurn = false;
            enemyTurn = true;
        }
    }

    public void ActualizarFondo(string color)
    {
        foreach (GameObject fondo in fondos)
            fondo.SetActive(fondo.name == color);

        ENDdata.Instance.cR++;

        primeraVez = false;
    }

    public void CambiarTextoTurno(bool ignorarReverseSkip = false)
    {
        if (!primeraVez && (ignorarReverseSkip || (valorUltimaCarta != "Reverse" && valorUltimaCarta != "Skip")))
        {
            StartCoroutine(ActualizarTextosTurno(whoText.text == "PLAYER" ? "ENEMY" : "PLAYER"));
        }
    }

    public int ObtenerSiguientePosicion(int totalPosiciones)
    {
        int nuevaPosicion;
        do
        {
            nuevaPosicion = Random.Range(0, totalPosiciones);
        } while (nuevaPosicion == ultimaPosicion);
        
        ultimaPosicion = nuevaPosicion;
        return nuevaPosicion;
    }

    public IEnumerator ActivarGameOverDespuesDeRobar()
    {
        yield return new WaitForSeconds(1f);
        gameOver = true;
    }

    public IEnumerator ActivarGameOverDespuesDeLanzar()
    {
        yield return new WaitForSeconds(0.5f);
        gameOver = true;
    }

    private IEnumerator ActualizarTextosPrimerTurno(string quien)
    {
        yield return new WaitForSeconds(0.8f);
        if (whoText != null)
        {
            string textoCompleto = quien;
            string textoTurn = "TURN";
            whoText.text = "";
            turnText.text = "";
            turnText.transform.localScale = turnTextOriginalScale;
            whoText.transform.localScale = whoTextOriginalScale;

            whoText.color = textoCompleto == "PLAYER" ? Color.green : Color.red;

            if (baseTurno != null && primeraAnimacionTurno)
            {
                float duracionBase = 0.3f;
                float tiempoInicioBase = Time.time;
                Vector3 escalaInicial = new Vector3(0, baseTurnoOriginalScale.y, baseTurnoOriginalScale.z);
                
                while (Time.time - tiempoInicioBase < duracionBase)
                {
                    float avanceExpansion = (Time.time - tiempoInicioBase) / duracionBase;
                    avanceExpansion = avanceExpansion * avanceExpansion * (3f - 2f * avanceExpansion);
                    baseTurno.transform.localScale = Vector3.Lerp(escalaInicial, baseTurnoOriginalScale, avanceExpansion);
                    yield return null;
                }
                baseTurno.transform.localScale = baseTurnoOriginalScale;
                primeraAnimacionTurno = false;
            }
            else if (baseTurno != null)
            {
                baseTurno.transform.localScale = baseTurnoOriginalScale;
            }

            string textoActualWho = "";
            string textoActualTurn = "";

            int maxLength = Mathf.Max(textoCompleto.Length, textoTurn.Length);
            for (int i = 0; i < maxLength; i++)
            {
                if (i < textoCompleto.Length)
                {
                    textoActualWho += textoCompleto[i];
                    whoText.text = textoActualWho;
                }
                if (i < textoTurn.Length)
                {
                    textoActualTurn += textoTurn[i];
                    turnText.text = textoActualTurn;
                }
                yield return new WaitForSeconds(0.05f);
            }

            whoText.text = textoCompleto;
            turnText.text = textoTurn;
        }
    }

    private IEnumerator ActualizarTextosTurno(string who)
    {
        if (whoText != null)
        {
            string textoCompleto = who;

            whoText.color = textoCompleto == "PLAYER" ? Color.green : Color.red;

            whoText.text = "";
            turnText.text = "TURN";
            turnText.transform.localScale = turnTextOriginalScale;
            whoText.transform.localScale = whoTextOriginalScale;

            if (baseTurno != null)
            {
                baseTurno.transform.localScale = baseTurnoOriginalScale;
            }

            string textoActualWho = "";

            for (int i = 0; i < textoCompleto.Length; i++)
            {
                textoActualWho += textoCompleto[i];
                whoText.text = textoActualWho;
                yield return new WaitForSeconds(0.05f);
            }

            whoText.text = textoCompleto;
        }
    }
}

public class ColorButtonSound : MonoBehaviour
{
    private GameObject ultimoBotonSeleccionado;

    void Update()
    {
        GameObject botonSeleccionado = EventSystem.current.currentSelectedGameObject;

        if (botonSeleccionado != null && botonSeleccionado == gameObject)
        {
            if (ultimoBotonSeleccionado != botonSeleccionado)
            {
                AudioManager.Instance.PlaySound("button");
            }
            ultimoBotonSeleccionado = botonSeleccionado;
        }
        else
        {
            ultimoBotonSeleccionado = null;
        }
    }
}