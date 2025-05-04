using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// Administra la pantalla principal: crear sala, unirse y elegir modo de juego.
public class MainMenuManager : MonoBehaviourPunCallbacks
{
    /* ---------- Referencias UI ---------- */
    [Header("Botones")]
    public Button btnCrearCampaña;  // modo Dragon
    public Button btnBossFinal;     // modo FinalBoss
    public Button btnUnirse;        // unirse a sala existente

    [Header("Campos de texto")]
    public InputField inputRoomID;
    public Text txtRoomCode;
    public Text txtEstado;

    /* ---------- Inicialización ---------- */
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true; // ✅ importante para sincronizar escena Lobby y combate

        btnCrearCampaña.onClick.AddListener(OnPlayDragon);
        btnBossFinal   .onClick.AddListener(OnPlayBossFinal);
        btnUnirse      .onClick.AddListener(UnirseACampaña);

        txtRoomCode.gameObject.SetActive(false);
        txtEstado.text = "🔌 Conectando a Photon...";
    }

    public override void OnConnectedToMaster()
    {
        txtEstado.text = "✅ Conectado. Listo para crear o unirse.";
    }

    /* ---------- Métodos públicos (Inspector / OnClick) ---------- */
    public void OnPlayDragon()     => CrearCampaña(GameMode.Dragon);
    public void OnPlayBossFinal()  => CrearCampaña(GameMode.FinalBoss); // ✅ este ya guarda el modo FinalBoss

    /* ---------- Crear / Unirse ---------- */

    void CrearCampaña(GameMode mode)
    {
        // ✅ Guardamos el modo elegido para que el Lobby lo lea después
        MatchSettings.Instance.mode = mode;

        string roomID = Random.Range(10000, 99999).ToString();
        RoomOptions options = new RoomOptions { MaxPlayers = 3 };
        PhotonNetwork.CreateRoom(roomID, options);

        string modoTxt = mode == GameMode.FinalBoss ? "Boss Final" : "Dragón";
        txtEstado.text = $"⏳ Creando campaña ({modoTxt})...";
    }

    void UnirseACampaña()
    {
        string roomID = inputRoomID.text;
        if (string.IsNullOrEmpty(roomID))
        {
            txtEstado.text = "⚠ Ingresa un código válido.";
            return;
        }

        PhotonNetwork.JoinRoom(roomID);
        txtEstado.text = "⏳ Intentando unirse...";
    }

    /* ---------- Callbacks de Photon ---------- */

    public override void OnCreatedRoom()
    {
        txtRoomCode.text = $"📘 Código de campaña: {PhotonNetwork.CurrentRoom.Name}";
        txtRoomCode.gameObject.SetActive(true);
        txtEstado.text = "✅ Campaña creada. Esperando jugadores...";
    }

    public override void OnJoinedRoom()
    {
        txtEstado.text = $"✅ Unido a campaña: {PhotonNetwork.CurrentRoom.Name}";
        CheckStart();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        txtEstado.text =
            $"👤 {newPlayer.NickName} se unió ({PhotonNetwork.CurrentRoom.PlayerCount}/3)";
        CheckStart();
    }

    void CheckStart()
    {
        // ✅ Cuando hay 3 jugadores y soy MasterClient, cargo el Lobby
        if (PhotonNetwork.CurrentRoom.PlayerCount == 3 && PhotonNetwork.IsMasterClient)
        {
            txtEstado.text = "🚀 Cargando Lobby…";
            PhotonNetwork.LoadLevel("Lobby"); // Todos llegarán sincronizados gracias a AutomaticallySyncScene
        }
    }

    /* ---------- Manejo de errores ---------- */

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        txtEstado.text = $"❌ No se pudo unir: {message}";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        txtEstado.text = $"❌ Error al crear sala: {message}";
    }
}
