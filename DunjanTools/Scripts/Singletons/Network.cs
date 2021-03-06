using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Network : Node
{
    public ClientVariables ClientVariables { get; set; }
    public Global Global { get; set; }
    public Viewport Root { get; set; }

    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        Global = (Global)GetNode("/root/Global");
        Root = GetTree().Root;

        GetTree().Connect("network_peer_connected", this, nameof(_PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(_PlayerDisconnected));
        GetTree().Connect("connected_to_server", this, nameof(_ConnectedOk));
        GetTree().Connect("connection_failed", this, nameof(_ConnectionFailed));
        GetTree().Connect("server_disconnected", this, nameof(_ServerDisconnected));
    }

    public void _PlayerConnected(int id)
    {
        if (GetTree().IsNetworkServer())
        {
            String tokens = JsonConvert.SerializeObject(ClientVariables.InsertedTokens);
            String players = JsonConvert.SerializeObject(ClientVariables.ConnectedPlayers);
            RpcId(id, nameof(RegisterPlayer), tokens, ClientVariables.SelectedMap, players);
        }
    }

    [Remote]
    public void RegisterPlayer(String jsonTokens, String map, String jsonPlayers)
    {
        Session sessionScene = (Session)Root.GetChild(Root.GetChildCount() - 1);
        List<TokenReference> tokens = JsonConvert.DeserializeObject<List<TokenReference>>(jsonTokens);
        List<PlayerReference> connectedPlayers = JsonConvert.DeserializeObject<List<PlayerReference>>(jsonPlayers);

        if (!"empty".Equals(map))
        {
            if (!ClientVariables.NetworkOptions.DMRole)
            {
                sessionScene.ChangeMap(map, new Vector2(1,1));
            }
            else
            {
                ClientVariables.SelectedMap = map;
                Global.ChangeMap();
            }
        }

        if (!ClientVariables.NetworkOptions.DMRole)
        {
            sessionScene.CreateTokens(tokens);
        }

        foreach (PlayerReference player in connectedPlayers)
        {
            sessionScene.AddPlayer(player.Identity, player.Name);
        }
    }

    public void _PlayerDisconnected(int id)
    {
        Session sessionScene = (Session)Root.GetChild(Root.GetChildCount() - 1);
        sessionScene.Rpc("RemovePlayer", id);
    }

    public void _ConnectedOk()
    {
        Global.GotoScene("res://Session/Session.tscn");
    }

    public void _ConnectionFailed()
    {
        NetworkedMultiplayerENet peer = (NetworkedMultiplayerENet)GetTree().NetworkPeer;
        peer.CloseConnection();
        GetTree().NetworkPeer = null;
    }

    public void _ServerDisconnected()
    {
        if (ClientVariables.NetworkOptions.DMRole)
        {
            Session sessionScene = (Session)Root.GetChild(Root.GetChildCount() - 1);
            sessionScene.SaveSession();
        }
        Global.GotoScene("res://GUI/MainMenu.tscn");
        NetworkedMultiplayerENet peer = (NetworkedMultiplayerENet)GetTree().NetworkPeer;
        peer.CloseConnection();
        GetTree().NetworkPeer = null;
    }

    // Main menu network controls:
    public void OnHostPressed()
    {
        ClientVariables.SaveMainMenu();
        if (ClientVariables.NetworkOptions.UseUPNP)
        {
            UPNP upnp = new UPNP();
            UPNP.UPNPResult resultV4 = (UPNP.UPNPResult)upnp.Discover(2000, 2, "InternetGatewayDevice");
            if (resultV4 == UPNP.UPNPResult.Success)
            {
                GD.Print("Will attempt to add port-forward with upnp ip v4");
                upnp.AddPortMapping(ClientVariables.NetworkOptions.Port);
            }
            else
            {
                upnp.DiscoverIpv6 = true;
                UPNP.UPNPResult resultV6 = (UPNP.UPNPResult)upnp.Discover(2000, 2, "InternetGatewayDevice");
                if (resultV6 == UPNP.UPNPResult.Success)
                {
                    GD.Print("Will attemt to add port-forward with upnp ip v6");
                    upnp.AddPortMapping(ClientVariables.NetworkOptions.Port);
                }
            }
        }
        var peer = new NetworkedMultiplayerENet();
        Error result = peer.CreateServer(ClientVariables.NetworkOptions.Port);
        if (result == Error.Ok)
        {
            GetTree().NetworkPeer = peer;
            Global.GotoScene("res://Session/Session.tscn");
        } else {
            GD.Print(result);
            GD.Print("On port:" + ClientVariables.NetworkOptions.Port);
        }
    }

    public void OnConnectPressed()
    {
        ClientVariables.SaveMainMenu();
        NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
        Error result = peer.CreateClient(ClientVariables.NetworkOptions.IPAddress, ClientVariables.NetworkOptions.Port);
        if (result == Error.Ok)
        {
            GetTree().NetworkPeer = peer;
        }
    }

    public void OnUPNPToggled(Boolean buttonToggled)
    {
        ClientVariables.NetworkOptions.UseUPNP = buttonToggled;
    }

    public void OnDMToggled(Boolean buttonToggled)
    {
        ClientVariables.NetworkOptions.DMRole = buttonToggled;
    }

}
