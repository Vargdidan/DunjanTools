using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class SessionData {

    public SessionData() {
        Tokens = new List<TokenReference>();
        MapScale = new Vector2(1,1);
    }

    public List<TokenReference> Tokens { get; set; }
    public Vector2 MapScale { get; set; }
}

public class Session : Node2D
{
    public PackedScene TokenScene { get; set; }
    public int TokenCounter { get; set; }
    public Node2D Tokens { get; set; }
    public Map Map { get; set; }
    public CPUParticles2D Ping { get; set; }
    public Global Global { get; set; }
    public ClientVariables ClientVariables { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TokenScene = (PackedScene)GD.Load("res://Session/Token.tscn");
        TokenCounter = 0;
        Tokens = (Node2D)GetNode("Tokens");
        Map = (Map)GetNode("Map");
        Ping = (CPUParticles2D)GetNode("Ping");
        Global = (Global)GetNode("/root/Global");
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");

        // Connect signals
        Global.Connect("ChangedMap", this, nameof(RecievedChangeMap));
        GetTree().Connect("files_dropped", this, nameof(OnDropped));

        if (ClientVariables.NetworkOptions.DMRole)
        {
            Control mapList = (Control)GetNode("UI/HBoxContainer/MapList");
            mapList.Visible = true;
        }

        RpcId(1, nameof(RequestAddMe), GetTree().GetNetworkUniqueId(), ClientVariables.NetworkOptions.Username);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustReleased("ui_esc"))
        {
            ClientVariables.SelectedTokens.Clear();
        }

        if (Input.IsActionJustPressed("delete"))
        {
            foreach (Node token in ClientVariables.SelectedTokens)
            {
                Rpc(nameof(RemoveToken), token.Name);
            }
        }

        if (Input.IsActionJustPressed("ping"))
        {
            Rpc(nameof(PingMap), GetGlobalMousePosition());
        }

        if (Input.IsActionJustPressed("ui_duplicate")) {
            foreach (Node token in ClientVariables.SelectedTokens)
            {
                Token tokenNode = (Token)token;
                TokenReference tokenReference = ClientVariables.FindTokenReferenceByName(token.Name);
                if (tokenReference != null)
                {
                    RpcId(1, nameof(RequestCreateToken), tokenReference.UniqueName.Split("_")[0], tokenReference.ImageFile, tokenNode.GlobalPosition, tokenNode.Scale);
                }
            }
        }
    }

    [RemoteSync]
    public void RequestCreateToken(String tokenName, String tokenFilePath, Vector2 position, Vector2 scale)
    {
        String name = tokenName + "_" + TokenCounter;
        TokenCounter += 1;
        Rpc(nameof(CreateToken), name, tokenFilePath, position, scale);
    }

    [RemoteSync]
    public void CreateToken(String tokenName, String tokenFilePath, Vector2 position, Vector2 scale)
    {
        Token token = (Token)TokenScene.Instance();
        Tokens.AddChild(token);
        token.InitializeToken(tokenName, tokenFilePath, position, scale);
        ClientVariables.InsertedTokens.Add(new TokenReference(tokenName, tokenFilePath));
    }

    public void CreateTokens(List<TokenReference> tokenReferences)
    {
        foreach (TokenReference tokenRef in tokenReferences)
        {
            Token token = (Token)TokenScene.Instance();
            Tokens.AddChild(token);
            token.InitializeToken(tokenRef.UniqueName, tokenRef.ImageFile, Vector2.Zero, Vector2.Zero);
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

            TokenReference tokenReference = ClientVariables.FindTokenReferenceByName(tokenName);
            if (tokenReference != null) {
                ClientVariables.InsertedTokens.Remove(tokenReference);
            }
        }
    }

    [RemoteSync]
    public void ClearTokensForNewMap()
    {
        String map = ClientVariables.SelectedMap;

        TokenCounter = 0;
        foreach (Node token in Tokens.GetChildren())
        {
            token.Free();
        }
        ClientVariables.ResetVariables();

        if (ClientVariables.NetworkOptions.DMRole)
        {
            ClientVariables.SelectedMap = map;
            LoadSession();
            Rpc(nameof(ChangeMap), ClientVariables.SelectedMap, Map.Scale);
        }
    }
    public void RecievedChangeMap()
    {
        if (ClientVariables.NetworkOptions.DMRole)
        {
            SaveSession();
            String map = ClientVariables.SelectedMap;
            Map.Scale = new Vector2(1,1);
            Rpc(nameof(ClearTokensForNewMap));
        }
    }

    [RemoteSync]
    public void ChangeMap(String map, Vector2 scale)//TODO session data from dm
    {
        ClientVariables.SelectedMap = map;
        Map.SetMap(map, scale);
    }

    public void OnDropped(String[] files, int droppedFrom)
    {
        foreach (String absoluteFilePath in files)
        {
            if (absoluteFilePath.Contains("Tokens"))
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

                RpcId(1, nameof(RequestCreateToken), fileName, relativePath, dropPosition, Vector2.Zero);
            }
        }
    }

    [RemoteSync]
    public void RequestAddMe(int id, string name)
    {
        Rpc(nameof(AddPlayer), id, name);
    }

    [RemoteSync]
    public void AddPlayer(int id, String name)
    {
        Label playerLabel = new Label();
        playerLabel.Name = id.ToString();
        playerLabel.Text = name;
        playerLabel.AddFontOverride("font", (Font)GD.Load("res://Assets/Fonts/Default.tres"));
        GetNode("UI/VBoxContainer/Players").AddChild(playerLabel);

        PlayerReference playerReference = new PlayerReference(id, name);
        ClientVariables.ConnectedPlayers.Add(playerReference);
    }

    [RemoteSync]
    public void RemovePlayer(int id)
    {
        PlayerReference? playerReference = ClientVariables.FindPlayerReferenceById(id);
        if (playerReference.HasValue) {
            ClientVariables.ConnectedPlayers.Remove(playerReference.Value);
            if (GetNode("UI/VBoxContainer/Players").GetNodeOrNull(id.ToString()) != null)
            {
                GetNode("UI/VBoxContainer/Players").GetNode(id.ToString()).QueueFree();
            }
        }
    }

    [RemoteSync]
    public void PingMap(Vector2 position)
    {
        Ping.Position = position;
        Ping.Emitting = true;
    }

    public void OnBackButtonPressed()
    {
        if (ClientVariables.NetworkOptions.DMRole)
        {
            SaveSession();
        }
        
        NetworkedMultiplayerENet peer = (NetworkedMultiplayerENet)GetTree().NetworkPeer;
        peer.CloseConnection();
        GetTree().NetworkPeer = null;

        Global.GotoScene("res://GUI/MainMenu.tscn");
    }

    public void SaveSession()
    {
        if (Map.CurrentMap.Equals("empty")) { return; }

        SessionData session = new SessionData();
        session.MapScale = Map.Scale;

        foreach (TokenReference token in ClientVariables.InsertedTokens)
        {
            Token tokenNode = (Token)Tokens.GetNode(token.UniqueName);
            TokenReference tokenRef = new TokenReference(token.UniqueName.Split("_")[0], token.ImageFile, tokenNode.GlobalPosition, tokenNode.Scale);
            session.Tokens.Add(tokenRef);
        }

        String sessionData = JsonConvert.SerializeObject(session, Formatting.Indented);
        System.IO.File.WriteAllText(ClientVariables.DataFolder + Map.CurrentMap.Split(".")[0] + ".json", sessionData);
    }

    public void LoadSession()
    {
        try
        {
            String sessionData = System.IO.File.ReadAllText(ClientVariables.DataFolder + ClientVariables.SelectedMap.Split(".")[0] + ".json");
            SessionData session = JsonConvert.DeserializeObject<SessionData>(sessionData);
            Map.Scale = session.MapScale;

            foreach (TokenReference tokenRef in session.Tokens)
            {
                RpcId(1, nameof(RequestCreateToken), tokenRef.UniqueName, tokenRef.ImageFile, tokenRef.Position, tokenRef.Scale);
            }
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            GD.Print("Could not find the directory to the session file.");
        }
        catch (System.IO.FileNotFoundException)
        {
            GD.Print("Could not find the session file.");
        }
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest)
        {
            NetworkedMultiplayerENet peer = (NetworkedMultiplayerENet)GetTree().NetworkPeer;
            peer.CloseConnection();
            GetTree().NetworkPeer = null;

            if (ClientVariables.NetworkOptions.DMRole)
            {
                SaveSession();
            }
        }
    }
}
