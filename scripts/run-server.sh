#!/bin/zsh
DIR=${0:a}

/Applications/Godot_mono.app/Contents/MacOS/Godot  --no-window --verbose --path ${DIR}/../project.godot --start-server --game "A Multiplayer Barefoot Jaywalk"
