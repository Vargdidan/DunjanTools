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
        if (Input.IsActionJustPressed("delete"))
        {
            Rpc(nameof(RemoveToken), ClientVariables.SelectedToken.Name);
        }

        if (Input.IsActionJustPressed("ping"))
        {
            Rpc(nameof(PingMap), GetGlobalMousePosition());
        }

        //Rset on counter necessary?
    }

    [RemoteSync]
    public void CreateToken(String tokenName, String tokenFilePath, Vector2 position, Vector2 scale)
    {
        //This one should be synced
        //However maybe we should use a request to createToken so that only the server has to count
        //and make unique names
        Token token = (Token)TokenScene.Instance();
        token.Name = tokenName + "_" + TokenCounter;
        Tokens.AddChild(token);
        token.InitializeToken(tokenFilePath, position, scale);
        ClientVariables.InsertedTokens.Add(new TokenReference(TokenCounter, token.Name, tokenFilePath));
        TokenCounter += 1; //Should this be synced?
    }

    public void CreateTokens(List<TokenReference> tokenReferences)
    {
        foreach (TokenReference tokenRef in tokenReferences)
        {
            Token token = (Token)TokenScene.Instance();
            token.Name = tokenRef.UniqueName;
            Tokens.AddChild(token);
            token.InitializeToken(tokenRef.ImageFile, Vector2.Zero, Vector2.Zero);
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
        foreach (String absoluteFilePath in files)
        {
            int pos = absoluteFilePath.RFindN("Tokens\\");
            String relativePath = absoluteFilePath.Right(pos + 7);

            String fileName = relativePath.Split(".")[0];
            int index = fileName.RFindN("\\");
            if (index != -1) 
            {
                //Remove file extention from name
                fileName = fileName.Right(index+1);
            }
            Vector2 dropPosition = GetGlobalMousePosition();

            Rpc(nameof(CreateToken), fileName, relativePath, dropPosition, Vector2.Zero);
        }
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

    [RemoteSync]
    public void RemovePlayer(int id)
    {
        PlayerReference? playerReference = ClientVariables.FindPlayerReferenceById(id);
        if (playerReference.HasValue) {
            ClientVariables.ConnectedPlayers.Remove(playerReference.Value);
        }
        GetNode("UI/Players").GetNode(id.ToString()).QueueFree();
    }

    [RemoteSync]
    public void PingMap(Vector2 position)
    {
        Ping.Position = position;
        Ping.Emitting = true;
    }

    public void OnBackButtonPressed()
    {
        //Save battlemap
        NetworkedMultiplayerENet peer = (NetworkedMultiplayerENet)GetTree().NetworkPeer;
        peer.CloseConnection();
        GetTree().NetworkPeer = null;

        Global.GotoScene("res://GUI/MainMenu.tscn");
    }
}
