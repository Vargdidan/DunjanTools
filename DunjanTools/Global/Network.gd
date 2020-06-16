extends Node

const MAX_PLAYERS = 5

var battlemap_scene = preload("res://Session/Battlemap.tscn")

# Called when the node enters the scene tree for the first time.
func _ready():
	get_tree().connect("network_peer_connected", self, "_player_connected")
	get_tree().connect("network_peer_disconnected", self, "_player_disconnected")
	get_tree().connect("connected_to_server", self, "_connected_ok")
	get_tree().connect("connection_failed", self, "_connected_fail")
	get_tree().connect("server_disconnected", self, "_server_disconnected")
	

func _player_connected(id):
	var root = get_tree().get_root()
	var current_scene = root.get_child(root.get_child_count() - 1)
	
	for token in ClientVariables.inserted_tokens:
		var array = ClientVariables.inserted_tokens.get(token)
		var temp = current_scene.get_node("Tokens").get_node(token)
		ClientVariables.inserted_tokens.get(token)[1] = temp.get_node("Sprite").get_global_position()
	
	rpc_id(id, "register_player", ClientVariables.inserted_tokens, ClientVariables.current_map)

remote func register_player(tokens, map):
	var root = get_tree().get_root()
	var current_scene = root.get_child(root.get_child_count() - 1)
	
	current_scene.change_map(map)
	
	for token in tokens:
		var array = tokens.get(token)
		current_scene.create_token(array[0], array[1])
	

func _player_disconnected(id):
	pass # Erase player from info.

func _connected_ok():
	# Only called on clients, not server. Will go unused; not useful here.
	Global.goto_scene("res://Session/Battlemap.tscn")

func _server_disconnected():
	Global.goto_scene("res://GUI/MainMenu.tscn")
	pass # Server kicked us; show error and abort.

func _connected_fail():
	pass # Could not even connect to server; abort.

func _on_Host_pressed():
	var upnp = UPNP.new()
	upnp.discover(2000, 2, "InternetGatewayDevice")
	upnp.add_port_mapping(ClientVariables.port)
	var peer = NetworkedMultiplayerENet.new()
	var result = peer.create_server(ClientVariables.port, MAX_PLAYERS)
	if (result == OK):
		Global.goto_scene("res://Session/Battlemap.tscn")
		get_tree().network_peer = peer


func _on_Connect_pressed():
	var peer = NetworkedMultiplayerENet.new()
	var result = peer.create_client(ClientVariables.ip_address, ClientVariables.port)
	if (result == OK):
		get_tree().network_peer = peer
		
	

#func _notification(what):
	#if what == MainLoop.NOTIFICATION_WM_QUIT_REQUEST:
		#var upnp = UPNP.new()
		#upnp.delete_port_mapping(DEFAULT_PORT)
		#get_tree().quit() # default behavior
