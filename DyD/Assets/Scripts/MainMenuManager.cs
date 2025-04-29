
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    public Button btnCrearCampaña;
    public Button btnUnirse;
    public InputField inputRoomID;
    public Text txtRoomCode;
    public Text txtEstado;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        btnCrearCampaña.onClick.AddListener(CrearCampaña);
        btnUnirse.onClick.AddListener(UnirseACampaña);
        txtRoomCode.gameObject.SetActive(false);
        txtEstado.text = "🔌 Conectando a Photon...";
    }

    public override void OnConnectedToMaster()
    {
        txtEstado.text = "✅ Conectado. Listo para crear o unirse.";
    }

    void CrearCampaña()
    {
        string roomID = Random.Range(10000, 99999).ToString();
        RoomOptions options = new RoomOptions { MaxPlayers = 3 };
        PhotonNetwork.CreateRoom(roomID, options);
        txtEstado.text = "⏳ Creando campaña...";
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

    public override void OnCreatedRoom()
    {
        txtRoomCode.text = $"📘 Código de campaña: {PhotonNetwork.CurrentRoom.Name}";
        txtRoomCode.gameObject.SetActive(true);
        txtEstado.text = "✅ Campaña creada. Esperando jugadores...";
    }

    public override void OnJoinedRoom()
    {
        txtEstado.text = $"✅ Unido a campaña: {PhotonNetwork.CurrentRoom.Name}";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 3)
        {
            PhotonNetwork.LoadLevel("Lobby");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        txtEstado.text = $"👤 Jugador {newPlayer.NickName} se unió ({PhotonNetwork.CurrentRoom.PlayerCount}/3)";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 3)
        {
            PhotonNetwork.LoadLevel("Lobby");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        txtEstado.text = $"❌ No se pudo unir: {message}";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        txtEstado.text = $"❌ Error al crear sala: {message}";
    }
}
