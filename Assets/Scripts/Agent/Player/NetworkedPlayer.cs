using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NetworkedPlayer : NetworkedAgent
{

    [SerializeField]
    private TextMeshPro playerNameText;

    [SerializeField]
    private GameObject floatingInfo;

    private Material playerMaterialClone;

    [SyncVar(hook = nameof(OnNameChanged))]
    private string playerName;

    public string PlayerName { get { return playerNameText.text; } }

    [SyncVar(hook = nameof(OnColorChanged))]
    private Color playerColor = Color.white;

    [SerializeField]
    private Player clientPlayer;
    public Player ClientPlayer { get { return clientPlayer; } private set { clientPlayer = value; } }

    private Camera PlayerCamera {
        get
        {
            return ClientPlayer.Camera ?? transform.GetChild(0).GetComponent<Camera>();
        }
    }

    void OnNameChanged(string _Old, string _New)
    {
        playerName = _New;

        playerNameText.text = playerName;
    }

    void OnColorChanged(Color _Old, Color _New)
    {
        playerNameText.color = _New;
        playerMaterialClone = new Material(GetComponent<Renderer>().material);
        playerMaterialClone.color = _New;
        GetComponent<Renderer>().material = playerMaterialClone;
    }

    public override void OnStartLocalPlayer()
    {
        PlayerCamera.transform.position = transform.position;
        PlayerCamera.transform.localPosition = new Vector3(0f, 4.13f, -5.82f);

        floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
        floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        string name = "Player" + Random.Range(100, 999);
        Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        CmdSetupPlayer(name, color);
        
        OnNameChanged(playerNameText.text, name);
        OnColorChanged(playerNameText.color, color);

        this.clientPlayer = this.gameObject.GetComponent<Player>();

        if (clientPlayer.CurrentWorld is null && NetworkClient.isConnected)
        {
            LoadWorldFromServer();
        }
        else if (NetworkClient.isConnecting)
        {
            Debug.LogError("Connection suspended.");
        }
    }

    [Command]
    public void CmdSetupPlayer(string _name, Color _col)
    {
        // player info sent to server, then server updates sync vars which handles it on all clients
        playerName = _name;
        playerColor = _col;
    }

    [Command]
    public void CmdSendChatMessage(string _msg)
    {
        if (!isServer)
            return;

        this.RpcBroadcast(this.playerName, _msg);
    }

    [Command]
    public void CmdNick(string _nick)
    {
        if (!isServer) return;

        this.playerName = _nick;
    }

    [Command]
    public void CmdTeleport(string[] command)
    {
        if (string.IsNullOrEmpty(command[1]))
        {
            RpcReceive(this.connectionToClient, "Please enter a valid player name.");
            return;
        }

        string playerName = command[1];

        string result = $"Could not find the player: {playerName}";
        foreach (var connection in NetworkServer.connections.Values)
        {
            if (connection.identity.GetComponent<NetworkedPlayer>().PlayerName.Equals(playerName))
            {
                RpcTeleport(gameObject.GetComponent<NetworkedPlayer>().connectionToClient, connection.identity.GetComponent<Player>().transform.position);
                RpcReceive(this.connectionToClient, $"Teleporting to player: {playerName}...");
                
                return;
            }
        }

        RpcReceive(this.connectionToClient, result);
    }

    [Command]
    public void CmdList()
    {
        int playersConnected = NetworkServer.connections.Count;

        string result = $"There are {playersConnected} players online.\n";
        result += "Players:\n";

        foreach (var connection in NetworkServer.connections.Values)
        {
            result += "\t" + connection.identity.GetComponent<NetworkedPlayer>().PlayerName + "\n";
        }

        RpcReceive(this.connectionToClient, result);
    }

    [Command]
    public void CmdNick(string[] command)
    {
        if (string.IsNullOrEmpty(command[1]) || command[1].Length > 255)
        {
            RpcReceive(this.connectionToClient, "Please enter a valid name.");

            return;
        }

        if (NetworkServer.connections.Values
            .Select(connection => connection.identity.GetComponent<NetworkedPlayer>().PlayerName)
            .ToList()
            .Contains(command[1]))
        {

            RpcReceive(this.connectionToClient, "Another player already possesses that name.");
            
            return;
        }

        NetworkedPlayer player = gameObject.GetComponent<NetworkedPlayer>();
        player.playerName = command[1];

        RpcReceive(this.connectionToClient, $"Display name set to {command[1]}");
    }

    [ClientRpc]
    public void RpcBroadcast(string _name, string _msg)
    {
        NetworkedChatController.ChatController.Push($"{_name}: {_msg}");
    }

    [TargetRpc]
    public void RpcReceive(NetworkConnectionToClient target, string _msg)
    {
        NetworkedChatController.ChatController.Push($"{_msg}");
    }

    [TargetRpc]
    public void RpcTeleport(NetworkConnectionToClient target, Vector3 position)
    {
        this.gameObject.transform.position = position;
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            // make non-local players run this
            if (Camera.main != null)
            {
                floatingInfo.transform.LookAt(Camera.main.transform);
            }
        }
    }

    [Command]
    private void CmdRequestWorld()
    {
        World world = WorldAccessor.Identify(this.clientPlayer);

        if (world == null)
        {
            throw new KeyNotFoundException(world.parameters.Name);
        }

        // Send world information back to the client
        // Also we should add the player to the world here so it's there server-side
        // Fine for now though.
        RpcReceiveWorld(this.connectionToClient, world.parameters);
    }

    [TargetRpc]
    private void RpcReceiveWorld(NetworkConnectionToClient target, WorldParameters world)
    {
        clientPlayer.CurrentWorld = new WorldBuilder().SetParameters(world).Build();
    }

    public async void LoadWorldFromServer()
    {
        if (isServer)
        {
            Debug.LogError("RequestWorldFromServer should not be called on the server.");
            return;
        }

        CmdRequestWorld();

        while (clientPlayer.CurrentWorld == null)
        {
            await System.Threading.Tasks.Task.Delay(100);
        }

        Debug.Log($"World information received from the server. Seed: {clientPlayer.CurrentWorld.parameters.Seed}");
    }

    public HashSet<Vector3Int> chunksInProgress = new HashSet<Vector3Int>();

    [Command]
    public void CmdRequestChunk(Vector3Int chunkCoords, NetworkIdentity identity)
    {
        World world = NetworkClient.localPlayer.GetComponent<Player>().CurrentWorld;
        chunksInProgress.Add(chunkCoords);

        Chunk chunk = world.GetChunk(chunkCoords);

        if (chunk)
        {
            VoxelRun voxels = chunk.voxels;
            TargetReceiveChunk(identity.connectionToClient, new ChunkMessage
            {
                ChunkCoords = chunkCoords,
                Voxels = voxels,
                WorldName = chunk.world.parameters.Name
            });
        }
        else
        {
            // Handle the case where the chunk is not found on the server
            world.ForceRequestChunk(chunkCoords);
            // Will trigger async loading which then calls into a ClientRpc
            // that loads for all clients in range.
        }
    }

    [TargetRpc]
    public void TargetReceiveChunk(NetworkConnectionToClient target, ChunkMessage chunkMessage)
    {
        var currentWorld = clientPlayer.CurrentWorld;
        if (currentWorld == null || currentWorld.parameters.Name != chunkMessage.WorldName)
        {
            Debug.LogError("Received chunk for the wrong world or world not loaded.");
            return;
        }

        currentWorld.ChunkFinished(chunkMessage.ChunkCoords, chunkMessage.Voxels);
    }
}

