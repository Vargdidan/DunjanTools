extends Node2D

onready var length_label = get_node("Length")
onready var color = Color(0.2, 0.2, 1, 0.7)
onready var color_origin = Color(1, 0.2, 0.2, 0.7)
var start_pos = Vector2(0,0)
var start_pos_raw = Vector2(0,0)
var end_pos = Vector2(0,0)
var pos_is_set = false

func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	update()
	
	if (Input.is_action_pressed("ui_space") && !Input.is_action_pressed("ui_shift")):
		var mouse_position = get_global_mouse_position()
		var pos_x = stepify(mouse_position.x-32, 64)
		var pos_y = stepify(mouse_position.y-32, 64)
		end_pos = Vector2(pos_x/64, pos_y/64)
		if (!pos_is_set):
			start_pos = end_pos
			start_pos_raw = mouse_position
			pos_is_set = true
			global_position = mouse_position
		length_label.set_global_position(mouse_position)
		
	
	if (Input.is_action_just_released("ui_space")):
		pos_is_set = false
		length_label.set_visible(false)
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

func _draw():
	if (pos_is_set):
		length_label.set_visible(true)
		Input.set_mouse_mode(Input.MOUSE_MODE_HIDDEN)
		
		var center_start = to_local(Vector2(start_pos.x*64+32,start_pos.y*64+32))
		var center_end = to_local(Vector2(end_pos.x*64+32, end_pos.y*64+32))
		var center_length = get_distance(center_start, center_end)
		var length_ft = round(get_distance(start_pos, end_pos)*5)
		
		var intersect_start = to_local(start_pos_raw.snapped(Vector2(64,64)))
		var snapped_end_raw = to_local(get_global_mouse_position().snapped(Vector2(64,64)))
		var intersect_length = get_distance(intersect_start, snapped_end_raw)
		
		if (Input.is_action_pressed("circle") && Input.is_action_pressed("ui_control")):
			length_ft = round(center_length/64)*5
			draw_circle(center_start, center_length, color)
			draw_circle(center_start, 3, color_origin)
		elif (Input.is_action_pressed("circle")):
			length_ft = round(intersect_length/64)*5
			draw_circle(intersect_start, intersect_length, color)
			draw_circle(intersect_start, 3, color_origin)
		elif (Input.is_action_pressed("cone")):
			var cone_end_pos = to_local(get_global_mouse_position())
			var cone_width = intersect_start.distance_to(cone_end_pos)/2
			var cone_end_pos_1 = cone_end_pos+cone_end_pos.tangent().normalized()*cone_width
			var cone_end_pos_2 = cone_end_pos+cone_end_pos.tangent().normalized()*-cone_width
			length_ft = int((Vector2(intersect_start.x/64, intersect_start.y/64).distance_to(Vector2(cone_end_pos.x/64, cone_end_pos.y/64)))*5)
			draw_line(intersect_start, cone_end_pos, color, 3, true)
			draw_line(intersect_start, cone_end_pos_1, color, 3, true)
			draw_line(cone_end_pos_1, cone_end_pos_2, color, 3, true)
			draw_line(intersect_start, cone_end_pos_2, color, 3, true)
			draw_circle(intersect_start, 3, color_origin)
		elif (Input.is_action_pressed("box")):
			length_ft = round(intersect_length/64)*5
			draw_rect(Rect2(intersect_start, Vector2(intersect_length, intersect_length)),color, true, 3,true)
			draw_circle(intersect_start, 3, color_origin)
		elif (Input.is_action_pressed("line_aoe")):
			length_ft = round(intersect_length/64)*5
			draw_line(intersect_start, snapped_end_raw, color, 3, true)
			draw_circle(intersect_start, 3, color_origin)
		else:
			draw_circle(center_start, 3, color_origin)
			draw_line(center_start, center_end, color, 3, true)
		
		length_label.set_text(String(length_ft) + " ft")
		

func get_distance(unit_x, unit_y):
	var distance_x = abs(unit_x.x - unit_y.x)
	var distance_y = abs(unit_x.y - unit_y.y)
	var distance = int(max(distance_x, distance_y) + min(distance_x, distance_y)/2)
	return distance
