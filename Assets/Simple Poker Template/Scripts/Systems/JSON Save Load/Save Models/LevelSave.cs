namespace SimplePoker.SaveLoad
{
    [System.Serializable]
    public class LevelSave
    {
        public int LevelNumber;
        public int PlayersAmount = 2;
        public int Ticket = 2000;
        public int Reward = 30000;
        public bool Completed = false;
    }
}