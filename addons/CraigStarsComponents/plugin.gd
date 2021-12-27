tool
extends EditorPlugin

func _enter_tree():
	var texture = preload("res://addons/CraigStarsComponents/icon.svg")
	var button_texture = preload("res://addons/CraigStarsComponents/assets/icon_button.svg")
	add_custom_type("ProductionQueueItemsTable", "MarginContainer", preload("res://addons/CraigStarsComponents/src/ProductionQueueItemsTable.cs"), texture)
	add_custom_type("PlayersTable", "MarginContainer", preload("res://addons/CraigStarsComponents/src/PlayersTable.cs"), texture)
	add_custom_type("PlayerInfosTable", "MarginContainer", preload("res://addons/CraigStarsComponents/src/PlayerInfosTable.cs"), texture)
	add_custom_type("PublicGameInfosTable", "MarginContainer", preload("res://addons/CraigStarsComponents/src/PublicGameInfosTable.cs"), texture)
	add_custom_type("PlayerSavesTable", "MarginContainer", preload("res://addons/CraigStarsComponents/src/PlayerSavesTable.cs"), texture)
	add_custom_type("CSButton", "Button", preload("res://addons/CraigStarsComponents/src/CSButton.cs"), button_texture)
	
func _exit_tree():
	remove_custom_type("ProductionQueueItemsTable")
	remove_custom_type("PlayersTable")
	remove_custom_type("PlayerInfosTable")
	remove_custom_type("PublicGameInfosTable")
	remove_custom_type("PlayerSavesTable")
	remove_custom_type("CSButton")
	
