using Godot;
using System;
using System.IO;
using System.IO.Compression;

public class Token : Node2D
{
    private int tileSize = 64;
    private Boolean isDragging = false;
    public Vector2 DragStartPosition { set; get; }
    public Vector2 DragEndPosition { set; get; }
    private Color dragColor = new Color(1f, 1f, 1f, 1f);
    public String ImagePath { set; get; }
    public Label TokenName { set; get; }
    public Node UI { set; get; }
    public Vector2 TargetPosition { set; get; }
    public Vector2 TargetScale { set; get; }
    public Vector2 TargetCollisionScale { set; get; }
    public ClientVariables ClientVariables { set; get; }
    public Sprite TokenSprite { set; get; }
    public PopupMenu PopupMenu { set; get; }
    public Area2D CollisionBox { set; get; }

    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        DragStartPosition = new Vector2();
        DragEndPosition = new Vector2();
        tileSize = ClientVariables.TileSize;
        ImagePath = "";
        TokenName = (Label)GetNode("UI/TokenName");
        PopupMenu = (PopupMenu)GetNode("UI/PopupMenu");
        CollisionBox = (Area2D)GetNode("Collision/Area2D");
        TokenSprite = (Sprite)GetNode("Sprite");
        TargetPosition = new Vector2();
        TargetScale = new Vector2(1,1);
        TargetCollisionScale = new Vector2();
        RsetConfig(nameof(TargetPosition), Godot.MultiplayerAPI.RPCMode.Remotesync);
        RsetConfig(nameof(TargetScale), Godot.MultiplayerAPI.RPCMode.Remotesync);
        RsetConfig(nameof(TargetCollisionScale), Godot.MultiplayerAPI.RPCMode.Remotesync);
    }

    public void InitializeToken(String tokenName, String imagePath, Vector2 position, Vector2 scale)
    {
        this.Name = tokenName;
        TokenName.Text = this.Name;
        ImagePath = imagePath;
        Image image = new Image();
        Error result = image.Load(ClientVariables.TokenFolder + imagePath);
        if (result == Error.Ok)
        {
            SetImageOnSprite(image);
        }
        else
        {
            TokenSprite.Texture = (Godot.Texture)ResourceLoader.Load("res://icon.png");
            int id = GetTree().NetworkPeer.GetUniqueId();
            Rpc(nameof(RequestImageData), id, this.Name, imagePath);
        }

        if (!scale.Equals(Vector2.Zero))
        {
            TargetScale = scale;
            
            //Update collisionBox
            float sizeTo = scale.x * TokenSprite.GetRect().Size.x;
            float scaleTo = sizeTo / tileSize;
            TargetCollisionScale = new Vector2(scaleTo, scaleTo);
        }
        else
        {
            Vector2 sizeTo = new Vector2(tileSize, tileSize);
            TargetScale = sizeTo / TokenSprite.GetRect().Size;
            float scaleTo = sizeTo.x / tileSize;
            TargetCollisionScale = new Vector2(scaleTo, scaleTo);
        }

        TargetPosition = position.Snapped(new Vector2(tileSize, tileSize));

        GlobalPosition = TargetPosition;
        Scale = TargetScale;
        CollisionBox.Scale = TargetCollisionScale;
/*
        if (!GetTree().IsNetworkServer())
        {
            RpcId(1, nameof(RequestPostionAndScale));
        }
*/
    }

    public void SetImageOnSprite(Image image)
    {
        ImageTexture texture = new ImageTexture();
        texture.CreateFromImage(image, 0);
        TokenSprite.Texture = texture;
    }

    public override void _Process(float delta)
    {
        CheckSelection();
        if (ClientVariables.SelectedTokens.Contains(this))
        {
            MoveWithMouse();
            MoveWithKeys();
            Resize();
            TokenName.Visible = true;
        }
        else
        {
            isDragging = false;
            TokenName.Visible = false;
        }
        
        GlobalPosition = MathUtil.Lerp(GlobalPosition, TargetPosition, 0.2f);
        TokenName.SetGlobalPosition(GlobalPosition);
        PopupMenu.SetGlobalPosition(GlobalPosition);
        CollisionBox.GlobalPosition = GlobalPosition;
        Scale = MathUtil.Lerp(Scale, TargetScale, 0.1f);
        CollisionBox.Scale = MathUtil.Lerp(CollisionBox.Scale, TargetCollisionScale, 0.1f);
    }

    public void CheckSelection()
    {
        if (!TokenSprite.GetRect().HasPoint(ToLocal(GetGlobalMousePosition())))
        {
            if (Input.IsActionJustReleased("ui_mouse_click") || Input.IsActionJustReleased("ui_right_click"))
            {
                PopupMenu.Visible = false;
            }
        }

        if (Input.IsActionJustReleased("ui_mouse_click") && Input.IsActionPressed("ui_shift"))
        {
            if (ClientVariables.SelectionBox.Intersects(new Rect2(GlobalPosition, TokenSprite.GetRect().Size*Scale)))
            {
                ClientVariables.SelectedTokens.Add(this);
            }
        }
    }

    public void MoveWithKeys()
    {
        if (Input.IsActionJustPressed("ui_move_left") || Input.IsActionJustPressed("ui_left") ||
            Input.IsActionJustPressed("ui_move_right") || Input.IsActionJustPressed("ui_right") ||
            Input.IsActionJustPressed("ui_move_up") || Input.IsActionJustPressed("ui_up") ||
            Input.IsActionJustPressed("ui_move_down") || Input.IsActionJustPressed("ui_down"))
        {
            int left = Convert.ToInt32(Input.IsActionJustPressed("ui_move_left")) + Convert.ToInt32(Input.IsActionJustPressed("ui_left"));
            int right = Convert.ToInt32(Input.IsActionJustPressed("ui_move_right")) + Convert.ToInt32(Input.IsActionJustPressed("ui_right"));
            int up = Convert.ToInt32(Input.IsActionJustPressed("ui_move_up")) + Convert.ToInt32(Input.IsActionJustPressed("ui_up"));
            int down = Convert.ToInt32(Input.IsActionJustPressed("ui_move_down")) + Convert.ToInt32(Input.IsActionJustPressed("ui_down"));

            Vector2 pos = TargetPosition;
            pos.x += (-left + right) * tileSize;
            pos.y += (-up + down) * tileSize;

            RpcId(1, nameof(RequestMovement), pos);
        }
    }

    public void MoveWithMouse()
    {
        if (Input.IsActionPressed("ui_mouse_click")  && !Input.IsActionPressed("ui_shift"))
        {
            if (!isDragging)
            {
                DragStartPosition = GetGlobalMousePosition();
                DragEndPosition = DragStartPosition;
                isDragging = true;
            }
            DragEndPosition = GetGlobalMousePosition();
        }

        if (Input.IsActionJustReleased("ui_mouse_click"))
        {
            isDragging = false;
            Vector2 distanceToMove = DragEndPosition - DragStartPosition;
            Vector2 finalPosition = TargetPosition + distanceToMove;

            RpcId(1, nameof(RequestMovement), finalPosition.Snapped(new Vector2(tileSize, tileSize)));
            DragStartPosition = TargetPosition;
            DragEndPosition = TargetPosition;
        }
    }

    public void Resize()
    {
        if (Input.IsActionJustReleased("ui_scroll_up") && Input.IsActionPressed("ui_control"))
        {
            Vector2 currentSize = TokenSprite.GetRect().Size * TargetScale;
            Vector2 sizeTo = new Vector2(currentSize.x + tileSize, currentSize.y + tileSize);
            Vector2 scale = sizeTo / TokenSprite.GetRect().Size;
            RpcId(1, nameof(RequestScale), scale);

            //Update collisionBox
            float scaleTo = sizeTo.x / tileSize;
            RpcId(1, nameof(RequestCollisionScale), new Vector2(scaleTo, scaleTo));
        }

        if (Input.IsActionJustReleased("ui_scroll_down") && Input.IsActionPressed("ui_control"))
        {
            Vector2 currentSize = TokenSprite.GetRect().Size * TargetScale;
            Vector2 sizeTo = new Vector2(currentSize.x - tileSize, currentSize.y - tileSize);

            if (sizeTo.x > tileSize / 2)
            {
                Vector2 scale = sizeTo / TokenSprite.GetRect().Size;
                RpcId(1, nameof(RequestScale), scale);

                //Update collisionBox
                float scaleTo = sizeTo.x / tileSize;
                RpcId(1, nameof(RequestCollisionScale), new Vector2(scaleTo, scaleTo));
            }
        }
    }

    [RemoteSync]
    public void RequestMovement(Vector2 pos)
    {
        Rset(nameof(TargetPosition), pos);
    }

    [RemoteSync]
    public void RequestScale(Vector2 scale)
    {
        Rset(nameof(TargetScale), scale);
    }

    [RemoteSync]
    public void RequestCollisionScale(Vector2 scale)
    {
        Rset(nameof(TargetCollisionScale), scale);
    }

    [RemoteSync]
    public void RequestPostionAndScale()
    {
        Rset(nameof(TargetPosition), TargetPosition);
        Rset(nameof(TargetScale), TargetScale);
        Rset(nameof(TargetCollisionScale), TargetCollisionScale);
    }

    [RemoteSync]
    public void RequestImageData(int id, String tokenName, String imagePath)
    {
        Image image = new Image();
        Error result = image.Load(ClientVariables.TokenFolder + imagePath);
        if (result == Error.Ok)
        {
            byte[] imageData = Compress(image.GetData());
            int height = image.GetHeight();
            int width = image.GetWidth();
            Image.Format format = image.GetFormat();
            int senderId = GetTree().NetworkPeer.GetUniqueId();
            RpcId(id, nameof(RecieveImageData), tokenName, senderId, height, width, format, imageData);
            
        }
    }

    [RemoteSync]
    public void RecieveImageData(String tokenName, int senderId, int height, int width, Image.Format format, byte[] imageData)
    {
        if (this.Name == tokenName)
        {
            Image image = new Image();
            image.CreateFromData(width, height, false, format, Decompress(imageData));
            SetImageOnSprite(image);
            RpcId(senderId, nameof(RequestPostionAndScale));
        }
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

    public Godot.Collections.Dictionary<string, object> SaveToken()
    {
        return new Godot.Collections.Dictionary<string, object>() {
                {"name", this.Name},
                {"image_path", ImagePath},
                {"pos_x", TargetPosition.x},
                {"pos_y", TargetPosition.y},
                {"scale_x", TargetScale.x},
                {"scale_y", TargetScale.y}
            };
    }

    public void OnMoveUpPressed()
    {
        Rpc(nameof(MoveToFront));
    }

    public void OnMoveBackPressed()
    {
        Rpc(nameof(MoveToBack));
    }

    [RemoteSync]
    public void MoveToFront()
    {
        GetParent().MoveChild(this, GetParent().GetChildCount()-1);
    }

    [RemoteSync]
    public void MoveToBack()
    {
        GetParent().MoveChild(this, 0);
    }
}
