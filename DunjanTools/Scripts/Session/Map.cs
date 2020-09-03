using Godot;
using System;

public class Map : Sprite
{
    private int tileSize = 64;
    public String CurrentMap { set; get; }
    public ClientVariables ClientVariables { set; get; }
    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        tileSize = ClientVariables.TileSize;
        CurrentMap = "";
        RsetConfig(nameof(Scale), MultiplayerAPI.RPCMode.Remotesync);
    }

    public void SetMap(String mapPath)
    {
        
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
