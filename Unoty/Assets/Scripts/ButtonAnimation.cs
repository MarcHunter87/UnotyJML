using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour
{
    public Animator animator;
    private GameObject ultimoBotonSeleccionado;

    void Update()
    {
        GameObject botonSeleccionado = EventSystem.current.currentSelectedGameObject;

        if (botonSeleccionado != null && botonSeleccionado == gameObject)
        {
            if (ultimoBotonSeleccionado != botonSeleccionado && !MenuManager.esPrimerBoton)
            {
                AudioManager.Instance.PlaySound("button");
            }

            if (Input.GetButtonDown("Submit"))
            {
                AudioManager.Instance.PlaySound("buttonPress");
            }
            
            MenuManager.esPrimerBoton = false;
            ultimoBotonSeleccionado = botonSeleccionado;
            animator.SetBool("seleccionado", true);
        }
        else
        {
            animator.SetBool("seleccionado", false);
            animator.Play("Seleccionado", 0, 0f);
            ultimoBotonSeleccionado = null;
        }
    }
}