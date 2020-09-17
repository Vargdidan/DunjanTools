using Godot;
using System;

public class Tilemap : TileMap
{
    public ClientVariables ClientVariables { set; get; }
    private Color gridColor = new Color(0.5f, 0.5f, 0.5f);
    public Boolean ShowGrid { set; get; }
    public Map Map { set; get; }
    public Timer RefreshTimer { set; get; }
    public Godot.Collections.Array UsedCells { set; get; }
    public Boolean ShowingMap { set; get; }

    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        ShowGrid = false;
        Map = (Map)GetParent().GetNode("Map");
        RefreshTimer = (Timer)GetParent().GetNode("Timer");
        UsedCells = new Godot.Collections.Array();
        ShowingMap = true;

        RsetConfig(nameof(UsedCells), MultiplayerAPI.RPCMode.Remotesync);
    }

    public override void _Process(float delta)
    {
        //Schedule redrawing for a set interval (optimizing GPU usage)
        Update();

        if (ClientVariables.NetworkOptions.DMRole)
        {

        }

        if (Input.IsActionJustPressed("ui_show_grid"))
        {
            ShowGrid = !ShowGrid;
        }
    }

    public override void _Draw()
    {
        if (ShowGrid)
        {
            float x = 0;
            float y = 0;
            float mapEndX = Map.GetRect().Size.x*Map.Scale.x;
            float mapEndY = Map.GetRect().Size.y*Map.Scale.y;

            while (x*CellSize.x <= mapEndX)
            {
                DrawLine(new Vector2(x*CellSize.x, 0), new Vector2(x*CellSize.x, mapEndY), gridColor);
                x++;
            }

            while (y*CellSize.y <= mapEndY)
            {
                DrawLine(new Vector2(0, y*CellSize.y), new Vector2(mapEndX, y*CellSize.y), gridColor);
                y++;
            }
        }
    }
}
