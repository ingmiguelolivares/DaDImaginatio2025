using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    public Dictionary<int, string> playerNames = new Dictionary<int, string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Verificar si el GameObject tiene un PhotonView, si no, mostrar un error
        if (GetComponent<PhotonView>() == null)
        {
            Debug.LogError("❌ No hay un PhotonView en NetworkManager. Asegúrate de agregarlo manualmente en Unity.");
        }
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("🔄 Conectando a Photon...");
            //PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Conectado a Photon Master Server.");
        //PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("✅ Conectado al lobby de Photon.");
        PhotonNetwork.JoinOrCreateRoom("SalaMultijugador", new RoomOptions { MaxPlayers = 3 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogError("❌ Error al unirse a la sala.");
            return;
        }

        Debug.Log($"✅ Jugador {PhotonNetwork.LocalPlayer.NickName} se unió a la sala.");

        // Verificar si este GameObject tiene un PhotonView antes de llamar a un RPC
        if (GetComponent<PhotonView>() == null)
        {
            Debug.LogError("❌ No se encontró un PhotonView en NetworkManager. Agrega uno en Unity.");
            return;
        }

        if (!playerNames.ContainsKey(PhotonNetwork.LocalPlayer.ActorNumber))
        {
            GetComponent<PhotonView>().RPC("SyncPlayerName", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.LocalPlayer.NickName);
        }
    }

    [PunRPC]
    void SyncPlayerName(int actorNumber, string nickname)
    {
        if (!playerNames.ContainsKey(actorNumber))
        {
            playerNames[actorNumber] = nickname;
        }
    }

    public string GetPlayerName(int actorNumber)
    {
        if (playerNames.ContainsKey(actorNumber))
        {
            return playerNames[actorNumber];
        }
        return "Jugador Desconocido";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"⚠ Desconectado de Photon: {cause}. Intentando reconectar...");
        PhotonNetwork.ConnectUsingSettings();
    }
}
