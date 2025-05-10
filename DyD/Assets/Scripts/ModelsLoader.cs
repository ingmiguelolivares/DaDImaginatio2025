using UnityEngine;
using TMPro;  // Para usar TextMesh Pro

public class ModelsLoader : MonoBehaviour
{
    public static ModelsLoader Instance;  // Instancia estática para acceder desde otros scripts

    public GameObject[] easyModels;  // Modelos fáciles
    public GameObject[] mediumModels; // Modelos medios
    public GameObject[] hardModels;   // Modelos difíciles
    public GameObject capsule; // Referencia a la cápsula
    public TMP_Text feedbackText; // Texto para mostrar feedback

    private GameObject[] models; // Este array se ajustará dependiendo del evento

    void Awake()
    {
        // Asegurarse de que la instancia esté correctamente asignada
        if (Instance == null)
        {
            Instance = this;  // Asigna esta instancia a la propiedad estática
        }
        else
        {
            Destroy(gameObject);  // Si ya existe, destruye el objeto duplicado
        }
    }

    private void Start()
    {
        // Llamar a la función que asigna los modelos según el eventID
        AssignModelsBasedOnEvent();

        // Llamar a la función de spawneo de modelos
        LoadRandomModel();
    }

    // Función que asigna los modelos dependiendo del evento
    private void AssignModelsBasedOnEvent()
    {
        int eventID = PlayerPrefs.GetInt("EventID");  // Lee el eventID de PlayerPrefs

        if (eventID == 1)  // Calabozo fácil
        {
            models = easyModels;
            Debug.Log("Cargando modelos fáciles.");
        }
        else if (eventID == 2)  // Calabozo medio
        {
            models = mediumModels;
            Debug.Log("Cargando modelos medios.");
        }
        else if (eventID == 3)  // Calabozo difícil
        {
            models = hardModels;
            Debug.Log("Cargando modelos difíciles.");
        }
        else
        {
            models = easyModels; // Predeterminado: cargar modelos fáciles
            Debug.Log("Cargando modelos por defecto (fáciles).");
        }
    }

    // Función que carga un modelo aleatorio del array correspondiente
    public void LoadRandomModel()
    {
        feedbackText.text = "¡Botón presionado! Cargando modelo...";

        if (models.Length > 0)
        {
            foreach (Transform child in capsule.transform)
            {
                Destroy(child.gameObject);  // Eliminar modelos anteriores
            }

            int randomIndex = Random.Range(0, models.Length);
            GameObject model = Instantiate(models[randomIndex], capsule.transform.position, Quaternion.identity);

            feedbackText.text = "Modelo instanciado: " + model.name;

            MeshRenderer modelRenderer = model.GetComponent<MeshRenderer>();
            if (modelRenderer != null)
            {
                modelRenderer.enabled = true;
            }
            else
            {
                feedbackText.text = "¡Advertencia! El prefab no tiene un MeshRenderer.";
            }

            model.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            model.transform.SetParent(capsule.transform);
        }
        else
        {
            feedbackText.text = "No hay modelos para cargar.";  // Si no hay modelos en el array
        }
    }
}
