using Godot;
using System;

public class MainMenu : Node2D
{
    public Panel OnlinePanel { set; get; }
    public ClientVariables ClientVariables { set; get; }
    public Global Global { set; get; }
    public Network Network { set; get; }
    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        Global = (Global)GetNode("/root/Global");
        Network = (Network)GetNode("/root/Network");
        OnlinePanel = (Panel)GetNode("UI/OnlinePanel");

        OnlinePanel.GetNode("Host").Connect("pressed", Network, "OnHostPressed");
        OnlinePanel.GetNode("Connect").Connect("pressed", Network, "OnConnectPressed");
        OnlinePanel.GetNode("UPNP").Connect("toggled", Network, "OnUPNPToggled");
        OnlinePanel.GetNode("DM").Connect("toggled", Network, "OnDMToggled");

        ClientVariables.ResetVariables();
        ClientVariables.ConnectedPlayers.Clear();

        if (ClientVariables.NetworkOptions.Username != "Incognito")
        {
            LineEdit username = (LineEdit)OnlinePanel.GetNode("Username");
            username.Text = ClientVariables.NetworkOptions.Username;
        }

        if (ClientVariables.NetworkOptions.IPAddress != "127.0.0.1")
        {
            LineEdit ip = (LineEdit)OnlinePanel.GetNode("IP");
            ip.Text = ClientVariables.NetworkOptions.IPAddress;
        }

        if (ClientVariables.NetworkOptions.Port != 31400)
        {
            LineEdit port = (LineEdit)OnlinePanel.GetNode("Port");
            port.Text = ClientVariables.NetworkOptions.Port.ToString();
        }

        
        CheckBox dm = (CheckBox)OnlinePanel.GetNode("DM");
        dm.Pressed = ClientVariables.NetworkOptions.DMRole;
    
        CheckBox upnp = (CheckBox)OnlinePanel.GetNode("UPNP");
        upnp.Pressed = ClientVariables.NetworkOptions.UseUPNP;
    }

    public void OnIpTextChanged(String text)
    {
        ClientVariables.NetworkOptions.IPAddress = text;
    }

    public void OnPortTextChanged(String text)
    {
        if (!"".Equals(text))
        {
            ClientVariables.NetworkOptions.Port = text.ToInt();
        }
        else 
        {
            ClientVariables.NetworkOptions.Port = 31400;
        }
    }

    public void OnUsernameTextChanged(String text)
    {
        ClientVariables.NetworkOptions.Username = text;
    }
}
