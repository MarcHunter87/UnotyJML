using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public GameObject[] enemyCardsPositions;
    private GameObject[] enemyCards;
    private float velocidadCarta = 10f;
    private bool ultimaCartaFueWild = false;

    void Awake()
    {
        Instance = this;
        enemyCards = new GameObject[enemyCardsPositions.Length];
    }

    void Update()
    {
        if (GameManager.Instance.enemyTurn && !GameManager.Instance.seleccionandoColor)
        {
            StartCoroutine(EnemyTurn());
        }
    }

    private IEnumerator EnemyTurn()
    {
        GameManager.Instance.enemyTurn = false;

        yield return new WaitForSeconds(1.5f);

        GameObject cartaJugable = ObtenerPrimeraCartaJugable();

        if (cartaJugable != null)
        {
            int index = System.Array.IndexOf(enemyCards, cartaJugable);
            yield return StartCoroutine(LanzarCartaEnemigo(index));
        }
        else
        {
            RobarCartaEnemigo();

            GameManager.Instance.CambiarTextoTurno(true);

            yield return new WaitForSeconds(0.3f);

            GameManager.Instance.playerTurn = true;
            GameManager.Instance.enemyTurn = false;
        }
    }

    private GameObject ObtenerPrimeraCartaJugable()
    {
        
        foreach (GameObject carta in enemyCards)
        {
            if (carta != null && CompararCartaValida(carta))
            {
                Renderer rendererCarta = carta.GetComponent<Renderer>();
                string[] datos = rendererCarta.material.name.Replace(" (Instance)", "").Split('_');
                string colorCarta = datos[0];

                if (colorCarta != "Wild" && colorCarta == GameManager.Instance.colorUltimaCarta)
                {
                    return carta;
                }
            }
        }

        foreach (GameObject carta in enemyCards)
        {
            if (carta != null && CompararCartaValida(carta))
            {
                Renderer rendererCarta = carta.GetComponent<Renderer>();
                string[] datos = rendererCarta.material.name.Replace(" (Instance)", "").Split('_');
                string valorCarta = datos.Length > 1 ? datos[1] : "";

                if (valorCarta == GameManager.Instance.valorUltimaCarta)
                {
                    return carta;
                }
            }
        }

        foreach (GameObject carta in enemyCards)
        {
            if (carta != null && CompararCartaValida(carta))
            {
                Renderer rendererCarta = carta.GetComponent<Renderer>();
                string[] datos = rendererCarta.material.name.Replace(" (Instance)", "").Split('_');
                string colorCarta = datos[0];

                if (colorCarta != "Wild")
                {
                    return carta;
                }
            }
        }

        foreach (GameObject carta in enemyCards)
        {
            if (carta != null && CompararCartaValida(carta))
            {
                return carta;
            }
        }

        return null;
    }

    private bool CompararCartaValida(GameObject carta)
    {
        Renderer rendererCartaActual = carta.GetComponent<Renderer>();
        string nombreMaterialActual = rendererCartaActual.material.name.Replace(" (Instance)", "");
        string[] caracteristicasCartaActual = nombreMaterialActual.Split('_');

        string colorCartaActual = caracteristicasCartaActual[0];
        string valorCartaActual = caracteristicasCartaActual.Length > 1 ? caracteristicasCartaActual[1] : null;

        if (GameManager.Instance.totalCartasRobar > 0)
        {
            return valorCartaActual == "Draw";
        }

        return colorCartaActual == GameManager.Instance.colorUltimaCarta || valorCartaActual == GameManager.Instance.valorUltimaCarta || colorCartaActual == "Wild";
    }

    private IEnumerator LanzarCartaEnemigo(int index)
    {
        AudioManager.Instance.PlaySound("selectCard");
        GameObject carta = enemyCards[index];
        int idPosicionesVariables = GameManager.Instance.ObtenerSiguientePosicion(GameManager.Instance.cardThrowVariables.Length);
        GameObject posicionLanzamiento = GameManager.Instance.cardThrowVariables[idPosicionesVariables];

        Vector3 nuevaPosicion = new Vector3(posicionLanzamiento.transform.position.x, posicionLanzamiento.transform.position.y, GameManager.zUltimaCartaLanzada - 0.001f);
        GameManager.zUltimaCartaLanzada = nuevaPosicion.z;

        yield return StartCoroutine(MoverCartaEnemigo(carta, nuevaPosicion, posicionLanzamiento.transform.rotation));

        enemyCards[index] = null;

        SpawnManager.Instance.ultimaCartaUsada = carta;
        Renderer renderer = carta.GetComponent<Renderer>();
        string nombreMaterial = renderer.material.name.Replace(" (Instance)", "");
        string[] caracteristicasCartaActual = nombreMaterial.Split('_');

        string colorCartaActual = caracteristicasCartaActual[0];
        string valorCartaActual = caracteristicasCartaActual.Length > 1 ? caracteristicasCartaActual[1] : null;

        print("Enemigo Carta Lanzada: " + colorCartaActual + " " + valorCartaActual + " y carta anterior: " + GameManager.Instance.colorUltimaCarta + " " + GameManager.Instance.valorUltimaCarta);

        GameManager.Instance.colorUltimaCarta = colorCartaActual;
        GameManager.Instance.valorUltimaCarta = valorCartaActual;

        if (colorCartaActual == "Wild")
        {
            CambiarColorWild();
        }
        else
        {
            GameManager.Instance.ActualizarFondo(colorCartaActual);
        }
        GameManager.Instance.EfectosSpecialCards();
        ReorganizarCartasEnemigo();

        int cartasRestantes = 0;
        foreach (GameObject cartaRestante in enemyCards)
        {
            if (cartaRestante != null) cartasRestantes++;
        }
        
        if (cartasRestantes <= 0)
        {
            ENDdata.Instance.playerWon = false;
            StartCoroutine(GameManager.Instance.ActivarGameOverDespuesDeLanzar());
            yield return null;
        }

        if (GameManager.Instance.valorUltimaCarta != "Reverse" && GameManager.Instance.valorUltimaCarta != "Skip")
        {
            GameManager.Instance.CambiarTextoTurno(false);

            yield return new WaitForSeconds(0.3f);

            GameManager.Instance.playerTurn = true;
            GameManager.Instance.enemyTurn = false;
        }
    }

    private IEnumerator MoverCartaEnemigo(GameObject carta, Vector3 posicionFinal, Quaternion rotacionFinal)
{
    Vector3 posicionInicial = carta.transform.position;
    Quaternion rotacionInicial = carta.transform.rotation;
    float distancia = Vector3.Distance(posicionInicial, posicionFinal);

    if (distancia == 0f)
    {
        carta.transform.rotation = rotacionFinal;
        yield break;
    }

    float tiempoInicio = Time.time;

    while (true)
    {
        float fraccion = (Time.time - tiempoInicio) * velocidadCarta / distancia;
        if (fraccion >= 1) break;

        carta.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, fraccion);
        carta.transform.rotation = Quaternion.Lerp(rotacionInicial, rotacionFinal, fraccion);
        yield return null;
    }

    carta.transform.position = posicionFinal;
    carta.transform.rotation = rotacionFinal;
}

    private void CambiarColorWild()
    {
        Dictionary<string, int> conteoColores = new Dictionary<string, int>()
        {
            {"Blue", 0},
            {"Green", 0},
            {"Red", 0},
            {"Yellow", 0}
        };
        
        foreach (GameObject c in enemyCards)
        {
            if (c == null) continue;
            
            Renderer r = c.GetComponent<Renderer>();string[] datosCarta = r.material.name.Replace(" (Instance)", "").Split('_');
            
            string colorCarta = datosCarta[0];
            if (conteoColores.ContainsKey(colorCarta))
            {
                conteoColores[colorCarta]++;
            }
        }
        
        string colorMasComun = "Blue";
        int maxConteo = 0;
        foreach (var par in conteoColores)
        {
            if (par.Value > maxConteo ||
                (par.Value == maxConteo && Random.Range(0, 2) == 0))
            {
                colorMasComun = par.Key;
                maxConteo = par.Value;
            }
        }
        
        GameManager.Instance.colorUltimaCarta = colorMasComun;
        GameManager.Instance.ActualizarFondo(colorMasComun);
        ultimaCartaFueWild = true;
        PlayerManager.Instance.haSidoWild = false;
    }

    public void ReorganizarCartasEnemigo()
    {
        if (ultimaCartaFueWild)
        {
            ultimaCartaFueWild = false;
        }

        List<GameObject> cartasActivas = new List<GameObject>();
        foreach (GameObject carta in enemyCards)
        {
            if (carta != null) cartasActivas.Add(carta);
        }

        enemyCards = new GameObject[enemyCardsPositions.Length];
        for (int i = 0; i < cartasActivas.Count; i++)
        {
            int posicion = enemyCardsPositions.Length / 2 - cartasActivas.Count / 2 + i;
            enemyCards[posicion] = cartasActivas[i];
            Quaternion rotacionCorrecta = Quaternion.Euler(0, -180, enemyCardsPositions[posicion].transform.rotation.eulerAngles.z);
            StartCoroutine(MoverCartaEnemigo(
                cartasActivas[i],
                enemyCardsPositions[posicion].transform.position,
                rotacionCorrecta
            ));
        }
    }

    public void RobarCartaEnemigo()
    {
        int cartasActuales = 0;
        foreach (GameObject carta in enemyCards)
        {
            if (carta != null) cartasActuales++;
        }
        
        if (cartasActuales >= 13)
        {
            ENDdata.Instance.playerWon = true;
            StartCoroutine(GameManager.Instance.ActivarGameOverDespuesDeRobar());
            return;
        }

        GameObject nuevaCarta = SpawnManager.Instance.GetPrimeraCartaMazo();
        AudioManager.Instance.PlaySound("draw");
        SpawnManager.Instance.SpawnNuevaCarta();

        int posicionCentral = enemyCardsPositions.Length / 2;
        int mejorPosicion = -1;
        int menorDistancia = int.MaxValue;

        for (int i = 0; i < enemyCards.Length; i++)
        {
            if (enemyCards[i] == null)
            {
                int distanciaAlCentro = Mathf.Abs(i - posicionCentral);
                if (distanciaAlCentro < menorDistancia)
                {
                    menorDistancia = distanciaAlCentro;
                    mejorPosicion = i;
                }
            }
        }

        if (mejorPosicion != -1)
        {
            enemyCards[mejorPosicion] = nuevaCarta;
            Quaternion rotacionCorrecta = Quaternion.Euler(0, -180, enemyCardsPositions[mejorPosicion].transform.rotation.eulerAngles.z);
            StartCoroutine(MoverCartaEnemigo(
                nuevaCarta,
                enemyCardsPositions[mejorPosicion].transform.position,
                rotacionCorrecta
            ));
        }

        ReorganizarCartasEnemigo();
    }

    public void CartaEnPosicionEnemiga(int posicion, GameObject carta)
    {
        if (posicion < enemyCards.Length)
        {
            enemyCards[posicion] = carta;
        }
    }

    public bool TieneCartaDraw()
    {
        foreach (GameObject carta in enemyCards)
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