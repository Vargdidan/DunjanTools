using Godot;
using System;
using System.IO;
using System.IO.Compression;

public class Map : Sprite
{
    private int tileSize = 64;
    public String CurrentMap { set; get; }
    public Vector2 TargetScale { set; get; }
    public ClientVariables ClientVariables { set; get; }
    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        tileSize = ClientVariables.TileSize;
        CurrentMap = "empty";
        TargetScale = new Vector2(1,1);
        RsetConfig(nameof(TargetScale), MultiplayerAPI.RPCMode.Remotesync);
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
        else
        {
            int id = GetTree().NetworkPeer.GetUniqueId();
            Rpc(nameof(RequestMapImageData), id, mapPath);
        }

        TargetScale = scale;
        RpcId(1, nameof(GetMapScale));
    }

     public override void _Process(float delta)
     {
         if (ClientVariables.NetworkOptions.DMRole && Input.IsActionPressed("ui_shift"))
         {
             ResizeMap();
         }

         Scale = MathUtil.Lerp(Scale, TargetScale, 0.2f);
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
        else if (Input.IsActionJustReleased("ui_scroll_down"))
        {
            scaleTo.x -= scaleFactor;
            scaleTo.y -= scaleFactor;
        }

        if(!scaleTo.Equals(Scale)) {
            RpcId(1, nameof(RequestScale), scaleTo);
        }
    }

    [RemoteSync]
    public void RequestMapImageData(int id, String imagePath)
    {
        Image image = new Image();
        Error result = image.Load(ClientVariables.MapFolder + imagePath);
        if (result == Error.Ok)
        {
            byte[] imageData = Compress(image.GetData());
            int height = image.GetHeight();
            int width = image.GetWidth();
            Image.Format format = image.GetFormat();
            RpcId(id, nameof(RecieveMapImageData), height, width, format, imageData);
            
        }
    }

    [RemoteSync]
    public void RecieveMapImageData(int height, int width, Image.Format format, byte[] imageData)
    {
        Image image = new Image();
        image.CreateFromData(width, height, false, format, Decompress(imageData));
        ImageTexture imageTexture = new ImageTexture();
        imageTexture.CreateFromImage(image, 0);
        Texture = imageTexture;
    }

    public static byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        MemoryStream input = new MemoryStream(data);
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
        {
            dstream.CopyTo(output);
        }
        return output.ToArray();
    }

    [RemoteSync]
    public void RequestScale(Vector2 scale)
    {
        Rset(nameof(TargetScale), scale);
    }

    [RemoteSync]
    public void GetMapScale()
    {
        Rset(nameof(TargetScale), TargetScale);
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
