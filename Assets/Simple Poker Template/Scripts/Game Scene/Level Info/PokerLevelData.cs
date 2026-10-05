using SimplePoker.ScriptableObjects;
using UnityEngine;

namespace SimplePoker.Data
{

    /// <summary>
    /// Singleton class for managing the data related to poker levels, 
    /// including CPU opponents and level-specific data.
    /// </summary>
    public class PokerLevelData : Singleton<PokerLevelData>
    {
        [field: SerializeField] public CpuData[] CpusData { get; private set; }
        [field: SerializeField] public LevelData LevelData { get; private set; }
        [field: SerializeField] public PlayerData PlayerData { get; private set; }

        private int CpuIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            CpusData = Resources.LoadAll<CpuData>("Scriptable Objects/CpuData/");
            LevelData = Resources.LoadAll<LevelData>("Scriptable Objects/LevelData/")[0];
            PlayerData = Resources.LoadAll<PlayerData>("Scriptable Objects/PlayerData/")[0];
        }

        /// <summary>
        /// Sets the data for the poker match, including the level data and shuffling the CPU opponents.
        /// </summary>
        /// <param name="levelData">The level data for the poker match.</param>
        public void SetPokerMatchData(LevelData levelData)
        {
            LevelData = levelData;
            ShuffleCpus();
            CpuIndex = 0;
        }

        /// <summary>
        /// Shuffles the CPU opponents for the poker match.
        /// </summary>
        private void ShuffleCpus()
        {
            for (int i = 0; i < CpusData.Length; i++)
            {
                CpuData cpuAux = CpusData[i];
                int randomCpu = Random.Range(0, CpusData.Length);
                CpusData[i] = CpusData[randomCpu];
                CpusData[randomCpu] = cpuAux;
            }
        }

        /// <summary>
        /// Gets the next CPU opponent for the poker match.
        /// </summary>
        /// <returns>The next CPU opponent.</returns>
        public CpuData GetNextCpu()
        {
            CpuData cpu = CpusData[CpuIndex];
            CpuIndex = (CpuIndex + 1) % CpusData.Length;
            return cpu;
        }
    }
}