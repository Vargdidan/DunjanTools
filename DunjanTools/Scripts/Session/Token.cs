using Godot;
using System;

public class Token : Node2D
{
    private int tileSize = 64;
    private Boolean isDragging = false;
    private Vector2 dragStartPosition = new Vector2();
    private Vector2 dragEndPosition = new Vector2();
    private Color selectedColor = new Color(1, 0.2f, 0.2f, 0.2f);
    private Color dragColor = new Color(1f, 1f, 1f, 1f);
    public String ImagePath { set; get; }
    public Label TokenName { set; get; }
    public Node UI { set; get; }
    public Vector2 TargetPosition { set; get; }
    public Vector2 TargetScale { set; get; }
    public ClientVariables ClientVariables { set; get; }
    public Sprite TokenSprite { set; get; }

    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        tileSize = ClientVariables.TileSize;
        ImagePath = "";
        TokenName = (Label)GetNode("UI/TokenName");
        TokenSprite = (Sprite)GetNode("Sprite");
        TargetPosition = new Vector2();
        TargetScale = new Vector2(1,1);
        RsetConfig(nameof(TargetPosition), Godot.MultiplayerAPI.RPCMode.Remotesync);
        RsetConfig(nameof(TargetScale), Godot.MultiplayerAPI.RPCMode.Remotesync);
    }

    public void InitializeToken(String imagePath, Vector2 position, Vector2 scale)
    {
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
            //TODO: Notify user of missing image
            GD.Print("Missing token. No token in this path: " + imagePath);
            Error resultSetDefault = image.Load("res://icon.png");
            if (resultSetDefault == Error.Ok)
            {
                SetImageOnSprite(image);
            }
        }

        if (!scale.Equals(Vector2.Zero))
        {
            TargetScale = scale;
        }
        else
        {
            Vector2 sizeTo = new Vector2(tileSize, tileSize);
            TargetScale = sizeTo / TokenSprite.GetRect().Size;
        }

        TargetPosition = position.Snapped(new Vector2(tileSize, tileSize));

        GlobalPosition = TargetPosition;
        Scale = TargetScale;

        if (!GetTree().IsNetworkServer())
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
        if (ClientVariables.SelectedTokens.Contains(this))
        {
            MoveWithMouse();
            MoveWithKeys();
            Resize();
        }
        else
        {
            isDragging = false;
        }
        
        GlobalPosition = MathUtil.Lerp(GlobalPosition, TargetPosition, 0.2f);
        TokenName.SetGlobalPosition(GlobalPosition);
        Scale = MathUtil.Lerp(Scale, TargetScale, 0.1f);
    }

    public override void _Draw()
    {
        if (ClientVariables.SelectedTokens.Contains(this))
        {
            DrawRect(TokenSprite.GetRect(), selectedColor, true);

            if (Input.IsActionPressed("ui_mouse_click"))
            {
                Vector2 distanceToMove = dragEndPosition - dragStartPosition;
                Vector2 finalPosition = TargetPosition + distanceToMove;
                Vector2 start = ToLocal(new Vector2(TargetPosition.x + tileSize / 2, TargetPosition.y + tileSize / 2));
                finalPosition = ToLocal(new Vector2(finalPosition.x + tileSize / 2, finalPosition.y + tileSize / 2));
                DrawLine(start, finalPosition, dragColor, 10, true);
            }
        }
    }

    public void CheckSelection()
    {
        if (TokenSprite.GetRect().HasPoint(ToLocal(GetGlobalMousePosition())))
        {
            TokenName.Visible = true;
            if (Input.IsActionJustPressed("ui_mouse_click"))
            {
                if (Input.IsActionPressed("ui_control"))
                {
                    if (ClientVariables.SelectedTokens.Contains(this))
                    {
                        ClientVariables.SelectedTokens.Remove(this);
                    }
                    else
                    {
                        ClientVariables.SelectedTokens.Add(this);
                    }
                }
            }

            if (Input.IsActionJustReleased("ui_mouse_click"))
            {
                if (!Input.IsActionPressed("ui_control"))
                {
                    ClientVariables.SelectedTokens.Clear();
                    ClientVariables.SelectedTokens.Add(this);
                }
            }
        }
        else
        {
            TokenName.Visible = false;
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

            RpcId(1, nameof(RequestMovement), pos);
        }
    }

    public void MoveWithMouse()
    {
        if (Input.IsActionPressed("ui_mouse_click"))
        {
            if (!isDragging)
            {
                dragStartPosition = GetGlobalMousePosition();
                dragEndPosition = dragStartPosition;
                isDragging = true;
            }
            dragEndPosition = GetGlobalMousePosition();
        }

        if (Input.IsActionJustReleased("ui_mouse_click"))
        {
            isDragging = false;
            Vector2 distanceToMove = dragEndPosition - dragStartPosition;
            Vector2 finalPosition = TargetPosition + distanceToMove;

            RpcId(1, nameof(RequestMovement), finalPosition.Snapped(new Vector2(tileSize, tileSize)));
            dragStartPosition = TargetPosition;
            dragEndPosition = TargetPosition;
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
        }

        if (Input.IsActionJustReleased("ui_scroll_down") && Input.IsActionPressed("ui_control"))
        {
            Vector2 currentSize = TokenSprite.GetRect().Size * TargetScale;
            Vector2 sizeTo = new Vector2(currentSize.x - tileSize, currentSize.y - tileSize);

            if (sizeTo.x > tileSize / 2)
            {
                Vector2 scale = sizeTo / TokenSprite.GetRect().Size;
                RpcId(1, nameof(RequestScale), scale);
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
