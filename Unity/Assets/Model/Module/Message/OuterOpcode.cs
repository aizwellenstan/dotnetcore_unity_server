using ETModel;
namespace ETModel
{
	[Message(OuterOpcode.Actor_TransferRequest)]
	public partial class Actor_TransferRequest : IActorLocationRequest {}

	[Message(OuterOpcode.Actor_TransferResponse)]
	public partial class Actor_TransferResponse : IActorLocationResponse {}

	[Message(OuterOpcode.C2R_Ping)]
	public partial class C2R_Ping : IRequest {}

	[Message(OuterOpcode.R2C_Ping)]
	public partial class R2C_Ping : IResponse {}

	[Message(OuterOpcode.C2S_Ping)]
	public partial class C2S_Ping : IRequest {}

	[Message(OuterOpcode.S2C_Ping)]
	public partial class S2C_Ping : IResponse {}

	[Message(OuterOpcode.C2M_Reload)]
	public partial class C2M_Reload : IRequest {}

	[Message(OuterOpcode.M2C_Reload)]
	public partial class M2C_Reload : IResponse {}

}
namespace ETModel
{
	public static partial class OuterOpcode
	{
		 public const ushort Actor_TransferRequest = 101;
		 public const ushort Actor_TransferResponse = 102;
		 public const ushort C2R_Ping = 103;
		 public const ushort R2C_Ping = 104;
		 public const ushort C2S_Ping = 105;
		 public const ushort S2C_Ping = 106;
		 public const ushort C2M_Reload = 107;
		 public const ushort M2C_Reload = 108;
	}
}
