using Mirror;
using System.Collections;
using System.Collections.Generic;
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

    [SyncVar(hook = nameof(OnColorChanged))]
    private Color playerColor = Color.white;

    [SerializeField]
    private Player clientPlayer;
    private Camera PlayerCamera {
        get
        {
            return clientPlayer.Camera ?? transform.GetChild(0).GetComponent<Camera>();
        }
    }

    void OnNameChanged(string _Old, string _New)
    {
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
    }

    [Command]
    public void CmdSetupPlayer(string _name, Color _col)
    {
        // player info sent to server, then server updates sync vars which handles it on all clients
        playerName = _name;
        playerColor = _col;
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

}
