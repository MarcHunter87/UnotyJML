using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private InputSystem_Actions inputActions;
    private bool animacionesHabilitadas = true;

    public GameObject[] playerCardsButtons;
    public GameObject[] playerCards;
    public int idSeleccionado = -1;
    private float posicionCartaSeleccionada = -3.5f;
    private bool posicionamientoInicialCompleto = false;
    private float velocidadCarta = 10f;
    private int idUltimaCartaLanzada = -1;
    private bool ultimaCartaFueWild = false;
    public bool haSidoWild = true;

    void Awake()
    {
        Instance = this;
        inputActions = new InputSystem_Actions();
        
        #if UNITY_ANDROID || UNITY_IOS
            animacionesHabilitadas = false;
        #endif
    }

    void Start()
    {
        playerCards = new GameObject[playerCardsButtons.Length];
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Crouch.performed += RobarCartaDesdeInputSystem;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Crouch.performed -= RobarCartaDesdeInputSystem;
    }

    public void CartaEnBotonConcreto(int boton, GameObject card)
    {
        if (boton < playerCards.Length)
        {
            playerCards[boton] = card;

            if (boton == 0)
            {
                posicionamientoInicialCompleto = true;
            }
        }
    }

    //FORZAR QUE LA FIRST SELECTED DEL EVENT SYSTEM TENGA EL EFECTO DE SELECCIONADA ---------------------------------------------------------------------------------------------

    public void ForzarFirstSelectedCardSeleccionada()
    {
        StartCoroutine(SeleccionarFirstSelectedCard());
    }

    private IEnumerator SeleccionarFirstSelectedCard()
    {
        yield return new WaitForEndOfFrame();

        GameObject botonSeleccionado = EventSystem.current.currentSelectedGameObject;

        if (botonSeleccionado != null)
        {
            int idBoton = System.Array.IndexOf(playerCardsButtons, botonSeleccionado);
            if (idBoton != -1 && playerCards[idBoton] != null)
            {
                SeleccionarCarta(idBoton);
            }
        }
    }

    //UPDATE  -------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void Update()
    {
        if (!posicionamientoInicialCompleto) return;

        GameObject botonSeleccionado = EventSystem.current.currentSelectedGameObject;

        if (botonSeleccionado != null)
        {
            int idBotonSeleccionado = System.Array.IndexOf(playerCardsButtons, botonSeleccionado);

            if (idBotonSeleccionado != -1 && idBotonSeleccionado != idSeleccionado)
            {
                if (idSeleccionado != -1 && playerCards[idSeleccionado] != null)
                {
                    DeseleccionarCarta(idSeleccionado);
                }

                if (playerCards[idBotonSeleccionado] != null)
                {
                    SeleccionarCarta(idBotonSeleccionado);
                }

                idSeleccionado = idBotonSeleccionado;
            }

            Button botonCartaActual = botonSeleccionado.GetComponent<Button>();
            botonCartaActual.onClick.RemoveAllListeners();
            botonCartaActual.onClick.AddListener(() => LanzarCarta(idBotonSeleccionado));
        }
        else if (idSeleccionado != -1)
        {
            if (playerCards[idSeleccionado] != null)
            {
                DeseleccionarCarta(idSeleccionado);
            }

            idSeleccionado = -1;
        }
    }

    //"EFECTOS" AL SELECCIONAR Y DESELECCIONAR CARTAS -----------------------------------------------------------------------------------------------------------------------------

    public void SeleccionarCarta(int index)
    {
        var card = playerCards[index];
        if (!animacionesHabilitadas)
        {
            return;
        }

        var posicionInicial = card.transform.position;
        var posicionFinal = new Vector3(card.transform.position.x, posicionCartaSeleccionada, -1f);
        StartCoroutine(MoverCartaSeleccionadaDeseleccionada(card, posicionInicial, posicionFinal));
    }

    public void DeseleccionarCarta(int index)
    {
        var card = playerCards[index];
        var boton = playerCardsButtons[index];
        if (!animacionesHabilitadas)
        {
            return;
        }

        var posicionInicial = card.transform.position;
        var posicionFinal = new Vector3(boton.transform.position.x, boton.transform.position.y, boton.transform.position.z);
        StartCoroutine(MoverCartaSeleccionadaDeseleccionada(card, posicionInicial, posicionFinal));
    }

    private IEnumerator MoverCartaSeleccionadaDeseleccionada(GameObject carta, Vector3 posicionInicial, Vector3 posicionFinal)
    {
        if (carta == null)
        {
            //NO BORRAR
            yield break;
        }

        float startTime = Time.time;
        float distanciaRecorrer = Vector3.Distance(posicionInicial, posicionFinal);

        if (distanciaRecorrer == 0)
        {
            //NO BORRAR
            yield break;
        }

        AudioManager.Instance.PlaySound("selectCard");

        while (true)
        {
            if (carta == null)
            {
                //NO BORRAR
                yield break;
            }

            float distanciaRecorrida = (Time.time - startTime) * velocidadCarta;
            float fractionOfJourney = distanciaRecorrida / distanciaRecorrer;

            if (fractionOfJourney >= 1f)
            {
                carta.transform.position = posicionFinal;
                break;
            }

            carta.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, fractionOfJourney);
            yield return null;
        }
    }

    //REORGANIZAR CARTAS --------------------------------------------------------------------------------------------------------------------------------------------------------

    public void ReorganizarCartasPlayer()
    {
        int idBotonCentral = playerCardsButtons.Length / 2;
        List<GameObject> cartasActualesJugador = new List<GameObject>();

        foreach (var carta in playerCards)
        {
            if (carta != null)
            {
                cartasActualesJugador.Add(carta);
            }
        }

        int idBotonDesdeCentral = idBotonCentral - (cartasActualesJugador.Count / 2);

        for (int i = 0; i < playerCards.Length; i++)
        {
            playerCards[i] = null;
        }

        float tiempoMaximo = 0f;

        for (int i = 0; i < cartasActualesJugador.Count; i++)
        {
            int botonId = idBotonDesdeCentral + i;
            playerCards[botonId] = cartasActualesJugador[i];
            GameObject botonDestino = playerCardsButtons[botonId];

            GameObject carta = cartasActualesJugador[i];
            float tiempoMovimiento = GetTiempoMovimiento(carta.transform.position, botonDestino.transform.position);

            if (tiempoMovimiento > tiempoMaximo)
            {
                tiempoMaximo = tiempoMovimiento;
            }

            StartCoroutine(MoverCartasAlReorganizar(carta, botonDestino.transform.position));
        }

        StartCoroutine(SeleccionarDespuesDeReorganizar(tiempoMaximo));
    }

    private float GetTiempoMovimiento(Vector3 posicionInicial, Vector3 posicionFinal)
    {
        float distanciaRecorrer = Vector3.Distance(posicionInicial, posicionFinal);
        return distanciaRecorrer / velocidadCarta;
    }

    private IEnumerator MoverCartasAlReorganizar(GameObject carta, Vector3 posicionFinal)
    {
        Vector3 posicionInicial = carta.transform.position;
        float startTime = Time.time;
        float distanciaRecorrer = Vector3.Distance(posicionInicial, posicionFinal);

        if (distanciaRecorrer == 0)
        {
            //NO BORRAR
            yield break;
        }

        while (true)
        {
            float distanciaRecorrida = (Time.time - startTime) * velocidadCarta;
            float fractionOfJourney = distanciaRecorrida / distanciaRecorrer;

            if (fractionOfJourney >= 1f)
            {
                carta.transform.position = posicionFinal;
                break;
            }

            carta.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, fractionOfJourney);
            yield return null;
        }
    }

    public IEnumerator SeleccionarDespuesDeReorganizar(float tiempoMaximo)
    {
        yield return new WaitForSeconds(tiempoMaximo);

        bool quedanCartas = false;
        int lastValidIndex = -1;

        if (ultimaCartaFueWild)
        {
            ultimaCartaFueWild = false;
            GameManager.Instance.SeleccionarPrimerBotonColor();
            yield break;
        }

        for (int i = 0; i < playerCardsButtons.Length; i++)
        {
            Button boton = playerCardsButtons[i].GetComponent<Button>();
            if (playerCards[i] == null)
            {
                var navigation = boton.navigation;
                navigation.mode = Navigation.Mode.None;
                boton.navigation = navigation;
            }
            else
            {
                var navigation = boton.navigation;
                navigation.mode = Navigation.Mode.Horizontal;
                boton.navigation = navigation;
                quedanCartas = true;
                lastValidIndex = i;
            }
        }

        if (!quedanCartas)
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield break;
        }

        GameObject botonSeleccionado = EventSystem.current.currentSelectedGameObject;
        int idBotonSeleccionado = System.Array.IndexOf(playerCardsButtons, botonSeleccionado);

        if (idBotonSeleccionado < 0 || idBotonSeleccionado >= playerCards.Length || playerCards[idBotonSeleccionado] == null)
        {
            EventSystem.current.SetSelectedGameObject(playerCardsButtons[lastValidIndex]);
            SeleccionarCarta(lastValidIndex);
        }
        else if (playerCards[idBotonSeleccionado] != null)
        {
            SeleccionarCarta(idBotonSeleccionado);
        }
    }

    //COMPARAR CARTA PARA SABER SI SE PUEDE LANZAR -------------------------------------------------------------------------------------------------------------------------------

    private bool CompararUltimaCartaLanzadaConActual(GameObject cartaActual)
    {
        Renderer rendererCartaActual = cartaActual.GetComponent<Renderer>();
        string nombreMaterialActual = rendererCartaActual.material.name.Replace(" (Instance)", "");
        string[] caracteristicasCartaActual = nombreMaterialActual.Split('_');

        string colorCartaActual = caracteristicasCartaActual[0];
        string valorCartaActual = caracteristicasCartaActual.Length > 1 ? caracteristicasCartaActual[1] : null;

        if (GameManager.Instance.totalCartasRobar > 0)
        {
            return valorCartaActual == "Draw";
        }

        if (colorCartaActual == "Wild")
        {
            haSidoWild = true;
            return true;
        }

        return (colorCartaActual == GameManager.Instance.colorUltimaCarta || (valorCartaActual != null && valorCartaActual == GameManager.Instance.valorUltimaCarta));
    }


    private IEnumerator SacudirCartaNoValida(GameObject carta)
    {
        int index = -1;
        Button boton = null;

        for (int i = 0; i < playerCards.Length; i++)
        {
            if (playerCards[i] == carta)
            {
                index = i;
                break;
            }
        }

        if (index == -1) yield break;

        GameObject buttonObj = playerCardsButtons[index];

        boton = buttonObj.GetComponent<Button>();

        var navigation = boton.navigation;
        navigation.mode = Navigation.Mode.None;
        boton.navigation = navigation;

        float duracionSacudida = 0.3f;
        float magnitudSacudida = 0.1f;
        float velocidadReduccionEfecto = 1.0f;

        if (carta == null) yield break;

        Vector3 posicionOriginal = carta.transform.position;
        float tiempoTranscurrido = 0.0f;

        AudioManager.Instance.PlaySound("error");

        while (tiempoTranscurrido < duracionSacudida)
        {
            if (carta == null) yield break;

            float x = posicionOriginal.x + Random.Range(-1f, 1f) * magnitudSacudida;
            float y = posicionOriginal.y + Random.Range(-1f, 1f) * magnitudSacudida;

            float magnitudActual = magnitudSacudida * (1.0f - (tiempoTranscurrido / duracionSacudida));
            carta.transform.position = new Vector3(
                posicionOriginal.x + Mathf.Sin(Time.time * 50) * magnitudActual,
                posicionOriginal.y + Mathf.Sin(Time.time * 50) * magnitudActual,
                posicionOriginal.z
            );

            tiempoTranscurrido += Time.deltaTime * velocidadReduccionEfecto;
            yield return null;
        }

        if (carta != null)
        {
            carta.transform.position = posicionOriginal;
        }

        navigation.mode = Navigation.Mode.Horizontal;
        boton.navigation = navigation;
    }

    //LANZAR CARTA --------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void LanzarCarta(int index)
    {
        if (!GameManager.Instance.playerTurn) return;

        if (index < 0 || index >= playerCards.Length || playerCards[index] == null)
        {
            return;
        }

        GameObject carta = playerCards[index];
        if (!CompararUltimaCartaLanzadaConActual(carta))
        {
            StartCoroutine(SacudirCartaNoValida(carta));
            return;
        }

        int idPosicionesVariables = GameManager.Instance.ObtenerSiguientePosicion(GameManager.Instance.cardThrowVariables.Length);
        GameObject PosicionAlLanzar = GameManager.Instance.cardThrowVariables[idPosicionesVariables];

        Vector3 nuevaPosicion = new Vector3(PosicionAlLanzar.transform.position.x, PosicionAlLanzar.transform.position.y, GameManager.zUltimaCartaLanzada - 0.001f);
        GameManager.zUltimaCartaLanzada = nuevaPosicion.z;

        SpawnManager.Instance.ultimaCartaUsada = carta;
        Renderer renderer = carta.GetComponent<Renderer>();
        string nombreMaterial = renderer.material.name.Replace(" (Instance)", "");
        string[] caracteristicasCartaActual = nombreMaterial.Split('_');

        string colorCartaActual = caracteristicasCartaActual[0];
        string valorCartaActual = caracteristicasCartaActual.Length > 1 ? caracteristicasCartaActual[1] : null;

        switch (colorCartaActual)
        {
            case "Blue":
                ENDdata.Instance.Blue++;
                break;
            case "Green":
                ENDdata.Instance.Green++;
                break;
            case "Red":
                ENDdata.Instance.Red++;
                break;
            case "Yellow":
                ENDdata.Instance.Yellow++;
                break;
            case "Wild":
                ENDdata.Instance.Wild++;
                break;
        }

        ENDdata.Instance.cCP++;

        print("Jugador Carta Lanzada: " + colorCartaActual + " " + valorCartaActual + " y carta anterior: " + GameManager.Instance.colorUltimaCarta + " " + GameManager.Instance.valorUltimaCarta);

        GameManager.Instance.colorUltimaCarta = colorCartaActual;
        GameManager.Instance.valorUltimaCarta = valorCartaActual;

        if (colorCartaActual == "Blue" || colorCartaActual == "Green" || colorCartaActual == "Red" || colorCartaActual == "Yellow")
        {
            GameManager.Instance.ActualizarFondo(colorCartaActual);
            haSidoWild = false;
        }

        if (colorCartaActual == "Wild")
        {
            GameManager.Instance.seleccionandoColor = true;
            ultimaCartaFueWild = true;
            GameManager.Instance.ActivarBotonesColor();
            GameManager.Instance.SeleccionarPrimerBotonColor();
        }

        StartCoroutine(MoverCartaAlLanzar(carta, nuevaPosicion, PosicionAlLanzar.transform.rotation));
        SpawnManager.Instance.ultimaCartaUsada = carta;

        idUltimaCartaLanzada = idPosicionesVariables;
        playerCards[index] = null;

        GameManager.Instance.EfectosSpecialCards();
        ReorganizarCartasPlayer();

        int cartasRestantes = 0;
        foreach (GameObject cartaRestante in playerCards)
        {
            if (cartaRestante != null) cartasRestantes++;
        }
        
        if (cartasRestantes <= 0)
        {
            ENDdata.Instance.playerWon = true;
            StartCoroutine(GameManager.Instance.ActivarGameOverDespuesDeLanzar());
            return;
        }

        if (GameManager.Instance.valorUltimaCarta != "Reverse" && GameManager.Instance.valorUltimaCarta != "Skip" && colorCartaActual != "Wild")
        {
            GameManager.Instance.playerTurn = false;
            GameManager.Instance.enemyTurn = true;
            GameManager.Instance.CambiarTextoTurno(false);
        }
    }

    private IEnumerator MoverCartaAlLanzar(GameObject card, Vector3 posicionFinal, Quaternion rotacionFinal)
    {
        Vector3 posicionInicial = card.transform.position;
        Quaternion rotacionInicial = card.transform.rotation;

        float distanciaRecorrer = Vector3.Distance(posicionInicial, posicionFinal);
        float startTime = Time.time;

        AudioManager.Instance.PlaySound("draw");
        
        while (true)
        {
            float distanciaRecorrida = (Time.time - startTime) * velocidadCarta;
            float fractionOfJourney = distanciaRecorrida / distanciaRecorrer;

            if (fractionOfJourney >= 1f)
            {
                card.transform.position = posicionFinal;
                card.transform.rotation = rotacionFinal;
                break;
            }

            card.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, fractionOfJourney);
            card.transform.rotation = Quaternion.Lerp(rotacionInicial, rotacionFinal, fractionOfJourney);

            yield return null;
        }
    }

    //COMPARAR CARTA PARA SABER SI SE PUEDE ROBAR -------------------------------------------------------------------------------------------------------------------------------
    private bool CompararUltimaCartaLanzadaConActualesPlayer()
    {
        foreach (GameObject c in playerCards)
        {
            if (c != null && CompararUltimaCartaLanzadaConActual(c))
            {
                return true;
            }
        }
        return false;
    }

    private bool estaSacudiendo = false;

    private IEnumerator SacudirMazo()
    {
        if (estaSacudiendo) yield break;
        estaSacudiendo = true;

        GameObject primeraCartaMazo = SpawnManager.Instance.GetPrimeraCartaMazoSinRemove();
        if (primeraCartaMazo == null)
        {
            estaSacudiendo = false;
            yield break;
        }

        float duracionSacudida = 0.3f;
        float magnitudSacudida = 0.22f;
        float velocidadReduccionEfecto = 1.0f;

        Vector3 posicionOriginal = primeraCartaMazo.transform.position;
        float tiempoTranscurrido = 0.0f;

        AudioManager.Instance.PlaySound("error");

        while (tiempoTranscurrido < duracionSacudida)
        {
            if (primeraCartaMazo == null)
            {
                estaSacudiendo = false;
                yield break;
            }

            float magnitudActual = magnitudSacudida * (1.0f - (tiempoTranscurrido / duracionSacudida));
            primeraCartaMazo.transform.position = new Vector3(
                posicionOriginal.x + Mathf.Sin(Time.time * 50) * magnitudActual,
                posicionOriginal.y + Mathf.Sin(Time.time * 50) * magnitudActual,
                posicionOriginal.z
            );

            tiempoTranscurrido += Time.deltaTime * velocidadReduccionEfecto;
            yield return null;
        }

        if (primeraCartaMazo != null)
        {
            primeraCartaMazo.transform.position = posicionOriginal;
        }

        estaSacudiendo = false;
    }


    //ROBAR CARTAS --------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void RobarCartaPlayer()
    {
        if (!GameManager.Instance.playerTurn || estaSacudiendo) return;
        if (estaSacudiendo) return;

        int cartasActuales = 0;
        foreach (GameObject cartaActual in playerCards)
        {
            if (cartaActual != null) cartasActuales++;
        }
        
        ENDdata.Instance.cDC++;

        if (cartasActuales >= 13)
        {
            ENDdata.Instance.playerWon = false;
            StartCoroutine(GameManager.Instance.ActivarGameOverDespuesDeRobar());
            return;
        }

        if (CompararUltimaCartaLanzadaConActualesPlayer())
        {
            StartCoroutine(SacudirMazo());
            return;
        }
        GameObject carta = SpawnManager.Instance.GetPrimeraCartaMazo();
        if (carta == null) return;

        AudioManager.Instance.PlaySound("draw");
        SpawnManager.Instance.SpawnNuevaCarta();

        int idBotonCentral = playerCardsButtons.Length / 2;
        int botonSeleccionado = -1;

        for (int i = 0; i <= playerCardsButtons.Length / 2; i++)
        {
            int botonIzquierda = idBotonCentral - i;
            int botonDerecha = idBotonCentral + i;

            if (botonIzquierda >= 0 && playerCards[botonIzquierda] == null)
            {
                botonSeleccionado = botonIzquierda;
                break;
            }

            if (botonDerecha < playerCardsButtons.Length && playerCards[botonDerecha] == null)
            {
                botonSeleccionado = botonDerecha;
                break;
            }
        }

        if (botonSeleccionado != -1)
        {
            GameObject botonDestino = playerCardsButtons[botonSeleccionado];
            float tiempoMovimiento = GetTiempoMovimiento(carta.transform.position, botonDestino.transform.position);

            StartCoroutine(MoverCartaDesdeMazo(carta, botonDestino));
            playerCards[botonSeleccionado] = carta;
            StartCoroutine(SeleccionarDespuesDeReorganizar(tiempoMovimiento));
        }
        GameManager.Instance.playerTurn = false;
        GameManager.Instance.enemyTurn = true;
        GameManager.Instance.CambiarTextoTurno(true);
    }

    private IEnumerator MoverCartaDesdeMazo(GameObject carta, GameObject botonDestino)
    {
        Vector3 posicionInicial = carta.transform.position;
        Vector3 posicionFinal = new Vector3(
            botonDestino.transform.position.x,
            botonDestino.transform.position.y,
            botonDestino.transform.position.z
        );
        Quaternion rotacionInicial = carta.transform.rotation;
        Quaternion rotacionFinal = botonDestino.transform.rotation;

        float distanciaRecorrer = Vector3.Distance(posicionInicial, posicionFinal);
        float tiempoInicio = Time.time;

        while (true)
        {
            float distanciaRecorrida = (Time.time - tiempoInicio) * velocidadCarta;
            float fractionOfJourney = distanciaRecorrida / distanciaRecorrer;

            if (fractionOfJourney >= 1f)
            {
                carta.transform.position = posicionFinal;
                carta.transform.rotation = rotacionFinal;

                int idBoton = System.Array.IndexOf(playerCardsButtons, botonDestino);
                CartaEnBotonConcreto(idBoton, carta);

                break;
            }

            carta.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, fractionOfJourney);
            carta.transform.rotation = Quaternion.Lerp(rotacionInicial, rotacionFinal, fractionOfJourney);

            yield return null;
        }
    }

    private void RobarCartaDesdeInputSystem(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RobarCartaPlayer();
        }
    }

    public void RobarCartaDesdeBoton()
    {
        RobarCartaPlayer();
    }

    public void DrawPlayer()
    {
        int cartasActuales = 0;
        foreach (GameObject cartaActual in playerCards)
        {
            if (cartaActual != null) cartasActuales++;
        }
        
        ENDdata.Instance.cDC++;

        if (cartasActuales >= 13)
        {
            ENDdata.Instance.playerWon = false;
            StartCoroutine(GameManager.Instance.ActivarGameOverDespuesDeRobar());
            return;
        }

        GameObject carta = SpawnManager.Instance.GetPrimeraCartaMazo();
        if (carta == null) return;

        SpawnManager.Instance.SpawnNuevaCarta();

        int idBotonCentral = playerCardsButtons.Length / 2;
        int botonSeleccionado = -1;

        for (int i = 0; i <= playerCardsButtons.Length / 2; i++)
        {
            int botonIzquierda = idBotonCentral - i;
            int botonDerecha = idBotonCentral + i;

            if (botonIzquierda >= 0 && playerCards[botonIzquierda] == null)
            {
                botonSeleccionado = botonIzquierda;
                break;
            }

            if (botonDerecha < playerCardsButtons.Length && playerCards[botonDerecha] == null)
            {
                botonSeleccionado = botonDerecha;
                break;
            }
        }

        if (botonSeleccionado != -1)
        {
            GameObject botonDestino = playerCardsButtons[botonSeleccionado];
            float tiempoMovimiento = GetTiempoMovimiento(carta.transform.position, botonDestino.transform.position);

            StartCoroutine(MoverCartaDesdeMazo(carta, botonDestino));
            playerCards[botonSeleccionado] = carta;
            StartCoroutine(SeleccionarDespuesDeReorganizar(tiempoMovimiento));
        }
        //PROBABLEMENTE ARREGLA EL CAMBIO DE TEXTO DE TURNO
        // GameManager.Instance.playerTurn = false;
        // GameManager.Instance.enemyTurn = true;
        // GameManager.Instance.CambiarTextoTurno(true);
    }

    public bool TieneCartaDraw()
    {
        foreach (GameObject carta in playerCards)
        {
            if (carta != null)
            {
                Renderer renderer = carta.GetComponent<Renderer>();
                string[] caracteristicas = renderer.material.name.Replace(" (Instance)", "").Split('_');
                if (caracteristicas.Length > 1 && caracteristicas[1] == "Draw")
                {
                    return true;
                }
            }
        }
        return false;
    }
}
