#!/bin/zsh
DIR=${0:a}

/Applications/Godot_mono.app/Contents/MacOS/Godot  --verbose --path ${DIR}/../project.godot --join-server localhost --player-name Bob $@
