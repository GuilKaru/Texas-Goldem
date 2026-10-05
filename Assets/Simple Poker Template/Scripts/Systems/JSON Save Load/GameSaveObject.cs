using System.Collections.Generic;

namespace SimplePoker.SaveLoad
{
    [System.Serializable]

    /// <summary>
    /// Represents the object used for saving game data.
    /// </summary>
    public class GameSaveObject
    {
        public PlayerSave Player = new PlayerSave();
        public List<LevelSave> Levels = new List<LevelSave>();
    }
}