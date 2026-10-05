namespace SimplePoker.SaveLoad
{
    /// <summary>
    /// Static class for managing game data in the local database.
    /// </summary>
    public static class DatabaseManager
    {
        /// <summary>
        /// Saves the current game state.
        /// </summary>
        /// <param name="gameSave">The object containing the game state to be saved.</param>
        public static void SaveGame(GameSaveObject gameSave)
        {
            JsonSave.Save(JsonSave.DEFAULT_GAME_SAVE, gameSave, "All saved");
        }

        /// <summary>
        /// Loads the saved game state.
        /// </summary>
        /// <returns>The loaded game state.</returns>
        public static GameSaveObject LoadGame()
        {
            return JsonSave.Load<GameSaveObject>(JsonSave.DEFAULT_GAME_SAVE, "All loaded");
        }

        /// <summary>
        /// Saves the player's portrait ID.
        /// </summary>
        /// <param name="portraitID">The ID of the player's portrait.</param>
        public static void SavePlayerPortrait(int portraitID)
        {
            GameSaveObject gameSave = JsonSave.Load<GameSaveObject>(JsonSave.DEFAULT_GAME_SAVE);
            gameSave.Player.PortraitID = portraitID;
            JsonSave.Save(JsonSave.DEFAULT_GAME_SAVE, gameSave, "Save portrait");
        }

        /// <summary>
        /// Loads the player's portrait ID.
        /// </summary>
        /// <returns>The ID of the player's portrait.</returns>
        public static int LoadPlayerPortrait()
        {
            GameSaveObject gameSave = JsonSave.Load<GameSaveObject>(JsonSave.DEFAULT_GAME_SAVE, "Load portrait");
            return gameSave.Player.PortraitID;
        }

        /// <summary>
        /// Saves the player's chips.
        /// </summary>
        /// <param name="chips">The number of chips to be saved.</param>
        public static void SavePlayerChips(int chips)
        {
            GameSaveObject gameSave = JsonSave.Load<GameSaveObject>(JsonSave.DEFAULT_GAME_SAVE);
            gameSave.Player.Chips = chips;
            JsonSave.Save(JsonSave.DEFAULT_GAME_SAVE, gameSave, "Save chips");
        }

        /// <summary>
        /// Adds chips to the player's current chip count and saves the updated value.
        /// </summary>
        /// <param name="chips">The number of chips to add.</param>
        public static void AddPlayerChipsAndSave(int chips)
        {
            GameSaveObject gameSave = JsonSave.Load<GameSaveObject>(JsonSave.DEFAULT_GAME_SAVE);
            gameSave.Player.Chips = gameSave.Player.Chips + chips;
            if (gameSave.Player.Chips >= 99999999)
                gameSave.Player.Chips = 99999999;

            JsonSave.Save(JsonSave.DEFAULT_GAME_SAVE, gameSave, "Save chips");
        }

        /// <summary>
        /// Subtracts chips from the player's current chip count and saves the updated value.
        /// </summary>
        /// <param name="chips">The number of chips to subtract.</param>
        public static void SubPlayerChipsAndSave(int chips)
        {
            GameSaveObject gameSave = JsonSave.Load<GameSaveObject>(JsonSave.DEFAULT_GAME_SAVE);
            gameSave.Player.Chips = gameSave.Player.Chips - chips;
            if (gameSave.Player.Chips <= 0)
                gameSave.Player.Chips = 0;

            JsonSave.Save(JsonSave.DEFAULT_GAME_SAVE, gameSave, "Save chips");
        }

        /// <summary>
        /// Loads the player's current chip count.
        /// </summary>
        /// <returns>The number of chips the player has.</returns>
        public static int LoadPlayerChips()
        {
            GameSaveObject gameSave = JsonSave.Load<GameSaveObject>(JsonSave.DEFAULT_GAME_SAVE, "Load chips");
            return gameSave.Player.Chips;
        }

        /// <summary>
        /// Deletes the saved game data.
        /// </summary>
        public static void DeleteSave()
        {
            JsonSave.DeleteSave(JsonSave.DEFAULT_GAME_SAVE);
        }
    }
}