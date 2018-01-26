using System;

namespace Topshelf.Leader.HighAvailability
{
    public class LeaderConfiguration
    {
        public LeaderConfiguration(string uniqueIdentifier, ILeaderManager leaderManager, TimeSpan heartBeatEvery, TimeSpan leaderCheckEvery)
        {
            LeaderManager = leaderManager ?? throw new ArgumentNullException(nameof(leaderManager));
            if (heartBeatEvery <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(heartBeatEvery), "Must not be less than or equal to zero.");
            }

            if (leaderCheckEvery <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(heartBeatEvery), "Must not be less than or equal to zero.");
            }

            if (string.IsNullOrEmpty(uniqueIdentifier))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(uniqueIdentifier));
            }

            UniqueIdentifier = uniqueIdentifier;
            HeartBeatEvery = heartBeatEvery;
            LeaderCheckEvery = leaderCheckEvery;
        }

        public string UniqueIdentifier { get; }

        public TimeSpan HeartBeatEvery { get; }

        public TimeSpan LeaderCheckEvery { get; }

        public ILeaderManager LeaderManager { get; }
    }
}