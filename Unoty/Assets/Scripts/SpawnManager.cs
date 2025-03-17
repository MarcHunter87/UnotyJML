using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public GameObject[] playerCardsButtons;
    public GameObject[] enemyCardsPositions;
    public GameObject[] cardPrefabs;
    private float initialCardsSpawnTimer = 0.01f;
    private int initialCards;
    private float velocidadCarta = 10f;
    private int idCartaActual = 0;
    private int cartasJugadorInicializadas = 0;
    public GameObject posicionPrimeraCartaTablero;
    public GameObject ultimaCartaUsada = null;

    private List<GameObject> mazo = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartSpawn();
    }

    public void StartSpawn()
    {
        initialCards = 40;
        idCartaActual = 0;
        InvokeRepeating("SpawnInitialCards", 0f, initialCardsSpawnTimer);
    }

    private void SpawnInitialCards()
    {
        if (initialCards <= 0)
        {
            CancelInvoke("SpawnInitialCards");
            return;
        }

        int numAleatorio = Random.Range(0, cardPrefabs.Length);
        GameObject cardPrefab = cardPrefabs[numAleatorio];
        GameObject spawnedCard = Instantiate(cardPrefab, transform.position, Quaternion.Euler(0, -180, 90));
        mazo.Add(spawnedCard);

        initialCards--;

        if (idCartaActual < 7)
        {
            StartCoroutine(MoverCartasGeneradasInicialesEnemigo(spawnedCard, idCartaActual + 3));
            idCartaActual++;
        }
        else if (idCartaActual < 14)
        {
            StartCoroutine(MoverCartasGeneradasInicialesJugador(spawnedCard, idCartaActual - 4));
            idCartaActual++;
        }

        if (initialCards == 0)
        {
            for (int i = 0; i < playerCardsButtons.Length; i++)
            {
                if (playerCardsButtons[i] != null && PlayerManager.Instance != null && mazo.Count > 0)
                {
                    PlayerManager.Instance.CartaEnBotonConcreto(i, mazo[0]);
                    Destroy(mazo[0]);
                    mazo.RemoveAt(0);
                }
            }
            StartCoroutine(MoverPrimeraCartaPartida(spawnedCard, posicionPrimeraCartaTablero));
        }
    }

    public void SpawnNuevaCarta()
    {
        int numAleatorio = Random.Range(0, cardPrefabs.Length);
        GameObject cardPrefab = cardPrefabs[numAleatorio];
        GameObject spawnedCard = Instantiate(cardPrefab, transform.position, Quaternion.Euler(0, -180, 90));
        mazo.Add(spawnedCard);
    }

    private IEnumerator MoverCartasGeneradasInicialesEnemigo(GameObject card, int idPosicion)
    {
        if (idPosicion >= enemyCardsPositions.Length || enemyCardsPositions[idPosicion] == null)
        {
            yield break;
        }

        AudioManager.Instance.PlaySound("sendInitial");
        GameObject square = enemyCardsPositions[idPosicion];
        Vector3 posicionInicial = card.transform.position;
        Quaternion rotacionInicial = card.transform.rotation;
        Vector3 posicionFinal = new Vector3(
            square.transform.position.x,
            square.transform.position.y,
            square.transform.position.z
        );

        Quaternion rotacionDelSquare = Quaternion.Euler(0, -180, square.transform.rotation.eulerAngles.z);

        float distanciaRecorrer = Vector3.Distance(posicionInicial, posicionFinal);
        float tiempoInicio = Time.time;

        while (true)
        {
            float distanciaRecorrida = (Time.time - tiempoInicio) * velocidadCarta;
            float fractionOfJourney = distanciaRecorrida / distanciaRecorrer;

            mazo.Remove(card);

            if (fractionOfJourney >= 1f)
            {
                card.transform.position = posicionFinal;
                card.transform.rotation = rotacionDelSquare;

                break;
            }

            card.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, fractionOfJourney);
            card.transform.rotation = Quaternion.Lerp(rotacionInicial, rotacionDelSquare, fractionOfJourney);

            yield return null;
        }
        EnemyManager.Instance.CartaEnPosicionEnemiga(idPosicion, card);
    }

    private IEnumerator MoverCartasGeneradasInicialesJugador(GameObject card, int idBoton)
    {
        if (idBoton >= playerCardsButtons.Length || playerCardsButtons[idBoton] == null)
        {
            yield break;
        }

        AudioManager.Instance.PlaySound("sendInitial");
        GameObject boton = playerCardsButtons[idBoton];
        Vector3 posicionInicial = card.transform.position;
        Quaternion rotacionInicial = card.transform.rotation;
        Vector3 posicionFinal = new Vector3(
            boton.transform.position.x,
            boton.transform.position.y,
            boton.transform.position.z
        );
        Quaternion rotacionDelBoton = boton.transform.rotation;

        float distanciaRecorrer = Vector3.Distance(posicionInicial, posicionFinal);
        float tiempoInicio = Time.time;

        while (true)
        {
            float distanciaRecorrida = (Time.time - tiempoInicio) * velocidadCarta;
            float fractionOfJourney = distanciaRecorrida / distanciaRecorrer;

            mazo.Remove(card);

            if (fractionOfJourney >= 1f)
            {
                card.transform.position = posicionFinal;
                card.transform.rotation = rotacionDelBoton;

                PlayerManager.Instance.CartaEnBotonConcreto(idBoton, card);

                cartasJugadorInicializadas++;

                if (cartasJugadorInicializadas == 7)
                {
                    PlayerManager.Instance.ForzarFirstSelectedCardSeleccionada();
                }

                break;
            }

            card.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, fractionOfJourney);
            card.transform.rotation = Quaternion.Lerp(rotacionInicial, rotacionDelBoton, fractionOfJourney);

            yield return null;
        }
    }

    private IEnumerator MoverPrimeraCartaPartida(GameObject card, GameObject posicionPrimeraCartaTablero)
    {
        Renderer renderer = card.GetComponent<Renderer>();
        string nombreMaterial = renderer.material.name.Replace(" (Instance)", "");
        string[] caracteristicasCarta = nombreMaterial.Split('_');
        GameManager.Instance.colorUltimaCarta = caracteristicasCarta[0];
        GameManager.Instance.valorUltimaCarta = caracteristicasCarta.Length > 1 ? caracteristicasCarta[1] : "";

        string[] valoresProhibidos = { "Draw", "Reverse", "Skip" };

        while (GameManager.Instance.colorUltimaCarta == "Wild" || System.Array.Exists(valoresProhibidos, v => v == GameManager.Instance.valorUltimaCarta))
        {
            mazo.Remove(card);
            Destroy(card);

            if (mazo.Count == 0)
            {
                yield break;
            }

            card = mazo[0];
            mazo.RemoveAt(0);

            renderer = card.GetComponent<Renderer>();
            nombreMaterial = renderer.material.name.Replace(" (Instance)", "");
            caracteristicasCarta = nombreMaterial.Split('_');
            GameManager.Instance.colorUltimaCarta = caracteristicasCarta[0];
            GameManager.Instance.valorUltimaCarta = caracteristicasCarta.Length > 1 ? caracteristicasCarta[1] : "";
        }

        ultimaCartaUsada = card;

        if (GameManager.Instance.colorUltimaCarta == "Blue" || GameManager.Instance.colorUltimaCarta == "Green" || GameManager.Instance.colorUltimaCarta == "Red" || GameManager.Instance.colorUltimaCarta == "Yellow")
        {
            GameManager.Instance.ActualizarFondo(GameManager.Instance.colorUltimaCarta);
        }

        Vector3 posicionInicial = card.transform.position;
        Quaternion rotacionInicial = card.transform.rotation;
        Vector3 posicionFinal = new Vector3(
            posicionPrimeraCartaTablero.transform.position.x,
            posicionPrimeraCartaTablero.transform.position.y,
            posicionPrimeraCartaTablero.transform.position.z
        );

        GameManager.Instance.ultimaPosicion = System.Array.IndexOf(GameManager.Instance.cardThrowVariables, posicionPrimeraCartaTablero);

        Quaternion rotacionDelSquare = Quaternion.Euler(0, 0, posicionPrimeraCartaTablero.transform.rotation.eulerAngles.z);

        float distanciaRecorrer = Vector3.Distance(posicionInicial, posicionFinal);
        float tiempoInicio = Time.time;

        while (true)
        {
            float distanciaRecorrida = (Time.time - tiempoInicio) * velocidadCarta;
            float fractionOfJourney = distanciaRecorrida / distanciaRecorrer;

            mazo.Remove(card);

            if (fractionOfJourney >= 1f)
            {
                card.transform.position = posicionFinal;
                card.transform.rotation = rotacionDelSquare;
                break;
            }

            card.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, fractionOfJourney);
            card.transform.rotation = Quaternion.Lerp(rotacionInicial, rotacionDelSquare, fractionOfJourney);

            yield return null;
        }
    }

    public GameObject GetPrimeraCartaMazo()
    {
        if (mazo.Count == 0)
            return null;
            
        GameObject primeraCarta = mazo[0];
        mazo.RemoveAt(0);
        return primeraCarta;
    }

    public GameObject GetPrimeraCartaMazoSinRemove()
    {
        if (mazo.Count == 0)
            return null;
        return mazo[0];
    }

    public void StopSpawn()
    {
        CancelInvoke("SpawnInitialCards");
    }
}
