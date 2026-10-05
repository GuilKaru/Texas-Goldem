using UnityEngine;

namespace SimplePoker.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewPlayer", menuName = "Poker Data/Player Base Data", order = 1)]
    public class PlayerData : ScriptableObject
    {
        public Sprite[] Portraits;
        public string Name = "You";
    }
}