using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class TurnBasedGame : MonoBehaviourPunCallbacks
{
    public static TurnBasedGame Instance;

    public Text turnText, consoleText;
    public Button attackButton, defendButton;

    private List<Player> players = new List<Player>();
    private int currentTurnIndex = 0;

    private int enemyHealth = 200;
    private string currentEnemyType = "Terrestre";

    private Dictionary<int, int> playerHealths = new Dictionary<int, int>();
    private Dictionary<int, string> playerClasses = new Dictionary<int, string>();

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
        }
    }

    void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            LogMessage("❌ No estás en una sala.");
            return;
        }

        players.Clear();
        StartCoroutine(WaitForPlayersToSync());

        attackButton.onClick.AddListener(Attack);
        defendButton.onClick.AddListener(Defend);
    }

    IEnumerator WaitForPlayersToSync()
    {
        yield return new WaitForSeconds(2.0f);
        players.AddRange(PhotonNetwork.PlayerList);

        if (players.Count == 0)
        {
            LogMessage("⚠ No se encontraron jugadores en la sala.");
            yield break;
        }

        LogMessage($"🔹 Se detectaron {players.Count} jugadores.");

        foreach (Player player in players)
        {
            if (string.IsNullOrEmpty(player.NickName))
            {
                player.NickName = $"Jugador_{player.ActorNumber}";
            }

            AssignPlayerClass(player);
            playerHealths[player.ActorNumber] = GetPlayerHealth(playerClasses[player.ActorNumber]);
            LogMessage($"✅ {player.NickName} es un {playerClasses[player.ActorNumber]} con {playerHealths[player.ActorNumber]} HP.");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            ChooseRandomStartingPlayer();
        }
    }

    void ChooseRandomStartingPlayer()
    {
        if (players.Count == 0)
        {
            LogMessage("⚠ No hay jugadores para iniciar el turno.");
            return;
        }

        currentTurnIndex = Random.Range(0, players.Count);
        photonView.RPC("SyncTurn", RpcTarget.AllBuffered, currentTurnIndex);
    }

    void AssignPlayerClass(Player player)
    {
        int index = players.IndexOf(player);
        switch (index)
        {
            case 0: playerClasses[player.ActorNumber] = "Guerrero"; break;
            case 1: playerClasses[player.ActorNumber] = "Arquero"; break;
            case 2: playerClasses[player.ActorNumber] = "Mago"; break;
            default: playerClasses[player.ActorNumber] = "Desconocido"; break;
        }
    }

    int GetPlayerHealth(string playerClass)
    {
        switch (playerClass)
        {
            case "Guerrero": return 250;
            case "Arquero": return 200;
            case "Mago": return 100;
            default: return 100;
        }
    }

    int GetPlayerDamage(string playerClass)
    {
        switch (playerClass)
        {
            case "Guerrero": return 6;
            case "Arquero": return 10;
            case "Mago": return 15;
            default: return 5;
        }
    }

    [PunRPC]
    void SyncTurn(int newTurnIndex)
    {
        currentTurnIndex = newTurnIndex;
        StartTurn();
    }

    void StartTurn()
    {
        if (players.Count == 0)
        {
            LogMessage("⚠ No hay jugadores en la lista.");
            return;
        }

        if (currentTurnIndex >= players.Count)
        {
            LogMessage("⚠ Índice de turno fuera de rango, reiniciando.");
            currentTurnIndex = 0;
        }

        Player currentTurnPlayer = players[currentTurnIndex];

        if (currentTurnPlayer == null)
        {
            LogMessage("⚠ Error: Jugador actual es `null`, intentando recuperar...");
            ChooseRandomStartingPlayer();
            return;
        }

        turnText.text = $"Turno de: {currentTurnPlayer.NickName}";
        LogMessage($"🌀 Turno asignado a: {currentTurnPlayer.NickName}\n💀 Vida del Dragón: {enemyHealth} HP\n❤️ Vida de los jugadores:\n{GetPlayersHealth()}");
    }

    public void Attack()
    {
        PerformAction("Atacar");
    }

    public void Defend()
    {
        PerformAction("Defender");
    }

    void PerformAction(string action)
    {
        if (players.Count == 0) return;

        Player currentTurnPlayer = players[currentTurnIndex];

        if (currentTurnPlayer == null || !playerHealths.ContainsKey(currentTurnPlayer.ActorNumber)) return;

        // ❌ Jugadores muertos no actúan
        if (playerHealths[currentTurnPlayer.ActorNumber] <= 0)
        {
            LogMessage($"💀 {currentTurnPlayer.NickName} está muerto y no puede actuar.");
            photonView.RPC("NextTurn", RpcTarget.AllBuffered);
            return;
        }

        if (PhotonNetwork.LocalPlayer == currentTurnPlayer)
        {
            if (action == "Atacar")
            {
                int damage = GetPlayerDamage(playerClasses[currentTurnPlayer.ActorNumber]);
                LogMessage($"⚔ {currentTurnPlayer.NickName} ataca al dragón y causa {damage} de daño.");
                photonView.RPC("ApplyDamageToEnemy", RpcTarget.AllBuffered, damage);
                photonView.RPC("NextTurn", RpcTarget.AllBuffered);
            }
            else if (action == "Defender")
            {
                LogMessage($"🛡 {currentTurnPlayer.NickName} se defiende.");
                photonView.RPC("EnemyAttacksNextPlayer", RpcTarget.AllBuffered);
                photonView.RPC("NextTurn", RpcTarget.AllBuffered);
            }
        }
        else
        {
            LogMessage($"⚠ No es el turno de {PhotonNetwork.LocalPlayer.NickName}.");
        }
    }


    [PunRPC]
    void ApplyDamageToEnemy(int damage)
    {
        enemyHealth -= damage;
        if (enemyHealth < 0) enemyHealth = 0;

        LogMessage($"💥 El enemigo ({currentEnemyType}) recibió {damage} de daño. HP restante: {enemyHealth}");

        if (enemyHealth <= 0)
        {
            LogMessage("🎉 ¡El dragón ha sido derrotado! 🎉 Fin del juego.");
            attackButton.interactable = false;
            defendButton.interactable = false;
        }
    }

    [PunRPC]
    void UpdateDragonHP(int updatedHP)
    {
        enemyHealth = updatedHP;
        LogMessage($"💥 El enemigo ({currentEnemyType}) ahora tiene {enemyHealth} HP restante.");
    }

    [PunRPC]
    void EnemyAttacksNextPlayer()
    {
        if (players.Count == 0) return;

        int nextPlayerIndex = (currentTurnIndex + 1) % players.Count;
        int nextActorNumber = players[nextPlayerIndex].ActorNumber;

        if (!playerHealths.ContainsKey(nextActorNumber)) return;

        if (playerHealths[nextActorNumber] <= 0)
        {
            LogMessage($"⚠ {players[nextPlayerIndex].NickName} ya está muerto. El dragón no ataca.");
            return;
        }

        int damage = 30;
        playerHealths[nextActorNumber] -= damage;
        if (playerHealths[nextActorNumber] < 0) playerHealths[nextActorNumber] = 0;

        LogMessage($"🐉 El dragón ataca a {players[nextPlayerIndex].NickName} y le causa {damage} de daño. \n❤️ {GetPlayersHealth()}");

        if (playerHealths[nextActorNumber] <= 0)
        {
            LogMessage($"💀 {players[nextPlayerIndex].NickName} ha sido derrotado.");
        }
    }


    [PunRPC]
    void NextTurn()
    {
        if (players.Count == 0) return;

        currentTurnIndex = (currentTurnIndex + 1) % players.Count;
        photonView.RPC("SyncTurn", RpcTarget.AllBuffered, currentTurnIndex);
    }

    string GetPlayersHealth()
    {
        string healthInfo = "";
        foreach (var player in players)
        {
            if (playerHealths.ContainsKey(player.ActorNumber))
            {
                healthInfo += $"{player.NickName}: {playerHealths[player.ActorNumber]} HP\n";
            }
        }
        return healthInfo;
    }

    void LogMessage(string message)
    {
        Debug.Log(message);
        if (consoleText != null)
        {
            consoleText.text = message;
        }
    }
}
