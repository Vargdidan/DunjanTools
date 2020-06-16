extends Button


func _on_TokenButton_pressed():
	OS.shell_open(OS.get_executable_path().get_base_dir().plus_file("Tokens/"))
