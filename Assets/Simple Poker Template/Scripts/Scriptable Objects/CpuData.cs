using UnityEngine;

namespace SimplePoker.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewCpu", menuName = "Poker Data/Cpu Base Data", order = 1)]
    public class CpuData : ScriptableObject
    {
        public Sprite Portrait;
        public string Name;
        public TurnChoose Ante;
        public TurnChoose Flop;
        public TurnChoose Turn;
        public TurnChoose River;
    }

    [System.Serializable]
    public class TurnChoose
    {
        [Range(0, 100)] public int Fold = 20;
        [Range(0, 100)] public int Check = 50;
        [Range(0, 100)] public int Bet = 20;
        [Range(0, 100)] public int AllIn = 10;
    }
}

