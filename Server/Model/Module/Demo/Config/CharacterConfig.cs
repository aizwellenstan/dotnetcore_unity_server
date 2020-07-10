namespace ETModel
{
	[Config((int)(AppType.ClientH | AppType.Gate | AppType.Map | AppType.Lobby))]
	public partial class CharacterConfigCategory : ACategory<CharacterConfig>
	{
	}

	public class CharacterConfig: IConfig
	{
		public long Id { get; set; }
		public int Type;
		public string Name;
		public int[] Color;
		public int Off;
		public int Gender;
		public string Icon;
		public int IsStack;
		public int MaxCountOnSlot;
		public long UseExpireAt;
		public int MaxCountOnBag;
	}
}
