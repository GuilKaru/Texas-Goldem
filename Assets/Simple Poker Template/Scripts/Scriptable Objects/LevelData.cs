using UnityEngine;

namespace SimplePoker.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "Poker Data/Level Base Data", order = 1)]
    public class LevelData : ScriptableObject
    {
        [Range(2, 8)] public int AmountOfPlayers = 2;
        [Range(500, 1000000)] public int Ticket = 500;
        public int Reward = 1000;
        public string CityName = "City Name";
        public Sprite CityPhoto;
    }
}