namespace SimplePoker.SaveLoad
{
    [System.Serializable]
    public class PlayerSave
    {
        public int Chips = 200000;
        public int PortraitID = 0;

        public PlayerSave()
        {
            if (Chips >= 99999999)
                Chips = 99999999;
        }
    }
}