using Godot;
using System;

public class ClientVariables : Node
{
    // Important folders
    public String TokenPath {get; set;}
    public String MapPath {get; set;}
    public String DataPath {get; set;}
    
    // Main menu variables (Should they be moved?)
    public Boolean UseUPnp {get; set;}
    public String IPAddress {get; set;}
    public Int64 Port {get; set;}
    public String Username {get; set;}

    public override void _Ready()
    {
        TokenPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Tokens/");
        MapPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Maps/");
        DataPath = OS.GetExecutablePath().GetBaseDir().PlusFile("Data/");
    }

    public void ResetVariables() 
    {
    }
}
