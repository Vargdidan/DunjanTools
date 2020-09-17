using Godot;
using System;
using System.Collections.Generic;

//Struct's
public struct PlayerReference
{
    public PlayerReference(int id, String name)
    {
        Identity = id;
        Name = name;
    }

    public int Identity { get; set; }
    public String Name { get; set; }
}

public struct TokenReference
{
    public TokenReference(Int64 id, String name, String image)
    {
        Identity = id;
        UniqueName = name;
        ImageFile = image;
    }

    public Int64 Identity { get; set; }
    public String UniqueName { get; set; }
    public String ImageFile { get; set; }
}

public class NetworkOptions
{
    public Boolean UseUPNP { get; set; }
    public String IPAddress { get; set; }
    public int Port { get; set; }
    public String Username { get; set; }
    public Boolean DMRole { get; set; }

    public NetworkOptions()
    {
        UseUPNP = false;
        IPAddress = "127.0.0.1";
        Port = 31400;
        Username = "Incognito";
        DMRole = false;
    }
}

public class ClientVariables : Node
{
    // Important folders
    public String TokenFolder { get; set; }
    public String MapFolder { get; set; }
    public String DataFolder { get; set; }

    public NetworkOptions NetworkOptions { get; set; } 

    // Session variables
    public List<PlayerReference> ConnectedPlayers { get; set; }
    public List<TokenReference> InsertedTokens { get; set; }
    public List<Node> SelectedTokens { get; set; }
    public String SelectedMap { get; set; }
    public int TileSize { get; set; }

    public override void _Ready()
    {
        TokenFolder = OS.GetExecutablePath().GetBaseDir().PlusFile("Tokens/");
        MapFolder = OS.GetExecutablePath().GetBaseDir().PlusFile("Maps/");
        DataFolder = OS.GetExecutablePath().GetBaseDir().PlusFile("Data/");

        TileSize = 64;

        NetworkOptions = new NetworkOptions();

        ConnectedPlayers = new List<PlayerReference>();
        InsertedTokens = new List<TokenReference>();
        SelectedTokens = new List<Node>();
        SelectedMap = "empty";
    }

    public void ResetVariables()
    {
        NetworkOptions = new NetworkOptions();

        // Session
        InsertedTokens.Clear();
        SelectedTokens.Clear();
        SelectedMap = "empty";

        // Restore latest settings
        LoadMainMenu();
    }

    public void SaveMainMenu()
    {
        // Is there a native way to store data I could use?
        Godot.Collections.Dictionary<string, object> mainMenuData =
            new Godot.Collections.Dictionary<string, object>() {
                {"ip", NetworkOptions.IPAddress},
                {"port", NetworkOptions.Port},
                {"username", NetworkOptions.Username},
                {"dm", NetworkOptions.DMRole}
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

        NetworkOptions.IPAddress = mainMenuData["ip"].ToString();
        NetworkOptions.Port = mainMenuData["port"].ToString().ToInt();
        NetworkOptions.Username = mainMenuData["username"].ToString();
        NetworkOptions.DMRole = (Boolean)mainMenuData["dm"];
        loadMainMenu.Close();
    }

    public Nullable<TokenReference> FindTokenReferenceByName(String tokenName)
    {
        foreach (TokenReference tokenRef in InsertedTokens)
        {
            if (tokenRef.UniqueName == tokenName)
            {
                return tokenRef;
            }
        }
        return null;
    }

    public Nullable<PlayerReference> FindPlayerReferenceById(int id)
    {
        foreach (PlayerReference playerRef in ConnectedPlayers)
        {
            if (playerRef.Identity == id)
            {
                return playerRef;
            }
        }
        return null;
    } 
}
