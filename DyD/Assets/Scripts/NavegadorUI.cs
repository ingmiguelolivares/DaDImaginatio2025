using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegadorUI : MonoBehaviour
{
    public GameObject PantallaPaso3;
    public GameObject PantallaPaso4;

    public void IrAEscena(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    public void MostrarPaso4()
    {
        if (PantallaPaso3 != null) PantallaPaso3.SetActive(false);
        if (PantallaPaso4 != null) PantallaPaso4.SetActive(true);
    }
}
