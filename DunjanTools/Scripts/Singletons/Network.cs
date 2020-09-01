using Godot;
using System;
using System.Collections.Generic;

public class Network : Node
{
    public ClientVariables ClientVariables {get; set;}
    public Global Global {get; set;}
    public Viewport Root {get; set;}
    
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

    public void _PlayerConnected(int id) {
        if (GetTree().NetworkPeer != null && GetTree().IsNetworkServer()) {
            RpcId(id, nameof(RegisterPlayer), ClientVariables.InsertedTokens, ClientVariables.SelectedMap, ClientVariables.ConnectedPlayers);
        }
    }

    [Remote]
    public void RegisterPlayer(List<TokenReference> tokens, String map, List<PlayerReference> connectedPlayers) {
        Node currentScene = Root.GetChild(Root.GetChildCount() - 1);
        if (!map.Empty()) {
            if (!ClientVariables.DMRole) {
                //currentScene.change_map(map);
            } else {
                ClientVariables.SelectedMap = map;
                Global.ChangeMap();
            }
        }

        if (!ClientVariables.DMRole) {
            foreach (TokenReference token in tokens) {
                //Pass tokenreference (rework battlemap script)
                //currentScene.CreateToken()
            }
        }

        foreach (PlayerReference player in connectedPlayers) {
            //Pass playerReference (rework battlemap script)
            //currentScene.AddPlayer()
        }
    }

    public void _PlayerDisconnected(int id) {
        Node currentScene = Root.GetChild(Root.GetChildCount() - 1);
        //current_scene.rpc("remove_player", id)
    }

    public void _ConnectedOk() {
        Global.GotoScene("res://Session/Battlemap.tscn");
    }

    public void _ConnectionFailed() {
        GetTree().NetworkPeer = null;
    }

    public void _ServerDisconnected() {
        if (ClientVariables.DMRole) {
            Node currentScene = Root.GetChild(Root.GetChildCount() - 1);
            //TODO: port battlemap
            //currentScene.save_battlemap();
        }
        Global.GotoScene("res://GUI/MainMenu.tscn");
        GetTree().NetworkPeer = null;
    }

    // Main menu network controls:
    public void _OnHostPressed() {
        ClientVariables.SaveMainMenu();
        if (ClientVariables.UseUPNP) {
            UPNP upnp = new UPNP();
            UPNP.UPNPResult resultV4 = (UPNP.UPNPResult)upnp.Discover(2000, 2, "InternetGatewayDevice");
            if (resultV4 == UPNP.UPNPResult.Success) {
                GD.Print("Will attempt to add port-forward with upnp ip v4");
                upnp.AddPortMapping(ClientVariables.Port);
            } else {
                upnp.DiscoverIpv6 = true;
                UPNP.UPNPResult resultV6 = (UPNP.UPNPResult)upnp.Discover(2000, 2, "InternetGatewayDevice");
                if (resultV6 == UPNP.UPNPResult.Success) {
                    GD.Print("Will attemt to add port-forward with upnp ip v6");
                    upnp.AddPortMapping(ClientVariables.Port);
                }
            }

            NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
            Error result = peer.CreateServer(ClientVariables.Port);
            if (result == Error.Ok) {
                Global.GotoScene("res://Session/Battlemap.tscn");
                GetTree().NetworkPeer = peer;
            }
        }

    }

    public void _OnConnectPressed() {
        ClientVariables.SaveMainMenu();
        NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
        Error result = peer.CreateClient(ClientVariables.IPAddress, ClientVariables.Port);
        if (result == Error.Ok) {
            GetTree().NetworkPeer = peer;
        }
    }

    public void _OnUPNPToggled(Boolean buttonToggled) {
        ClientVariables.UseUPNP = buttonToggled;
    }

    public void _OnDMToggled(Boolean buttonToggled) {
        ClientVariables.DMRole = buttonToggled;
    }

}
