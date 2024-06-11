using Mirror;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedChatController : MonoBehaviour
{
    public static NetworkedChatController ChatController { get; private set; }

    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private TextMeshProUGUI output;
    
    [SerializeField]
    private TMP_InputField input;

    [SerializeField]
    private Scrollbar chatScrollbar;

    private static string HELP_MSG = @"
To display this menu, use /help
To view the world seed, use /seed
To list online players, use /list
To change your display name, use /nick [name]
To teleport to another player, use /teleport [playerName]";

    private void Awake()
    {
        ChatController = GetComponent<NetworkedChatController>();

        Pause();

        StartCoroutine(PushDisplayName());
    }

    private void OnEnable()
    {
        input.onSubmit.AddListener(Send);
    }

    private void OnDisable()
    {
        input.onSubmit.RemoveListener(Send);
    }

    public void Pause()
    {
        this.pauseMenu.SetActive(!pauseMenu.activeSelf);

        if (this.IsPaused())
        {
            input.Select();
        }
    }

    public bool IsPaused()
    {
        return pauseMenu.activeSelf;
    }

    private void Send(string newText)
    {
        input.text = string.Empty;

        if (output != null && newText != null && newText.Length > 0)
        {
            if (newText.StartsWith("/"))
            {
                string[] command = newText.Substring(1).Split(' ');
                command[0] = command[0].ToLower();

                Execute(command);
            }

            else
            {
                // From TMP example
                var timeNow = System.DateTime.Now;
                string formattedInput = "[<#FFFF80>" + timeNow.Hour.ToString("d2") + ":" + timeNow.Minute.ToString("d2") + ":" + timeNow.Second.ToString("d2") + "</color>] " + newText;
                NetworkClient.localPlayer.GetComponent<NetworkedPlayer>().CmdSendChatMessage(formattedInput);
            }
        }

        // Keep Chat input field active
        input.ActivateInputField();
        input.Select();
    }

    public void Push(string message)
    {
        output.text += "\n" + message;

        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();

        chatScrollbar.value = 0;
    }

    private IEnumerator PushDisplayName()
    {
        yield return new WaitForSeconds(1);

        Push($"Your username is: {NetworkClient.localPlayer.gameObject.GetComponent<NetworkedPlayer>().PlayerName}! " +
            $"Welcome to the server :)");
    }

    private void Execute(string[] command)
    {
        string result;

        switch(command[0])
        {
            case "help":
                result = HELP_MSG;
                break;

            case "seed":
                result = Seed();
                break;

            case "nick":
                result = Nick(command);
                break;

            case "list":
                result = List();
                break;

            case "teleport":
                result = Teleport(command);
                break;

            default:
                result = "Error: Command not found.";
                break;
        }

        if (result is not null && result != string.Empty)
            Push(result);
    }

    private string Teleport(string[] command)
    {
        if (string.IsNullOrEmpty(command[1]))
        {
            return "Please enter a valid player name.";
        }
        
        string playerName = command[1];

        string result = $"Could not find the player: {playerName}";
        foreach (var connection in NetworkServer.connections.Values)
        {
            if (connection.identity.GetComponent<NetworkedPlayer>().PlayerName.Equals(playerName))
            {
                NetworkClient.localPlayer.GetComponent<Player>().transform.position = 
                    connection.identity.GetComponent<Player>().transform.position;

                return $"Teleporting to player: {playerName}...";
            }
        }

        return result;
    }

    private string List()
    {
        int playersConnected = NetworkServer.connections.Count;

        string result = $"There are {playersConnected} players online.\n";
        result += "Players:\n";

        foreach (var connection in NetworkServer.connections.Values)
        {
            result += "\t" + connection.identity.GetComponent<NetworkedPlayer>().PlayerName + "\n";
        }

        return result;
    }

    private string Nick(string[] command)
    {
        if (string.IsNullOrEmpty(command[1]) || command[1].Length > 255)
        {
            return "Please enter a valid name.";
        }
        
        if (NetworkServer.connections.Values
            .Select(connection => connection.identity.GetComponent<NetworkedPlayer>().PlayerName)
            .ToList()
            .Contains(command[1])) {

            return "Another player already possesses that name.";
        }

        NetworkedPlayer player = NetworkClient.localPlayer.gameObject.GetComponent<NetworkedPlayer>();
        player.CmdNick(command[1]);

        return $"Display name set to {command[1]}";
    }

    private string Seed()
    {
        NetworkedPlayer player = NetworkClient.localPlayer.gameObject.GetComponent<NetworkedPlayer>();

        return $"This world's seed is: {player.ClientPlayer.CurrentWorld.parameters.Seed}";
    }
}
