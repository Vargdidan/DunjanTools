using Godot;
using System;
using System.Collections.Generic;

public class Session : Node2D
{
    public PackedScene TokenScene { get; set; }
    public int TokenCounter { get; set; }
    public Node2D Tokens { get; set; }
    public Map Map { get; set; }
    public Particles2D Ping { get; set; }
    public Global Global { get; set; }
    public ClientVariables ClientVariables { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TokenScene = (PackedScene)GD.Load("res://Session/Token.tscn");
        TokenCounter = 0;
        Tokens = (Node2D)GetNode("Tokens");
        Map = (Map)GetNode("Map");
        Ping = (Particles2D)GetNode("Ping");
        Global = (Global)GetNode("/root/Global");
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        Rset(nameof(TokenCounter), MultiplayerAPI.RPCMode.Remotesync);

        // Connect signals
        Global.Connect("ChangedMap", this, nameof(RecievedChangeMap));
        GetTree().Connect("files_dropped", this, nameof(OnDropped));

        Rpc(nameof(AddPlayer), GetTree().GetNetworkUniqueId(), ClientVariables.Username);
    }

    public override void _Process(float delta)
    {
        //TODO
    }

    [RemoteSync]
    public void CreateToken()
    {
        //This one should be synced
        //However maybe we should use a request to createToken so that only the server has to count
        //and make unique names
    }

    public void CreateTokens(List<TokenReference> tokenReferences)
    {
        foreach (TokenReference tokenRef in tokenReferences)
        {
            Token token = (Token)TokenScene.Instance();
            token.Name = tokenRef.UniqueName;
            token.InitializeToken(tokenRef.ImageFile, new Vector2(), new Vector2());
            Tokens.AddChild(token);
            ClientVariables.InsertedTokens.Add(tokenRef);
        }
    }

    [RemoteSync]
    public void RemoveToken(String tokenName)
    {
        if (!tokenName.Empty())
        {
            Node token = Tokens.GetNode(tokenName);
            token.QueueFree();

            TokenReference? tokenReference = ClientVariables.FindTokenReferenceByName(tokenName);
            if (tokenReference.HasValue) {
                ClientVariables.InsertedTokens.Remove(tokenReference.Value);
            }
        }
    }

    public void RecievedChangeMap()
    {
        if (ClientVariables.DMRole)
        {
            //SaveSession()
            //Load session data from DM
            Rpc(nameof(ChangeMap), ClientVariables.SelectedMap); //Use session data?
        }
    }

    [RemoteSync]
    public void ChangeMap(String map)//TODO session data from dm
    {
        ClientVariables.ResetVariables();
        ClientVariables.SelectedMap = map;
        TokenCounter = 0;

        foreach (Node token in Tokens.GetChildren())
        {
            token.QueueFree();
        }

        //Set map based on session data
        Map.SetMap(map, new Vector2(1,1));

        //Populate tokens based on sessionData
    }

    public void OnDropped(String[] files, int droppedFrom)
    {
        //TODO
    }

    [RemoteSync]
    public void AddPlayer(int id, String name)
    {
        Label playerLabel = new Label();
        playerLabel.Name = id.ToString();
        playerLabel.Text = name;
        playerLabel.AddFontOverride("font", (Font)GD.Load("res://Assets/Fonts/Default.tres"));
        GetNode("UI/Players").AddChild(playerLabel);

        PlayerReference playerReference = new PlayerReference(id, name);
        ClientVariables.ConnectedPlayers.Add(playerReference);
    }
}
