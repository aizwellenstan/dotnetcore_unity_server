using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ETModel
{
    public static class MessageHelper
    {
        public readonly static Dictionary<ushort, AppType> opcodeToAppTypeMap = new Dictionary<ushort, AppType>();

        public readonly static Dictionary<ushort, ushort> opcodeToRetOpcodeMap = new Dictionary<ushort, ushort>();
    }
}
