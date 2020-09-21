using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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

public class TokenReference
{
    public TokenReference(String name, String image)
    {
        UniqueName = name;
        ImageFile = image;
        Position = new Vector2(0,0);
        Scale = new Vector2(0,0);
    }

    [JsonConstructor]
    public TokenReference(String name, String image, Vector2 position, Vector2 scale)
    {
        UniqueName = name;
        ImageFile = image;
        Position = position;
        Scale = scale;
    }

    public String UniqueName { get; set; }
    public String ImageFile { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Scale { get; set; }
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
    public Rect2 SelectionBox { get; set; }

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

        SelectionBox = new Rect2();
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
        String networkOptions = JsonConvert.SerializeObject(NetworkOptions, Formatting.Indented);
        System.IO.File.WriteAllText(DataFolder + "main_menu.json", networkOptions);
    }

    public void LoadMainMenu()
    {
        try
        {
            String networkOptions = System.IO.File.ReadAllText(DataFolder + "main_menu.json");
            NetworkOptions = JsonConvert.DeserializeObject<NetworkOptions>(networkOptions);
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            GD.Print("Could not find the directory to the main_menu file.");
        }
        catch (System.IO.FileNotFoundException)
        {
            GD.Print("Could not find the main_menu file.");
        }
    }

    public TokenReference FindTokenReferenceByName(String tokenName)
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
