using SimplePoker.Helper;
using UnityEngine;

namespace SimplePoker.UnityAds
{
    /// <summary>
    /// Manages rewarded values and provides methods to retrieve and update them.
    /// </summary>
    public class RewardedManager
    {
        private int rewardIndex = 0;
        private int rewardValue;

        private int[] rewards = { 25000, 100000, 200000 };

        public RewardedManager()
        {
            rewardIndex = PlayerPrefs.GetInt("ads_index", 0);
        }

        /// <summary>
        /// Changes the reward index to the next one in the rewards array.
        /// </summary>
        public void ChangeToNextRewardValue()
        {
            rewardIndex = (rewardIndex + 1) % rewards.Length;
            PlayerPrefs.SetInt("ads_index", rewardIndex);
        }

        /// <summary>
        /// Retrieves the current reward value.
        /// </summary>
        /// <returns>The current reward value.</returns>
        public int GetRewardValue()
        {
            rewardIndex = PlayerPrefs.GetInt("ads_index", 0);
            int reward = rewards[rewardIndex];
            return reward;
        }

        /// <summary>
        /// Retrieves the reward value formatted as a string in USD format.
        /// </summary>
        /// <returns>The reward value formatted as a string.</returns>
        public string GetRewardStringFormat()
        {
            return Helpers.AbbreviateMoneyUSD(GetRewardValue());
        }
    }
}