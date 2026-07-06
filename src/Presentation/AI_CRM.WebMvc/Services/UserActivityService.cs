using System;
using System.Collections.Concurrent;

namespace AI_CRM.WebMvc.Services
{
    public interface IUserActivityService
    {
        void UpdateUserActivity(int userId);
        bool IsUserOnline(int userId);
        int GetOnlineUsersCount();
    }

    public class UserActivityService : IUserActivityService
    {
        private readonly ConcurrentDictionary<int, DateTime> _userActivity = new ConcurrentDictionary<int, DateTime>();
        private readonly TimeSpan _onlineThreshold = TimeSpan.FromMinutes(5); // User is online if active in last 5 mins

        public void UpdateUserActivity(int userId)
        {
            _userActivity[userId] = DateTime.Now;
        }

        public bool IsUserOnline(int userId)
        {
            if (_userActivity.TryGetValue(userId, out var lastActivity))
            {
                return (DateTime.Now - lastActivity) <= _onlineThreshold;
            }
            return false;
        }

        public int GetOnlineUsersCount()
        {
            var cutoff = DateTime.Now - _onlineThreshold;
            int count = 0;
            foreach (var kvp in _userActivity)
            {
                if (kvp.Value >= cutoff)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
