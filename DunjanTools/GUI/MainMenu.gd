extends Node2D

# Called when the node enters the scene tree for the first time.
func _ready():
	var dir = Directory.new()
	if dir.open(ClientVariables.token_path) != OK:
		dir.make_dir(ClientVariables.token_path)
	
	if dir.open(ClientVariables.map_path) != OK:
		dir.make_dir(ClientVariables.map_path)
	
	get_node("UI/Host").connect("pressed", Network, "_on_Host_pressed")
	get_node("UI/Connect").connect("pressed", Network, "_on_Connect_pressed")
	get_node("UI/UPNP").connect("toggled", Network, "_on_UPNP_toogled")


func _on_Offline_pressed():
	Global.goto_scene("res://Session/Battlemap.tscn")


func _on_IP_text_changed():
	ClientVariables.ip_address = get_node("UI/IP").get_text()


func _on_Port_text_changed():
	ClientVariables.port = int(get_node("UI/Port").get_text())


func _on_Username_text_changed():
	ClientVariables.username = get_node("UI/Username").get_text()
