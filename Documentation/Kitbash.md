# Kitbash

Make the game but fast, see if it even works.
No seralization needed, no deltas needed. Though how to do reversal? Maybe you can undo moves by just manually putting things back.
No ui visualization. 
Just a b&w board, icons to represent things. 
	No actual moves? just a paper saying what each icon can do. Basically a board game.
	Aliens have strict moves on paper to.

Basically make a chess board.

## Sample Moves
Basic Zergling
Move 4
Health 2
Damage 1
Always: Run Towards Closest Enemy Building or Unit.

Speed Zergling
Move 8
Health 1
Damage 1
Always: Run Towards Closest Enemy Building

Brute Zergling
Move 4
Health 5
Damage If Moved last turn ? 3 : 1
Always: Run Towards Closest Enemy Building or Unit.



Angels takes three turns to recall back home. Full health after channeling.
Taking damage or moving interrupts it.
Angels can do a call to. 4 + Tiles away / 12

Basic Angel
4 : 4
Move 3
Health 9
Abilities:
	Do 2 damage to a unit and push everyone but yourself one tile away.
	Do 1 damage to three units right in front of you, in a row.

Seige Angel
4 : 8
Move 2
Health 6
Abilities:
	Do (hasMoved ? 2 : 4) damage to a unit max 12 tiles away, min 2 tiles away
	Do 1 damage and push the unit infront of you 6 tiles away and yourself 1 tile away


Buildings - repairs one health every 4 turns
Followers Hut
Health 6


Church
Health 12
Every 6 turns revive a new Angel.
Takes 12 turns to create a church