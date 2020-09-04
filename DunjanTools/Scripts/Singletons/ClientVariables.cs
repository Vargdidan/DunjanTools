using Godot;
using System;
using System.Collections.Generic;

//Struct's
public struct PlayerReference
{
    public PlayerReference(Int64 id, String name)
    {
        Identity = id;
        Name = name;
    }

    public Int64 Identity { get; }
    public String Name { get; }

    public override string ToString() => $"({Identity}, {Name})";
}

public struct TokenReference
{
    public TokenReference(Int64 id, String name, String image)
    {
        Identity = id;
        UniqueName = name;
        ImageFile = image;
    }

    public Int64 Identity { get; }
    public String UniqueName { get; }
    public String ImageFile { get; }

    public override string ToString() => $"({Identity}, {UniqueName})";
}

public class ClientVariables : Node
{
    // Important folders
    public String TokenFolder { get; set; }
    public String MapFolder { get; set; }
    public String DataFolder { get; set; }

    // Network variables (Should they be moved?)
    public Boolean UseUPNP { get; set; }
    public String IPAddress { get; set; }
    public int Port { get; set; }
    public String Username { get; set; }
    public Boolean DMRole { get; set; }

    // Session variables
    public List<PlayerReference> ConnectedPlayers { get; set; }
    public List<TokenReference> InsertedTokens { get; set; }
    public Node SelectedToken { get; set; }
    public String SelectedMap { get; set; }
    public int TileSize { get; set; }

    public override void _Ready()
    {
        TokenFolder = OS.GetExecutablePath().GetBaseDir().PlusFile("Tokens/");
        MapFolder = OS.GetExecutablePath().GetBaseDir().PlusFile("Maps/");
        DataFolder = OS.GetExecutablePath().GetBaseDir().PlusFile("Data/");

        TileSize = 64;

        // Network
        UseUPNP = false;
        IPAddress = "127.0.0.1";
        Port = 31400;
        Username = "Incognito";
        DMRole = false;

        ConnectedPlayers = new List<PlayerReference>();
        InsertedTokens = new List<TokenReference>();
        SelectedToken = new Node();
        SelectedMap = "";
    }

    public void ResetVariables()
    {
        //Network
        UseUPNP = false;
        IPAddress = "127.0.0.1";
        Port = 31400;
        Username = "Incognito";
        DMRole = false;

        // Session
        ConnectedPlayers.Clear();
        InsertedTokens.Clear();
        SelectedToken = new Node();
        SelectedMap = "";

        // Restore latest settings
        LoadMainMenu();
    }

    public void SaveMainMenu()
    {
        // Is there a native way to store data I could use?
        Godot.Collections.Dictionary<string, object> mainMenuData =
            new Godot.Collections.Dictionary<string, object>() {
                {"ip", IPAddress},
                {"port", Port},
                {"username", Username},
                {"dm", DMRole}
            };

        var saveMainMenu = new File();
        Error result = saveMainMenu.Open(DataFolder + "main_menu.dat", File.ModeFlags.Write);
        if (result == Error.Ok)
        {
            saveMainMenu.StoreLine(JSON.Print(mainMenuData));
            GD.Print("Saved" + DataFolder + "main_menu.dat");
            saveMainMenu.Close();
        }
        else
        {
            GD.Print("Failed, error code: " + result.ToString());
        }
    }

    public void LoadMainMenu()
    {
        var loadMainMenu = new File();
        if (!loadMainMenu.FileExists(DataFolder + "main_menu.dat")) return;

        loadMainMenu.Open(DataFolder + "main_menu.dat", File.ModeFlags.Read);
        var mainMenuData =
            new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)JSON.Parse(loadMainMenu.GetLine()).Result);

        IPAddress = mainMenuData["ip"].ToString();
        Port = mainMenuData["port"].ToString().ToInt();
        Username = mainMenuData["username"].ToString();
        DMRole = (Boolean)mainMenuData["dm"];
        loadMainMenu.Close();
    }
}
