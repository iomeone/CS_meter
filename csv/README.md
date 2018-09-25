The following columns are included in the csv data.

## Our label
`team_ct_wins_round`
- `True` if the counter-terrorists win the round. `False` if the terrorists win the round. This is our label and what we are training the model to predict.

---

## Non-player variables

`map_id`
- `str` - The name of the map. This is categorical, there are a small number of maps this could be.

`elapsed_since_bombplant`
- `float` - The number of seconds elapsed since the bomb was planted. Continuous value from 0-n where n is ~40 seconds. The bomb explodes after 40 seconds and the terrorists win the round. Due to frame latency it is possible for this value to be slightly above 40.

`bombplant_site`
- `char` - `'A'` or `'B'`, based on which bombsite the bomb was planted at.

`round_number` * (see below)
- `int` - Value will be >0, denotes the current round number.

`round_of_half` * (see below)
- `int` - Value will be from [1, 15] in regulation and [1,3] in overtime.

`rounds_per_half` * (see below)
- `int` - Value will be 15 in regulation and 3 in overtime.

\*  Due to how training data was generated this value may be slightly incorrect. We will want to analyze if the network needs or can make use of this variable.

---

## Player variables
Player variables are all named following the same convention. Column name is constructed of 3 parts:
1. Team prefix `team_ct_` or `team_t_`, to denote which team the player is a part of
2. Player prefix `player1_`, `player2_`, etc. depending on which player of the team the variable is for. Goes from player 1-5.
3. Variable suffix `is_alive`, `has_helmet`, etc...

Example variable names: team_ct_player1_is_alive, team_t_player3_has_kevlar, team_t_player5_equipped_weapon

`is_alive`
- `True` if the player is alive or `False` if the player is dead. If true, HP will be >0, if false HP will be 0.

`equipment_value`
- `int` - In-game dollar cost of a player's equipement at the frame of snapshot. Will still be >0 for dead players. We may want to modify this at the data-scrape level since any equipment value on a dead player is worthless and goes away at round end. Continuous value from 0-n, where n is the theoretical max equipment value for a player based on the game rules.

`equipped_weapon`
- `str` - Name of the weapon currently equipped by the player. `"Unknown"` for dead players. This is a categorical column as there are a fixed number of weapons.

`has_helmet`
- `True` if the player has a helmet. `False` if the player is dead or has no helmet.

`has_kevlar`
- `True` if the player has kevlar with durability >0. Kevlar duarbility has no impact in game with the exception that when the durability reaches 0 the effects of kevlar are gone. For that reason we only care about kevlar as a boolean rather than a scalar 0-100. `False` if the player is dead or has no kevlar.

`hp`
- `int` - HP value. Dead players will have 0 HP. Scalar value from 0-100.

`has_defuse_kit` **CT Players Only**
- `True` if the player has a defuse kit. `False` if the player is dead or has no kit. This variable is not present for `team_t_` as terrorist players cannot have a defuse kit.
