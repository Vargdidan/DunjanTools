extends Node2D

onready var length_label = get_node("Length")
onready var color = Color(0.2, 0.2, 1)
var start_pos = Vector2(0,0)
var end_pos = Vector2(0,0)
var pos_is_set = false

func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	update()
	
	if (Input.is_action_pressed("ui_space") && !Input.is_action_pressed("ui_shift")):
		var mouse_position = get_global_mouse_position()
		var pos_x = stepify(mouse_position.x-32, 64)
		var pos_y = stepify(mouse_position.y-32, 64)
		end_pos = Vector2(pos_x/64, pos_y/64)
		if (!pos_is_set):
			start_pos = end_pos
			pos_is_set = true
		global_position = Vector2(mouse_position.x, mouse_position.y + 15)
		length_label.set_visible(true)
		
		var length_ft = round(start_pos.distance_to(end_pos))*5
		
		length_label.set_text(String(length_ft) + " ft")
	
	if (Input.is_action_just_released("ui_space")):
		pos_is_set = false
		length_label.set_visible(false)

func _draw():
	if (pos_is_set):
		var local_start_pos = to_local(Vector2(start_pos.x*64+32,start_pos.y*64+32))
		var local_end_pos = to_local(Vector2(end_pos.x*64+32, end_pos.y*64+32))
		if (Input.is_action_pressed("circle")):
			draw_circle(local_start_pos, local_start_pos.distance_to(local_end_pos),color)
		else:
			draw_line(local_start_pos, local_end_pos, color, 3, true)
		
