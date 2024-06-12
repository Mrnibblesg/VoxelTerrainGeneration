using Mirror;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [SerializeField]
    NetworkedPlayer player;

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

    public void Pause(bool commandPause = false)
    {
        this.pauseMenu.SetActive(!pauseMenu.activeSelf);

        if (this.IsPaused())
        {
            input.Select();

            if (commandPause)
            {
                input.text = "/";
            }

            input.ActivateInputField();
            input.caretPosition = input.text.Length;

            this.player = this.player ?? NetworkClient.localPlayer.GetComponent<NetworkedPlayer>();
        }
        else
        {
            input.text = "";
        }
    }

    public bool IsPaused()
    {
        if (pauseMenu.IsUnityNull())
        {
            return false;
        }
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
    }

    public void Push(string message)
    {
        output.text += "\n" + message;

        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        chatScrollbar.value = 0;
        input.ActivateInputField();
    }

    private IEnumerator PushDisplayName()
    {
        yield return new WaitForSeconds(1f);
        
        if (player)
        {
            player = player ?? NetworkClient.localPlayer.gameObject.GetComponent<NetworkedPlayer>();

            Push($"Your username is: {player.PlayerName}! " +
                $"Welcome to the server :)");
        }
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
        player.CmdTeleport(command);

        return null;
    }

    private string List()
    {
        player.CmdList();

        return null;
    }

    private string Nick(string[] command)
    {
        player.CmdNick(command);

        return null;
    }

    private string Seed()
    {
        NetworkedPlayer player = NetworkClient.localPlayer.gameObject.GetComponent<NetworkedPlayer>();

        return $"This world's seed is: {player.ClientPlayer.CurrentWorld.parameters.Seed}";
    }

    public void MainMenu()
    {
        player.ClientPlayer.CurrentWorld.UnloadAll();

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

        SceneManager.LoadScene(0);
        SceneManager.sceneLoaded += returnedToMenu;
        
    }
    void returnedToMenu(Scene s, LoadSceneMode mode)
    {
        WorldBuilder.InitializeMenuWorld();
        SceneManager.sceneLoaded -= returnedToMenu;
    }
}
