extends Node2D

# Called when the node enters the scene tree for the first time.
func _ready():
	$UI/OnlinePanel/Host.connect("pressed", Network, "_on_Host_pressed")
	$UI/OnlinePanel/Connect.connect("pressed", Network, "_on_Connect_pressed")
	$UI/OnlinePanel/UPNP.connect("toggled", Network, "_on_UPNP_toogled")
	
	ClientVariables.reset_variables()
	
	if(ClientVariables.username != "noname"):
		$UI/OnlinePanel/Username.set_text(ClientVariables.username)
	if(ClientVariables.ip_address != '127.0.0.1'):
		$UI/OnlinePanel/IP.set_text(ClientVariables.ip_address)
	if(ClientVariables.port != 31400):
		$UI/OnlinePanel/Port.set_text(String(ClientVariables.port))


func _on_Offline_pressed():
	Global.goto_scene("res://Session/Battlemap.tscn")


func _on_IP_text_changed(text):
	ClientVariables.ip_address = text


func _on_Port_text_changed(text):
	ClientVariables.port = int(text)


func _on_Username_text_changed(text):
	ClientVariables.username = text
