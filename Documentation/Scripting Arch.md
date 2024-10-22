# Scripting Arch

Isometric grid
2d grid, interpreted as a isometric grid.
Array of grid objects -> not monobehaviours.
	Easy to serialize/work with non monobehaviours.

Seperated Data and Rendering, 
	How to achive abstraction with seperate data and rendering models. 
	I mean the only thing the monobehaviour does is has it's own update&start. The tile does not need these.
	So not really seperated Data and Rendering. The rendering functions will just return a array of frames that it should animate or special effects metadata that a UTR(Universal Tile Render) can interpret, UTR also has a building renderer ig.
		UTR takes care of tile loading and unloading. No need for chunks, just do it by distance. 
	I guess we do the same thing for Entities. ETR(entity)? UTR(ui)?
TBD - DataObject -> a parent class of all data holding objects.
URs (Universal renders), basically all you need to do is give it the correct seralizable DataObject and it put itself in the correct place and render correctly. It will also have a unload function ig? And ig it could have a forced state update. DataObjects have a reference to send a forceupdate (System.NonSerialized).

Deltas are stored as a string or byte array. For the purpose of consistancy and to remove a possible major bug point. Basically a turn delta should have all the correct movesets. Through repeating them they should do the exact same thing. You should be able to acheive the same state.
Version updates -> see Problems.

Demons -> TBD Better name? -> Hadics
Action set, has a strict view of the world, and runs a algorithm that returns a moveset string. There needs to be a path finding system.
Also a path finding system that can be tuned, local pathfinder?
Research it, 

"Strict view of the world"
The world state needs to be seralizable, also hashable.
Only exists Tiles and Entities.

There needs to be a system in place to queue actions.

Just store all data with JSON, just have types you can read easily.

Tile focused rendering or map focused?