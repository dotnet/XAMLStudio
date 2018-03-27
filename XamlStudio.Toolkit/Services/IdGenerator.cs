using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlStudio.Toolkit.Services
{
    /// <summary>
    /// Simple thread-safe unique integer Id Generator.
    /// 
    /// Initial Count starts at 1.
    /// </summary>
    public static class IdGenerator
    {
        public static object lockable = new object();
        public static int _count = 1;

        public static int Next()
        {
            int? i = null;
            lock(lockable)
            {
                i = ++_count;
            }

            return (int)i;
        }
    }
}