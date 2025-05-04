using UnityEngine;

/// Enum con los modos que tu juego reconoce.
/// Agrega más modos si los necesitas (p.ej. Tutorial, Survival…).
public enum GameMode
{
    Dragon,      // Escena normal contra el dragón
    FinalBoss    // Escena Boss Final – Esfinge
}

/// Singleton que persiste entre escenas y guarda la configuración
/// de la partida que se está por crear / jugar.
public class MatchSettings : MonoBehaviour
{
    public static MatchSettings Instance;

    [Header("Modo seleccionado para esta partida")]
    public GameMode mode = GameMode.Dragon;   // valor por defecto

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);    // persiste entre escenas
        }
        else
        {
            Destroy(gameObject);              // evita duplicados
        }
    }
}
