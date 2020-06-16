extends Node

var selected_token = null
var selected_insert_token = null
var token_path = OS.get_executable_path().get_base_dir().plus_file("Tokens/")
var map_path = OS.get_executable_path().get_base_dir().plus_file("Maps/")

var inserted_tokens = {}
var current_map = null

var ip_address = '127.0.0.1'
var port = 31400
var username = "noname"

var connected_players = {}
