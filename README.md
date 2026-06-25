# UnityInternTest

1. Core Mechanics Shift
* **Removal of Swapping & Gravity:** The old drag-to-swap logic, neighbor checking, and vertical gravity (collapsing rows) have been removed from `Board.cs` and `BoardController.cs`.
* **Click-to-Move:** Tapping an item on the board now frees it from its board cell and sends it to `ContainerManager`.
* **Win/Lose Paradigm:** The game no longer relies on timers or move limits. 
  * **Win Condition:** The player wins when `Board.IsEmpty()` returns true.
  * **Lose Condition:** The player loses when the bottom `Container` becomes full (5 items) and no matches are formed.

2. `Container` Logic (New)
A custom `Container.cs` class was built to handle the bottom holding area. 
* **Capacity Tracking:** It holds a fixed maximum array of cells (5 by default, set via `ContainerSettings`).
* **Auto-Sorting:** When an item is added via `TryAddItem`, the container scans its items. Instead of appending the item to the end, it inserts the new item directly next to any existing items of the same type, shifting items to the right using `DOTween`.
* **Match Clearing:** `CheckAndClearMatches()` scans the container horizontally. If 3 matching items are found, they are exploded, and `ShiftItemLeft()` pulls the remaining items over to close the gap.

3. Game State & Modes Refactor (`GameManager.cs`)
The `GameManager` removed the old `LevelCondition` pattern (`LevelTime.cs` and `LevelMoves.cs` were removed from the game loop). It now use `OnWin()` and `OnLose()`. 

Additionally, the `eLevelMode` enum was updated to support four distinct game states:
1. **NORMAL:** The standard Triple-Match puzzle.
2. **AUTO_WIN:** Runs a coroutine bot that flawlessly clears the board.
3. **AUTO_LOSE:** Runs a coroutine bot that intentionally fills the container without matching.
4. **TIME_ATTACK:** A new game mode with specific rules