extends TileMap

onready var tilemap_rect = self.get_used_rect()
onready var tilemap_cell_size = self.cell_size
onready var color = Color(0.5, 0.5, 0.5)
onready var show_grid = false
onready var map = get_parent().get_node("Map")
onready var my_timer = get_parent().get_node("Timer")
var changed_cells = []
var showing_map = true

func _ready():
	rset_config("changed_cells", MultiplayerAPI.RPC_MODE_REMOTESYNC)

func _process(_delta):
	update()
	
	if (get_tree().get_network_peer() != null):
		if ClientVariables.DMRole:
			move_mouse()
			show_hide_map()
	else:
		move_mouse()
		show_hide_map()
		
	
	if (Input.is_action_just_pressed("ui_show_grid")):
			show_grid = !show_grid
	

func _draw():
	if show_grid:
		for y in range(0, map.get_rect().size.y*map.scale.y/tilemap_cell_size.y):
			draw_line(Vector2(0, y * tilemap_cell_size.y), Vector2(map.get_rect().size.x*map.scale.x, y * tilemap_cell_size.y), color)
		for x in range(0, map.get_rect().size.x*map.scale.x/tilemap_cell_size.x):
			draw_line(Vector2(x * tilemap_cell_size.x, 0), Vector2(x * tilemap_cell_size.x, map.get_rect().size.y*map.scale.y), color)

func move_mouse():
	if (Input.is_action_pressed("ui_mouse_click") && Input.is_action_pressed("ui_alt")):
		var target_pos = get_global_mouse_position()
		target_pos.x = stepify(target_pos.x-tilemap_cell_size.x/2, tilemap_cell_size.x)
		target_pos.y = stepify(target_pos.y-tilemap_cell_size.y/2, tilemap_cell_size.y)
		self.set_cell(target_pos.x/64, target_pos.y/64, 1)
	
	if (Input.is_action_pressed("ui_mouse_click") && Input.is_action_pressed("ui_shift")):
		var target_pos = get_global_mouse_position()
		target_pos.x = stepify(target_pos.x-tilemap_cell_size.x/2, tilemap_cell_size.x)
		target_pos.y = stepify(target_pos.y-tilemap_cell_size.y/2, tilemap_cell_size.y)
		self.set_cell(target_pos.x/64, target_pos.y/64, -1)

func show_hide_map():
	if (showing_map && Input.is_action_just_pressed("ui_hide")):
		var tiles_x = int(map.get_rect().size.x*map.scale.x/tilemap_cell_size.x)+1
		var tiles_y = int(map.get_rect().size.y*map.scale.y/tilemap_cell_size.y)+1
		for x in tiles_x:
			for y in tiles_y:
				set_cell(x, y, 1)
		showing_map = false
	elif (!showing_map && Input.is_action_just_pressed("ui_hide")):
		var tiles_x = get_used_rect().size.x
		var tiles_y = get_used_rect().size.y
		for x in tiles_x:
			for y in tiles_y:
				set_cell(x, y, -1)
		showing_map = true
	

func _on_Timer_timeout():
	if ((get_tree().get_network_peer() != null) && ClientVariables.DMRole):
		rset_unreliable("changed_cells", get_used_cells())
	elif((get_tree().get_network_peer() != null) && !ClientVariables.DMRole):
		clear()
		for cell in changed_cells:
			set_cell(cell.x, cell.y, 0)
	
