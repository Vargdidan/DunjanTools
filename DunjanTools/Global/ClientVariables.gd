extends Node

# Important paths
var token_path = OS.get_executable_path().get_base_dir().plus_file("Tokens/")
var map_path = OS.get_executable_path().get_base_dir().plus_file("Maps/")
var data_path = OS.get_executable_path().get_base_dir().plus_file("Data/")

# Main menu variables
var use_upnp = false
var ip_address = '127.0.0.1'
var port = 31400
var username = "noname"

# Session variables
var connected_players = {}
var selected_token = null
var inserted_tokens = {}
var selected_map = null

func reset_variables():
	use_upnp = false
	ip_address = '127.0.0.1'
	port = 31400
	username = "noname"
	
	selected_token = null
	inserted_tokens = {}
	selected_map = null
	connected_players = {}
	
	load_main_menu()

func reset_tokens():
	selected_token = null
	inserted_tokens = {}

func save_main_menu():
	var main_menu_dict = {
		"ip" : ip_address,
		"port" : port,
		"username" : username
	}
	
	var main_menu_file = File.new()
	main_menu_file.open(data_path + "main_menu.dat", File.WRITE)
	main_menu_file.store_line(to_json(main_menu_dict))
	main_menu_file.close()

func load_main_menu():
	var main_menu_file = File.new()
	if not main_menu_file.file_exists(data_path + "main_menu.dat"):
		return
	
	main_menu_file.open(data_path + "main_menu.dat", File.READ)
	var data = parse_json(main_menu_file.get_line())
	ip_address = data["ip"]
	port = data["port"]
	username = data["username"]
	main_menu_file.close()
