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
            //rpc("request_position_scale")
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
                //rpc("request_movement", pos)
            }
            else
            {
                TargetPosition = pos;
            }
        }
    }

    public void moveWithMouse()
    {
        if (Input.IsActionJustReleased("ui_mouse_click"))
        {
            
        }
    }
}
