using System.Collections.Generic;
using System.Net;

namespace ETModel
{
	public class LocationProxyComponent : Component
	{
		public IPEndPoint LocationAddress;

		public readonly Dictionary<long, Dictionary<long, ETTaskCompletionSource>> lockDict = new Dictionary<long, Dictionary<long, ETTaskCompletionSource>>();
	}
}