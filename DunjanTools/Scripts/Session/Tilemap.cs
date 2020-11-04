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
        if (ClientVariables.NetworkOptions.DMRole)
        {
            if (Input.IsActionPressed("ui_mouse_click") && (Input.IsActionPressed("ui_alt")))
            {
                DrawFog();
            }

            if (Input.IsActionJustPressed("ui_hide"))
            {
                ToggleFog();
            }
        }

        if (Input.IsActionJustPressed("ui_show_grid"))
        {
            ShowGrid = !ShowGrid;
            Update();
        }

        if (Input.IsActionPressed("ui_shift"))
        {
            Update();
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

    public void DrawFog()
    {
        int tile = 1;

        Vector2 target = GetGlobalMousePosition();
        target.x = target.x - CellSize.x/2;
        target.y = target.y - CellSize.y/2;
        target = target.Snapped(CellSize);

        if (Input.IsActionPressed("ui_shift") && Input.IsActionPressed("ui_alt"))
        {
            tile = -1;
        }

        SetCell((int)target.x/(int)CellSize.x, (int)target.y/(int)CellSize.y, tile);
    }

    public void ToggleFog()
    {
        if (ShowingMap)
        {
            int tilesX = ((int)(Map.GetRect().Size.x*Map.Scale.x) / (int)CellSize.x)+1;
            int tilesY = ((int)(Map.GetRect().Size.y*Map.Scale.y) / (int)CellSize.y)+1;

            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    SetCell(x, y, 1);
                }
            }
            ShowingMap = false;
        }
        else
        {
            int tilesX = (int)GetUsedRect().Size.x;
            int tilesY = (int)GetUsedRect().Size.y;

            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    SetCell(x, y, -1);
                }
            }
            ShowingMap = true;
        }
    }

    public void OnTimerTimeout()
    {
        if (ClientVariables.NetworkOptions.DMRole)
        {
            RsetUnreliable(nameof(UsedCells), GetUsedCells());
        }
        else
        {
            Clear();
            foreach (Vector2 usedCell in UsedCells)
            {
                SetCell((int)usedCell.x, (int)usedCell.y, 0);
            }
        }
    }
}
