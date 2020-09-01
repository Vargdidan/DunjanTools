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
    public String ImageFile {get;}

    public override string ToString() => $"({Identity}, {UniqueName})";
}

public class ClientVariables : Node
{
    // Important folders
    public String TokenPath {get; set;}
    public String MapPath {get; set;}
    public String DataPath {get; set;}
    
    // Network variables (Should they be moved?)
    public Boolean UseUPNP {get; set;}
    public String IPAddress {get; set;}
    public int Port {get; set;}
    public String Username {get; set;}
    public Boolean DMRole {get; set;}

    // Session variables
    public List<PlayerReference> ConnectedPlayers {get; set;}
    public List<TokenReference> InsertedTokens {get; set;}
    public String SelectedToken {get; set;}
    public String SelectedMap {get; set;} 

    public override void _Ready() {
        TokenPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Tokens/");
        MapPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Maps/");
        DataPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Data/");

        // Network
        UseUPNP = false;
        IPAddress = "127.0.0.1";
        Port = 31400;
        Username = "Incognito";
        DMRole = false;

        ConnectedPlayers = new List<PlayerReference>();
        InsertedTokens = new List<TokenReference>();
        SelectedToken = "";
        SelectedMap = "";
    }

    public void ResetVariables() {
        //Network
        UseUPNP = false;
        IPAddress = "127.0.0.1";
        Port = 31400;
        Username = "Incognito";
        DMRole = false;

        // Session
        ConnectedPlayers.Clear();
        InsertedTokens.Clear();
        SelectedToken = "";
        SelectedMap = "";

        // Restore latest settings
        LoadMainMenu();
    }

    public void SaveMainMenu() {
        // Is there a native way to store data I could use?
        Godot.Collections.Dictionary<string, object> mainMenuData =
            new Godot.Collections.Dictionary<string, object>() {
                {"ip", IPAddress},
                {"port", Port},
                {"username", Username},
                {"dm", DMRole}
            };
        
        var saveMainMenu = new File();
        saveMainMenu.Open(DataPath + "main_menu.dat", File.ModeFlags.Write);
        saveMainMenu.StoreLine(JSON.Print(mainMenuData));
        saveMainMenu.Close();
    }

    public void LoadMainMenu() {
        var loadMainMenu = new File();
        if (!loadMainMenu.FileExists(DataPath + "main_menu.dat")) return;

        loadMainMenu.Open(DataPath + "main_menu.dat", File.ModeFlags.Read);
        var mainMenuData = 
            new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)JSON.Parse(loadMainMenu.GetLine()).Result);

        IPAddress = mainMenuData["ip"].ToString();
        Port = mainMenuData["port"].ToString().ToInt();
        Username = mainMenuData["username"].ToString();
        DMRole = (Boolean)mainMenuData["dm"];
    }
}
