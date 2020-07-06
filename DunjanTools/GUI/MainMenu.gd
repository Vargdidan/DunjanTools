extends Node2D

# Called when the node enters the scene tree for the first time.
func _ready():
	$UI/OnlinePanel/Host.connect("pressed", Network, "_on_Host_pressed")
	$UI/OnlinePanel/Connect.connect("pressed", Network, "_on_Connect_pressed")
	$UI/OnlinePanel/UPNP.connect("toggled", Network, "_on_UPNP_toogled")


func _on_Offline_pressed():
	Global.goto_scene("res://Session/Battlemap.tscn")


func _on_IP_text_changed(text):
	ClientVariables.ip_address = text


func _on_Port_text_changed(text):
	ClientVariables.port = int(text)


func _on_Username_text_changed(text):
	ClientVariables.username = text
