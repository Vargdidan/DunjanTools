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

    public void SetMap(String mapPath, Vector2 scale)
    {
        CurrentMap = mapPath;
        Image mapImage = new Image();
        Error loaded = mapImage.Load(ClientVariables.MapFolder + CurrentMap);
        if (loaded == Error.Ok)
        {
            ImageTexture imageTexture = new ImageTexture();
            imageTexture.CreateFromImage(mapImage, 0);
            Texture = imageTexture;
        }

        //Set scale?
        Scale = scale;
    }

     public override void _Process(float delta)
     {
         if (ClientVariables.DMRole && Input.IsActionPressed("ui_shift"))
         {
             ResizeMap();
         }
     }

    public void ResizeMap()
    {
        Vector2 scaleTo = Scale;
        float scaleFactor = 0.1f;
        if (Input.IsActionPressed("ui_space"))
        {
            scaleFactor = 0.001f;
        }

        if (Input.IsActionJustReleased("ui_scroll_up"))
        {
            scaleTo.x += scaleFactor;
            scaleTo.y += scaleFactor;
        }
        else if (Input.IsActionJustReleased("ui_scroll_up"))
        {
            scaleTo.x -= scaleFactor;
            scaleTo.y -= scaleFactor;
        }

        if(!scaleTo.Equals(Scale)) {
            //Should this be a request to server?
            Rset(nameof(Scale), scaleTo);
        }
    }

    //Should this even be used? Or should one just get scale and use it in the save session?
    public Godot.Collections.Dictionary<string, object> SaveMapData()
    {
        // Is there a native way to store data I could use?
        return new Godot.Collections.Dictionary<string, object>() {
                {"map_scale_x", Scale.x},
                {"map_scale_y", Scale.y}
            };
    }
}
