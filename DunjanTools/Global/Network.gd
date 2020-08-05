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
	if (get_tree().get_network_peer() != null && get_tree().is_network_server()):
		rpc_id(id, "register_player", ClientVariables.inserted_tokens, ClientVariables.current_map, ClientVariables.connected_players)
	

remote func register_player(tokens, map, players):
	print(players)
	var root = get_tree().get_root()
	var current_scene = root.get_child(root.get_child_count() - 1)
	current_scene.change_map(map)
	
	for token in tokens:
		var array = tokens.get(token)
		current_scene.create_token(array[0], array[1], Vector2(0,0))
	
	for player in players:
		var array = players.get(player)
		current_scene.add_player(array[0], array[1])

func _player_disconnected(id):
	var root = get_tree().get_root()
	var current_scene = root.get_child(root.get_child_count() - 1)
	current_scene.rpc("remove_player", id)

# Only called on clients, not server.
func _connected_ok():
	Global.goto_scene("res://Session/Battlemap.tscn")

# Server kicked us; show error and abort.
func _server_disconnected():
	Global.goto_scene("res://GUI/MainMenu.tscn")
	get_tree().network_peer = null

 # Could not even connect to server; abort.
func _connected_fail():
	get_tree().network_peer = null 

func _on_Host_pressed():
	ClientVariables.save_main_menu()
	if (ClientVariables.use_upnp):
		var upnp = UPNP.new()
		var result_upnp = upnp.discover(2000, 2, "InternetGatewayDevice")
		if result_upnp == 0:
			upnp.add_port_mapping(ClientVariables.port)
		else:
			upnp.set_discover_ipv6(true)
			result_upnp = upnp.discover(2000, 2, "InternetGatewayDevice")
			if result_upnp == 0:
				upnp.add_port_mapping(ClientVariables.port)
	
	var peer = NetworkedMultiplayerENet.new()
	var result = peer.create_server(ClientVariables.port, MAX_PLAYERS)
	if (result == OK):
		Global.goto_scene("res://Session/Battlemap.tscn")
		get_tree().network_peer = peer


func _on_Connect_pressed():
	ClientVariables.save_main_menu()
	var peer = NetworkedMultiplayerENet.new()
	var result = peer.create_client(ClientVariables.ip_address, ClientVariables.port)
	if (result == OK):
		get_tree().network_peer = peer
		
	

func _on_UPNP_toogled(button_pressed):
	print(button_pressed)
	ClientVariables.use_upnp = button_pressed

#func _notification(what):
	#if what == MainLoop.NOTIFICATION_WM_QUIT_REQUEST:
		#var upnp = UPNP.new()
		#upnp.delete_port_mapping(DEFAULT_PORT)
		#get_tree().quit() # default behavior
