using Godot;
using System;

public class MapList : Button
{
    public ScrollContainer MapContainer {set; get;}
    public ClientVariables ClientVariables { set; get; }
    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        this.Disabled = !ClientVariables.NetworkOptions.DMRole;

        MapContainer = (ScrollContainer)GetParent().GetNode("MapContainer");
        MapContainer.Visible = false;
    }

    public void OnTokenButtonPressed() {
        MapContainer.Visible = !MapContainer.Visible;
    }
}