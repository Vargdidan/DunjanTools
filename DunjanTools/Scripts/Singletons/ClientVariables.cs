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
    public Int64 Port {get; set;}
    public String Username {get; set;}
    public Boolean DM {get; set;}

    // Session variables
    public PlayerReference[] ConnectedPlayers {get; set;}
    public Dictionary<Int64, TokenReference> InsertedTokens {get; set;}
    public String SelectedToken {get; set;}
    public String SelectedMap {get; set;} 

    public override void _Ready()
    {
        TokenPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Tokens/");
        MapPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Maps/");
        DataPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Data/");

        // Network
        UseUPNP = false;
        IPAddress = "127.0.0.1";
        Port = 31400;
        Username = "Incognito";
    }

    public void ResetVariables() 
    {
    }
}
