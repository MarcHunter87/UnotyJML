using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static bool esPrimerBoton = true;
    public Button Nueva;
    public Button Cargar;
    public Button Salir;

    void Start()
    {
        Nueva.onClick.AddListener(NuevaPartida);
        Cargar.onClick.AddListener(CargarPartida);
        Salir.onClick.AddListener(SalirJuego);
    }

    public void NuevaPartida()
    {
        StartCoroutine(CargarNuevaPartida());
    }

    private IEnumerator CargarNuevaPartida()
    {
        yield return new WaitForSeconds(0.35f);
        SceneManager.LoadScene("Juego");
    }

    public void CargarPartida()
    {
        StartCoroutine(CargarPartidaIniciada());
    }

    private IEnumerator CargarPartidaIniciada()
    {
        yield return new WaitForSeconds(0.35f);
        print("Pendiente de implementar");
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}