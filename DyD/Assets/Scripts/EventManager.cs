using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    public int MaxDistance = 35;

    public void Start()
    {
        // Puede tener lógica aquí si es necesario.
    }

    public void Update()
    {
        // Se puede usar para lógica de frame si es necesario.
    }

    public void ActivateEvent(int eventID)
    {
        // Guardar el ID del evento en PlayerPrefs
        PlayerPrefs.SetInt("EventID", eventID); // Guarda el ID para usarlo en el spawneo del modelo

        // Ahora cargamos la escena correspondiente
        SceneManager.LoadScene("MainMenu"); // Esto carga la escena MainMenu
    }
}
