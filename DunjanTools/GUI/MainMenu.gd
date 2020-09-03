extends Node2D

# Called when the node enters the scene tree for the first time.
func _ready():
	$UI/OnlinePanel/Host.connect("pressed", Network, "OnHostPressed")
	$UI/OnlinePanel/Connect.connect("pressed", Network, "OnConnectPressed")
	$UI/OnlinePanel/UPNP.connect("toggled", Network, "OnUPNPToggled")
	$UI/OnlinePanel/DM.connect("toggled", Network, "OnDMToggled")
	
	ClientVariables.ResetVariables()
	
	if(ClientVariables.Username != "Incognito"):
		$UI/OnlinePanel/Username.set_text(ClientVariables.Username)
	if(ClientVariables.IPAddress != '127.0.0.1'):
		$UI/OnlinePanel/IP.set_text(ClientVariables.IPAddress)
	if(ClientVariables.Port != 31400):
		$UI/OnlinePanel/Port.set_text(String(ClientVariables.Port))
	$UI/OnlinePanel/DM.set_pressed(ClientVariables.DMRole)


func _on_Offline_pressed():
	Global.goto_scene("res://Session/Session.tscn")


func _on_IP_text_changed(text):
	ClientVariables.IPAddress = text


func _on_Port_text_changed(text):
	ClientVariables.Port = int(text)


func _on_Username_text_changed(text):
	ClientVariables.Username = text
