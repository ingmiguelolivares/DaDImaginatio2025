using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TurnBasedGame : MonoBehaviourPunCallbacks
{
    public static TurnBasedGame Instance;

    public Text turnText;
    public Text consoleText;
    public TMP_Text questionLogText; // solo se asigna en FinalBoss

    public Button attackButton, defendButton;

    private List<Player> players = new List<Player>();
    private int currentTurnIndex = 0;

    private int enemyHealth = 200;
    private string currentEnemyType = "Terrestre";

    private Dictionary<int, int> playerHealths = new();
    private Dictionary<int, string> playerClasses = new();

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

        foreach (var p in players)
        {
            if (string.IsNullOrEmpty(p.NickName))
                p.NickName = $"Jugador_{p.ActorNumber}";

            AssignPlayerClass(p);
            playerHealths[p.ActorNumber] = GetPlayerHealth(playerClasses[p.ActorNumber]);

            LogMessage($"✅ {p.NickName} es un {playerClasses[p.ActorNumber]} con {playerHealths[p.ActorNumber]} HP.");
        }

        if (PhotonNetwork.IsMasterClient)
            ChooseRandomStartingPlayer();
    }

    void AssignPlayerClass(Player player)
    {
        int i = players.IndexOf(player);
        playerClasses[player.ActorNumber] = i switch
        {
            0 => "Guerrero",
            1 => "Arquero",
            2 => "Mago",
            _ => "Desconocido"
        };
    }

    int GetPlayerHealth(string playerClass) => playerClass switch
    {
        "Guerrero" => 250,
        "Arquero" => 200,
        "Mago" => 100,
        _ => 100
    };

    int GetPlayerDamage(string playerClass) => playerClass switch
    {
        "Guerrero" => 6,
        "Arquero" => 10,
        "Mago" => 15,
        _ => 5
    };

    void ChooseRandomStartingPlayer()
    {
        currentTurnIndex = Random.Range(0, players.Count);
        photonView.RPC("SyncTurn", RpcTarget.AllBuffered, currentTurnIndex);
    }

    [PunRPC]
    void SyncTurn(int newTurnIndex)
    {
        currentTurnIndex = newTurnIndex;
        StartTurn();
    }

    void StartTurn()
    {
        if (players.Count == 0) return;
        if (currentTurnIndex >= players.Count) currentTurnIndex = 0;

        var currentPlayer = players[currentTurnIndex];
        turnText.text = $"Turno de: {currentPlayer.NickName}";

        string msg = $"🌀 Turno de {currentPlayer.NickName}\nDragón: {enemyHealth} HP\n❤️ {GetPlayersHealth()}";
        LogMessage(msg);

        if (SceneManager.GetActiveScene().name == "FinalBoss" && questionLogText != null) {
            questionLogText.text = "Preparando las preguntas";
        }
         // Si estamos en FinalBoss, iniciar la fase de preguntas
        if (SceneManager.GetActiveScene().name == "FinalBoss")
        {
            var sphinx = FindObjectOfType<SphinxQuestionManager>();
            if (sphinx != null)
            {
                sphinx.StartQuestionPhase(currentPlayer.ActorNumber);
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró el SphinxQuestionManager en escena.");
            }
        }
   
    }

    public void ResolveAnswer(int actorNumber, bool success)
    {
        if (!success) AdvanceTurn();
    }

    void AdvanceTurn() => photonView.RPC("NextTurn", RpcTarget.AllBuffered);

    public void Attack() => PerformAction("Atacar");
    public void Defend() => PerformAction("Defender");

    void PerformAction(string action)
    {
        var currentPlayer = players[currentTurnIndex];

        if (playerHealths[currentPlayer.ActorNumber] <= 0)
        {
            LogMessage($"💀 {currentPlayer.NickName} está muerto.");
            AdvanceTurn();
            return;
        }

        if (PhotonNetwork.LocalPlayer != currentPlayer) return;

        if (action == "Atacar")
        {
            int dmg = GetPlayerDamage(playerClasses[currentPlayer.ActorNumber]);
            LogMessage($"⚔ {currentPlayer.NickName} ataca al dragón y causa {dmg} de daño.");
            photonView.RPC("ApplyDamageToEnemy", RpcTarget.AllBuffered, dmg);
        }
        else
        {
            LogMessage($"🛡 {currentPlayer.NickName} se defiende.");
            photonView.RPC("EnemyAttacksNextPlayer", RpcTarget.AllBuffered);
        }

        AdvanceTurn();
    }

    [PunRPC]
    void ApplyDamageToEnemy(int damage)
    {
        enemyHealth -= damage;
        if (enemyHealth < 0) enemyHealth = 0;

        LogMessage($"💥 El enemigo recibió {damage} de daño. HP restante: {enemyHealth}");

        if (enemyHealth <= 0)
        {
            LogMessage("🎉 ¡El dragón ha sido derrotado!");
            attackButton.interactable = false;
            defendButton.interactable = false;
        }
    }

    [PunRPC]
    void EnemyAttacksNextPlayer()
    {
        int next = (currentTurnIndex + 1) % players.Count;
        int actor = players[next].ActorNumber;

        if (!playerHealths.ContainsKey(actor) || playerHealths[actor] <= 0)
        {
            LogMessage($"⚠ {players[next].NickName} ya está muerto.");
            return;
        }

        playerHealths[actor] -= 30;
        if (playerHealths[actor] < 0) playerHealths[actor] = 0;

        LogMessage($"🐉 El dragón ataca a {players[next].NickName} y le causa 30 de daño.\n❤️ {GetPlayersHealth()}");

        if (playerHealths[actor] <= 0)
            LogMessage($"💀 {players[next].NickName} ha sido derrotado.");
    }

    [PunRPC]
    void NextTurn()
    {
        currentTurnIndex = (currentTurnIndex + 1) % players.Count;
        photonView.RPC("SyncTurn", RpcTarget.AllBuffered, currentTurnIndex);
    }

    string GetPlayersHealth()
    {
        string result = "";
        foreach (var p in players)
            result += $"{p.NickName}: {playerHealths[p.ActorNumber]} HP\n";
        return result;
    }

    void LogMessage(string msg)
    {
        Debug.Log(msg);
        if (consoleText != null) consoleText.text = msg;

        if (SceneManager.GetActiveScene().name == "FinalBoss" && questionLogText != null)
            questionLogText.text = msg;
    }
}
