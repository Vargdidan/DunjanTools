using Godot;
using System;

public class Token : Node2D
{
    private int tileSize = 64;
    private Color selectedColor = new Color(1, 0.2f, 0.2f, 0.2f);
    public String ImagePath { set; get; }
    public Label TokenName { set; get; }
    public Vector2 TargetPosition { set; get; }
    public Vector2 TargetScale { set; get; }
    public ClientVariables ClientVariables { set; get; }
    public Sprite TokenSprite { set; get; }

    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        ImagePath = "";
        TokenName = (Label)GetNode("TokenName");
        TokenSprite = (Sprite)GetNode("Sprite");
        TargetPosition = new Vector2();
        Vector2 sizeTo = new Vector2(tileSize, tileSize);
        TargetScale = sizeTo / TokenSprite.GetRect().Size;

        RsetConfig(nameof(TargetPosition), Godot.MultiplayerAPI.RPCMode.Remotesync);
        RsetConfig(nameof(TargetScale), Godot.MultiplayerAPI.RPCMode.Remotesync);
    }

    public void InitializeToken(String imagePath, Vector2 position, Vector2 scale)
    {
        TokenName.Text = this.Name;
        ImagePath = imagePath;
        Image image = new Image();
        Error result = image.Load(ClientVariables.TokenPath + imagePath);
        if (result == Error.Ok)
        {
            SetImageOnSprite(image);
        }
        else
        {
            //TODO: Notify user of missing image
            GD.Print("Missing token. No token in this path: " + imagePath);
            Error resultSetDefault = image.Load("res://icon.png");
            if (resultSetDefault == Error.Ok)
            {
                SetImageOnSprite(image);
            }
        }

        if (scale != null)
        {
            TargetScale = scale;
        }

        TargetPosition = position.Snapped(new Vector2(tileSize, tileSize));
        TokenSprite.GlobalPosition = TargetPosition;

        if (GetTree().NetworkPeer != null && !GetTree().IsNetworkServer())
        {
            RpcId(1, nameof(RequestPostionAndScale));
        }
    }

    public void SetImageOnSprite(Image image)
    {
        ImageTexture texture = new ImageTexture();
        texture.CreateFromImage(image, 0);
        TokenSprite.Texture = texture;
    }

    public override void _Process(float delta)
    {
        Update();

        CheckSelection();
        if (ClientVariables.SelectedToken == this)
        {
            Boolean dirty = false;
            if (Input.IsActionJustPressed("ui_mouse_click"))
            {
                if (!TokenSprite.GetRect().HasPoint(ToLocal(GetGlobalMousePosition())))
                {
                    ClientVariables.SelectedToken = null;
                    dirty = true;
                }
            }
            if (!dirty)
            {
                MoveWithMouse();
                MoveWithKeys();
                Resize();
            }
        }
        TokenSprite.GlobalPosition = Lerp(TokenSprite.GlobalPosition, TargetPosition, 0.2f);
        TokenSprite.Scale = Lerp(TokenSprite.Scale, TargetScale, 0.1f);
        TokenName.SetGlobalPosition(TokenSprite.GlobalPosition);
    }

    public override void _Draw()
    {
        if (ClientVariables.SelectedToken == this)
        {
            DrawRect(TokenSprite.GetRect(), selectedColor, true);
        }
    }

    public void CheckSelection()
    {
        if (TokenSprite.GetRect().HasPoint(ToLocal(GetGlobalMousePosition())))
        {
            TokenName.Visible = true;
            if (Input.IsActionJustReleased("ui_mouse_click"))
            {
                ClientVariables.SelectedToken = this;
            }
        }
        else
        {
            TokenName.Visible = false;
        }
    }

    public void MoveWithKeys()
    {
        if (Input.IsActionJustPressed("ui_move_left") ||
            Input.IsActionJustPressed("ui_move_right") ||
            Input.IsActionJustPressed("ui_move_up") ||
            Input.IsActionJustPressed("ui_move_down"))
        {
            int left = Convert.ToInt32(Input.IsActionJustPressed("ui_move_left"));
            int right = Convert.ToInt32(Input.IsActionJustPressed("ui_move_right"));
            int up = Convert.ToInt32(Input.IsActionJustPressed("ui_move_up"));
            int down = Convert.ToInt32(Input.IsActionJustPressed("ui_move_down"));

            Vector2 pos = TargetPosition;
            pos.x += (-left + right) * tileSize;
            pos.y += (-up + down) * tileSize;

            if (GetTree().NetworkPeer != null)
            {
                RpcId(1, nameof(RequestMovement), pos);
            }
            else
            {
                TargetPosition = pos;
            }
        }
    }

    public void MoveWithMouse()
    {
        if (Input.IsActionJustReleased("ui_mouse_click"))
        {
            Vector2 targetPos = GetGlobalMousePosition();
            Vector2 currentSize = TokenSprite.GetRect().Size * TargetScale;
            targetPos.x = targetPos.x - currentSize.x / 2;
            targetPos.y = targetPos.y - currentSize.y / 2;

            if (GetTree().NetworkPeer != null)
            {
                RpcId(1, nameof(RequestMovement), targetPos.Snapped(new Vector2(tileSize, tileSize)));
            }
            else
            {
                TargetPosition = targetPos.Snapped(new Vector2(tileSize, tileSize));
            }
        }
    }

    public void Resize()
    {
        if (Input.IsActionJustReleased("ui_scroll_up") && Input.IsActionPressed("ui_control"))
        {
            Vector2 currentSize = TokenSprite.GetRect().Size * TargetScale;
            Vector2 sizeTo = new Vector2(currentSize.x + tileSize, currentSize.y + tileSize);
            Vector2 scale = sizeTo / TokenSprite.GetRect().Size;

            if (GetTree().NetworkPeer != null)
            {
                RpcId(1, nameof(RequestScale), scale);
            }
            else
            {
                TargetScale = scale;
            }
        }

        if (Input.IsActionJustReleased("ui_scroll_down") && Input.IsActionPressed("ui_control"))
        {
            Vector2 currentSize = TokenSprite.GetRect().Size * TargetScale;
            Vector2 sizeTo = new Vector2(currentSize.x - tileSize, currentSize.y - tileSize);

            if (sizeTo.x > tileSize / 2)
            {
                Vector2 scale = sizeTo / TokenSprite.GetRect().Size;
                if (GetTree().NetworkPeer != null)
                {
                    RpcId(1, nameof(RequestScale), scale);
                }
                else
                {
                    TargetScale = scale;
                }
            }
        }
    }

    float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    Vector2 Lerp(Vector2 firstVector, Vector2 secondVector, float by)
    {
        float retX = Lerp(firstVector.x, secondVector.x, by);
        float retY = Lerp(firstVector.y, secondVector.y, by);
        return new Vector2(retX, retY);
    }

    [Remote]
    public void RequestMovement(Vector2 pos)
    {
        Rset(nameof(TargetPosition), pos);
    }

    [Remote]
    public void RequestScale(Vector2 scale)
    {
        Rset(nameof(TargetScale), scale);
    }

    [Remote]
    public void RequestPostionAndScale()
    {
        Rset(nameof(TargetPosition), TargetPosition);
        Rset(nameof(TargetScale), TargetScale);
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
}
