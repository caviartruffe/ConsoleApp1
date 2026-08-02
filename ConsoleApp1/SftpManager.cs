using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manage
{
    public class SftpManager
    {
        public static async Task<bool> RunAsync(ConcurrentDictionary<int, InfoDocument> _sftpPool)
        {
            foreach (var pair in _sftpPool)
            {
                // 進行状況で
                if (pair.Key == 0)
                {
                }
            }
            return true;
        }
    }
}
