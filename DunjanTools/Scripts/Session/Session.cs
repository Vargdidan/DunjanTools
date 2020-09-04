using Godot;
using System;

public class Session : Node2D
{
    public PackedScene TokenScene { get; set; }
    public int TokenCounter { get; set; }
    public Node2D Tokens { get; set; }
    public Sprite Map { get; set; }
    public Particles2D Ping { get; set; }
    public Global Global { get; set; }
    public ClientVariables ClientVariables { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TokenScene = (PackedScene)GD.Load("res://Session/Token.tscn");
        TokenCounter = 0;
        Tokens = (Node2D)GetNode("Tokens");
        Map = (Sprite)GetNode("Map");
        Ping = (Particles2D)GetNode("Ping");
        Global = (Global)GetNode("/root/Global");
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        Rset(nameof(TokenCounter), MultiplayerAPI.RPCMode.Remotesync);

        // Connect signals
        //Global.Connect("ChangedMap", this, nameof(RecievedChangeMap));
        //GetTree().Connect("files_dropped", this, nameof(OnDropped));

        //Rpc(nameof(RequestAddPlayer), GetTree().GetNetworkUniqueId(), ClientVariables.Username);
    }

    public override void _Process(float delta)
    {
        //TODO
    }
}
