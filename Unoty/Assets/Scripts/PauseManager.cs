using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    private InputSystem_Actions inputActions;
    public GameObject pauseMenu;
    public GameObject pauseBackground;
    private bool isPaused = false;
    private float duracionAnimacion = 0.4f;
    private float duracionDesaparicion = 0.2f;
    private Vector3 escalaOriginal;
    private bool animacionEnProgreso = false;

    public GameObject[] basesButtons;
    public Button Continue;
    public Button Restart;
    public Button Menu;
    public Button Pausa;

    private Vector3[] escalasOriginalesBotones;
    private Vector3 escalaOriginalContinue;
    private Vector3 escalaOriginalRestart;
    private Vector3 escalaOriginalMenu;
    private GameObject botonSeleccionadoAntesDePausa;

    private Vector3 escalaOriginalPausa;
    public GameObject iconoPausa;
    private SpriteRenderer iconoPausaSpriteRenderer;

    void Start()
    {
        escalaOriginal = pauseMenu.transform.localScale;
        escalasOriginalesBotones = new Vector3[basesButtons.Length];
        for (int i = 0; i < basesButtons.Length; i++)
        {
            escalasOriginalesBotones[i] = basesButtons[i].transform.localScale;
        }

        ConfigurarAnimators();

        Pausa.onClick.RemoveAllListeners();
        Pausa.onClick.AddListener(AbrirMenuPausaDesdeBoton);

        escalaOriginalPausa = Pausa.transform.localScale;
        Pausa.transform.localScale = Vector3.zero;

        pauseMenu.SetActive(false);
        pauseBackground.SetActive(false);
        Continue.gameObject.SetActive(false);
        Restart.gameObject.SetActive(false);
        Menu.gameObject.SetActive(false);
        foreach (GameObject button in basesButtons)
        {
            button.SetActive(false);
        }

        escalaOriginalContinue = Continue.transform.localScale;
        escalaOriginalRestart = Restart.transform.localScale;
        escalaOriginalMenu = Menu.transform.localScale;

        StartCoroutine(MostrarBotonPausa());
    }

    private void ConfigurarAnimators()
    {
        Animator continueAnimator = Continue.GetComponent<Animator>();
        if (continueAnimator != null)
        {
            continueAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        Animator restartAnimator = Restart.GetComponent<Animator>();
        if (restartAnimator != null)
        {
            restartAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        Animator menuAnimator = Menu.GetComponent<Animator>();
        if (menuAnimator != null)
        {
            menuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    void Update()
    {
        if (!isPaused) return;

        GameObject botonSeleccionado = EventSystem.current.currentSelectedGameObject;

        if (botonSeleccionado != null)
        {
            if (botonSeleccionado == Continue.gameObject)
            {
                Continue.onClick.RemoveAllListeners();
                Continue.onClick.AddListener(ReanudarPartida);
            }
            else if (botonSeleccionado == Restart.gameObject)
            {
                Restart.onClick.RemoveAllListeners();
                Restart.onClick.AddListener(PlayAgain);
            }
            else if (botonSeleccionado == Menu.gameObject)
            {
                Menu.onClick.RemoveAllListeners();
                Menu.onClick.AddListener(RegresoAlMenu);
            }
        }
    }

    void Awake()
    {
        Instance = this;
        inputActions = new InputSystem_Actions();
        iconoPausaSpriteRenderer = iconoPausa.GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Pause.performed += AbrirMenuPausa;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Pause.performed -= AbrirMenuPausa;
    }

    public void ReanudarPartida()
    {
        if (animacionEnProgreso) return;

        MenuManager.esPrimerBoton = true;
        ResetearAnimacionesBotones();

        isPaused = false;
        Time.timeScale = 1f;
        StartCoroutine(EfectoDesaparicionMenu());
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        ENDdata.Instance.ResetearValores();
        StartCoroutine(CargarEscena("Juego"));
    }

    public void RegresoAlMenu()
    {
        Time.timeScale = 1f;
        ENDdata.Instance.ResetearValores();
        StartCoroutine(CargarEscena("Menu Principal"));
    }

    private IEnumerator CargarEscena(string nombreEscena)
    {
        yield return new WaitForSeconds(0.35f);
        SceneManager.LoadScene(nombreEscena);
    }

    private void AbrirMenuPausa(InputAction.CallbackContext context)
    {
        if (!animacionEnProgreso)
        {
            if (isPaused)
            {
                ReanudarPartida();
            }
            else
            {
                MenuManager.esPrimerBoton = true;
                PausarPartida();
            }
        }
    }

    private void ResetearAnimacionesBotones()
    {
        Animator continueAnimator = Continue.GetComponent<Animator>();
        if (continueAnimator != null)
        {
            continueAnimator.Play("Seleccionado", 0, 0f);
            continueAnimator.SetBool("seleccionado", false);
        }

        Animator restartAnimator = Restart.GetComponent<Animator>();
        if (restartAnimator != null)
        {
            restartAnimator.Play("Seleccionado", 0, 0f);
            restartAnimator.SetBool("seleccionado", false);
        }

        Animator menuAnimator = Menu.GetComponent<Animator>();
        if (menuAnimator != null)
        {
            menuAnimator.Play("Seleccionado", 0, 0f);
            menuAnimator.SetBool("seleccionado", false);
        }
    }

    public void PausarPartida()
    {
        if (animacionEnProgreso) return;

        botonSeleccionadoAntesDePausa = EventSystem.current.currentSelectedGameObject;

        isPaused = true;
        Time.timeScale = 0;
        pauseBackground.SetActive(true);
        pauseMenu.SetActive(true);
        Continue.gameObject.SetActive(true);
        Restart.gameObject.SetActive(true);
        Menu.gameObject.SetActive(true);
        foreach (GameObject button in basesButtons)
        {
            button.SetActive(true);
            button.transform.localScale = Vector3.zero;
        }
        pauseMenu.transform.localScale = Vector3.zero;

        Color color = iconoPausaSpriteRenderer.color;
        color.a = 0.5f;
        iconoPausaSpriteRenderer.color = color;

        var continueNav = Continue.navigation;
        var restartNav = Restart.navigation;
        var menuNav = Menu.navigation;

        continueNav.mode = Navigation.Mode.Explicit;
        restartNav.mode = Navigation.Mode.Explicit;
        menuNav.mode = Navigation.Mode.Explicit;

        continueNav.selectOnDown = Restart;
        restartNav.selectOnUp = Continue;
        restartNav.selectOnDown = Menu;
        menuNav.selectOnUp = Restart;

        Continue.navigation = continueNav;
        Restart.navigation = restartNav;
        Menu.navigation = menuNav;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Continue.gameObject);
        
        StartCoroutine(EfectoAparicionMenu());
    }

    private IEnumerator EfectoAparicionMenu()
    {
        animacionEnProgreso = true;
        float tiempo = 0f;
        float mitadDuracion = duracionAnimacion / 2f;

        AudioManager.Instance.PlaySound("pause");

        while (tiempo < mitadDuracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float progreso = tiempo / mitadDuracion;
            progreso = Mathf.SmoothStep(0f, 1f, progreso);
            Vector3 nuevaEscala = Vector3.Lerp(Vector3.zero, escalaOriginal * 1.1f, progreso);
            pauseMenu.transform.localScale = nuevaEscala;
            for (int i = 0; i < basesButtons.Length; i++)
            {
                basesButtons[i].transform.localScale = Vector3.Lerp(Vector3.zero, escalasOriginalesBotones[i] * 1.1f, progreso);
            }
            Continue.transform.localScale = Vector3.Lerp(Vector3.zero, escalaOriginalContinue * 1.1f, progreso);
            Restart.transform.localScale = Vector3.Lerp(Vector3.zero, escalaOriginalRestart * 1.1f, progreso);
            Menu.transform.localScale = Vector3.Lerp(Vector3.zero, escalaOriginalMenu * 1.1f, progreso);
            yield return null;
        }

        tiempo = 0f;
        while (tiempo < mitadDuracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float progreso = tiempo / mitadDuracion;
            progreso = Mathf.SmoothStep(0f, 1f, progreso);
            Vector3 nuevaEscala = Vector3.Lerp(escalaOriginal * 1.1f, escalaOriginal, progreso);
            pauseMenu.transform.localScale = nuevaEscala;
            for (int i = 0; i < basesButtons.Length; i++)
            {
                basesButtons[i].transform.localScale = Vector3.Lerp(escalasOriginalesBotones[i] * 1.1f, escalasOriginalesBotones[i], progreso);
            }
            Continue.transform.localScale = Vector3.Lerp(escalaOriginalContinue * 1.1f, escalaOriginalContinue, progreso);
            Restart.transform.localScale = Vector3.Lerp(escalaOriginalRestart * 1.1f, escalaOriginalRestart, progreso);
            Menu.transform.localScale = Vector3.Lerp(escalaOriginalMenu * 1.1f, escalaOriginalMenu, progreso);
            yield return null;
        }

        pauseMenu.transform.localScale = escalaOriginal;
        for (int i = 0; i < basesButtons.Length; i++)
        {
            basesButtons[i].transform.localScale = escalasOriginalesBotones[i];
        }
        Continue.transform.localScale = escalaOriginalContinue;
        Restart.transform.localScale = escalaOriginalRestart;
        Menu.transform.localScale = escalaOriginalMenu;
        animacionEnProgreso = false;
    }

    private IEnumerator EfectoDesaparicionMenu()
    {
        animacionEnProgreso = true;
        float tiempo = 0f;

        AudioManager.Instance.PlaySound("pause");

        while (tiempo < duracionDesaparicion)
        {
            tiempo += Time.unscaledDeltaTime;
            float progreso = tiempo / duracionDesaparicion;
            progreso = Mathf.SmoothStep(0f, 1f, progreso);
            Vector3 nuevaEscala = Vector3.Lerp(escalaOriginal, Vector3.zero, progreso);
            pauseMenu.transform.localScale = nuevaEscala;
            for (int i = 0; i < basesButtons.Length; i++)
            {
                basesButtons[i].transform.localScale = Vector3.Lerp(escalasOriginalesBotones[i], Vector3.zero, progreso);
            }
            Continue.transform.localScale = Vector3.Lerp(escalaOriginalContinue, Vector3.zero, progreso);
            Restart.transform.localScale = Vector3.Lerp(escalaOriginalRestart, Vector3.zero, progreso);
            Menu.transform.localScale = Vector3.Lerp(escalaOriginalMenu, Vector3.zero, progreso);
            yield return null;
        }

        ResetearAnimacionesBotones();

        pauseMenu.transform.localScale = Vector3.zero;
        for (int i = 0; i < basesButtons.Length; i++)
        {
            basesButtons[i].transform.localScale = Vector3.zero;
        }
        pauseMenu.SetActive(false);
        pauseBackground.SetActive(false);
        Continue.gameObject.SetActive(false);
        Restart.gameObject.SetActive(false);
        Menu.gameObject.SetActive(false);
        foreach (GameObject button in basesButtons)
        {
            button.SetActive(false);
        }

        if (PlayerManager.Instance != null)
        {
            foreach (GameObject button in PlayerManager.Instance.playerCardsButtons)
            {
                var navigation = button.GetComponent<Button>().navigation;
                navigation.mode = Navigation.Mode.Horizontal;
                button.GetComponent<Button>().navigation = navigation;
            }

            if (botonSeleccionadoAntesDePausa != null)
            {
                EventSystem.current.SetSelectedGameObject(botonSeleccionadoAntesDePausa);
                int idBoton = System.Array.IndexOf(PlayerManager.Instance.playerCardsButtons, botonSeleccionadoAntesDePausa);
                if (idBoton != -1 && PlayerManager.Instance.playerCards[idBoton] != null)
                {
                    PlayerManager.Instance.SeleccionarCarta(idBoton);
                }
            }
        }

        Continue.transform.localScale = Vector3.zero;
        Restart.transform.localScale = Vector3.zero;
        Menu.transform.localScale = Vector3.zero;
        animacionEnProgreso = false;

        Color color = iconoPausaSpriteRenderer.color;
        color.a = 1f;
        iconoPausaSpriteRenderer.color = color;
    }

    public void AbrirMenuPausaDesdeBoton()
    {
        if (!animacionEnProgreso && !isPaused)
        {
            PausarPartida();
        }
    }

    private IEnumerator MostrarBotonPausa()
    {
        yield return new WaitForSeconds(1.1f);
        
        float duracion = 0.2f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            progreso = Mathf.SmoothStep(0f, 1f, progreso);
            Pausa.transform.localScale = Vector3.Lerp(Vector3.zero, escalaOriginalPausa, progreso);
            yield return null;
        }

        Pausa.transform.localScale = escalaOriginalPausa;
    }
}