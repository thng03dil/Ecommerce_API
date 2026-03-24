using System.Collections.Concurrent;
using System.Threading;

namespace Ecommerce.Application.Common.Auth
{
    
    public static class UserAuthLockRegistry
    {
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> Locks = new();

        public static SemaphoreSlim GetLock(int userId) =>
            Locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
    }
}
