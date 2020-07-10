using ETModel;
using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ETHotfix
{
    // TODO:尚未處理熱更新(須改用組件的方式處理)
    public static class MessageHelper
    {
        private class OpPair
        {
            public ushort key { set; get; } = 0;

            public ushort value { set; get; } = 0;
        }

        private static Dictionary<ushort, AppType> opcodeToAppTypeMap => ETModel.MessageHelper.opcodeToAppTypeMap;

        private static Dictionary<ushort, ushort> opcodeToRetOpcodeMap => ETModel.MessageHelper.opcodeToRetOpcodeMap;

        private readonly static string[] lobbyPattern = new [] { "c2l_" , "l2c_" };

        private readonly static string[] mapPattern = new[] { "c2m_", "m2c_" };

        public static void Initialize()
        {
            List<Type> types = Game.EventSystem.GetTypes(typeof(MessageAttribute));
            Dictionary<string, OpPair> reqToResMap = new Dictionary<string, OpPair>();
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
                if (messageAttribute == null)
                {
                    continue;
                }

                string opName = type.Name.ToLower();

                if (lobbyPattern.Any(e => opName.Contains(e)))
                {
                    ushort c = messageAttribute.Opcode;
                    opcodeToAppTypeMap.Add(c, AppType.Lobby);
                }
                else if (mapPattern.Any(e => opName.Contains(e)))
                {
                    ushort c = messageAttribute.Opcode;
                    opcodeToAppTypeMap.Add(c, AppType.Map);
                }

                string[] sp = opName.Split('_');

                if(sp.Length < 2)
                {
                    continue;
                }

                string op = sp[1];

                if (type.GetInterfaces().Contains(typeof(IRequest)))
                {
                    reqToResMap.TryAdd(op, new OpPair());
                    reqToResMap[op].key = messageAttribute.Opcode;
                }
                else if (type.GetInterfaces().Contains(typeof(IResponse)))
                {
                    reqToResMap.TryAdd(op, new OpPair());
                    reqToResMap[op].value = messageAttribute.Opcode;
                }
            }

            foreach(var pair in reqToResMap)
            {
                if(pair.Value.key == 0 || pair.Value.value == 0)
                {
                    continue;
                }
                opcodeToRetOpcodeMap.Add(pair.Value.key, pair.Value.value);
            }
        }

        private static List<FieldInfo> GetConstants(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                 BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
        }

        public static AppType Get(ushort opcode)
        {
            opcodeToAppTypeMap.TryGetValue(opcode, out var appType);
            return appType;
        }

        public static ushort GetResponseOpcode(ushort opcode)
        {
            opcodeToRetOpcodeMap.TryGetValue(opcode, out ushort retOpcode);
            return retOpcode;
        }
    }
}
