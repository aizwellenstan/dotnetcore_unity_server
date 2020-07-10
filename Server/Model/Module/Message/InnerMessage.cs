using ETModel;
using System.Collections.Generic;
namespace ETModel
{
/// <summary>
/// 传送unit
/// </summary>
//message M2M_TrasferUnitRequest // IRequest
//{
//    int32 RpcId = 90;
//    Unit Unit = 1;
//}
//
//message M2M_TrasferUnitResponse // IResponse
//{
//    int32 RpcId = 90;
//    int32 Error = 91;
//    string Message = 92;
//
//    int64 InstanceId = 1;
//}
	[Message(InnerOpcode.M2A_Reload)]
	public partial class M2A_Reload: IRequest
	{
		public int RpcId { get; set; }

	}

	[Message(InnerOpcode.A2M_Reload)]
	public partial class A2M_Reload: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.G2G_LockRequest)]
	public partial class G2G_LockRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public string Address { get; set; }

	}

	[Message(InnerOpcode.G2G_LockResponse)]
	public partial class G2G_LockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.G2G_LockReleaseRequest)]
	public partial class G2G_LockReleaseRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public string Address { get; set; }

	}

	[Message(InnerOpcode.G2G_LockReleaseResponse)]
	public partial class G2G_LockReleaseResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBSaveRequest)]
	public partial class DBSaveRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public ComponentWithId Component { get; set; }

	}

	[Message(InnerOpcode.DBSaveBatchResponse)]
	public partial class DBSaveBatchResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBSaveBatchRequest)]
	public partial class DBSaveBatchRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.DBSaveResponse)]
	public partial class DBSaveResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBQueryRequest)]
	public partial class DBQueryRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public string CollectionName { get; set; }

	}

	[Message(InnerOpcode.DBQueryResponse)]
	public partial class DBQueryResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public ComponentWithId Component { get; set; }

	}

	[Message(InnerOpcode.DBQueryBatchRequest)]
	public partial class DBQueryBatchRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public List<long> IdList = new List<long>();

	}

	[Message(InnerOpcode.DBQueryBatchResponse)]
	public partial class DBQueryBatchResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.DBQueryJsonRequest)]
	public partial class DBQueryJsonRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public string Json { get; set; }

	}

	[Message(InnerOpcode.DBQueryJsonResponse)]
	public partial class DBQueryJsonResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.DBQueryJsonCountRequest)]
	public partial class DBQueryJsonCountRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public string Json { get; set; }

	}

	[Message(InnerOpcode.DBQueryJsonSkipLimitRequest)]
	public partial class DBQueryJsonSkipLimitRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public string Json { get; set; }

		public long Skip { get; set; }

		public long Limit { get; set; }

	}

	[Message(InnerOpcode.DBQueryJsonCountResponse)]
	public partial class DBQueryJsonCountResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long Count { get; set; }

	}

	[Message(InnerOpcode.DBDeleteJsonRequest)]
	public partial class DBDeleteJsonRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public string Json { get; set; }

	}

	[Message(InnerOpcode.DBDeleteJsonResponse)]
	public partial class DBDeleteJsonResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

//Cache
//*************Start*************
	[Message(InnerOpcode.CacheQueryByIdRequest)]
	public partial class CacheQueryByIdRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public long Id { get; set; }

		public List<string> Fields = new List<string>();

	}

	[Message(InnerOpcode.CacheQueryByUniqueRequest)]
	public partial class CacheQueryByUniqueRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public string UniqueName { get; set; }

		public string Json { get; set; }

	}

	[Message(InnerOpcode.CacheCreateRequest)]
	public partial class CacheCreateRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public ComponentWithId Component { get; set; }

	}

	[Message(InnerOpcode.CacheUpdateByIdRequest)]
	public partial class CacheUpdateByIdRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public long Id { get; set; }

		public string DataJson { get; set; }

	}

	[Message(InnerOpcode.CacheQueryResponse)]
	public partial class CacheQueryResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public ComponentWithId Component { get; set; }

	}

	[Message(InnerOpcode.CacheDeleteByIdRequest)]
	public partial class CacheDeleteByIdRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public long Id { get; set; }

	}

	[Message(InnerOpcode.CacheDeleteByUniqueRequest)]
	public partial class CacheDeleteByUniqueRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public string UniqueName { get; set; }

		public string Json { get; set; }

	}

	[Message(InnerOpcode.CacheDeleteResponse)]
	public partial class CacheDeleteResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public bool IsSuccessful { get; set; }

	}

	[Message(InnerOpcode.CacheGetAllIdsRequest)]
	public partial class CacheGetAllIdsRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

	}

	[Message(InnerOpcode.CacheGetAllIdsResponse)]
	public partial class CacheGetAllIdsResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public List<long> IdList = new List<long>();

	}

//*************END*************
	[Message(InnerOpcode.ObjectAddRequest)]
	public partial class ObjectAddRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

		public long InstanceId { get; set; }

	}

	[Message(InnerOpcode.ObjectAddResponse)]
	public partial class ObjectAddResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectRemoveRequest)]
	public partial class ObjectRemoveRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

	}

	[Message(InnerOpcode.ObjectRemoveResponse)]
	public partial class ObjectRemoveResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectLockRequest)]
	public partial class ObjectLockRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

		public long InstanceId { get; set; }

		public int Time { get; set; }

	}

	[Message(InnerOpcode.ObjectLockResponse)]
	public partial class ObjectLockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectUnLockRequest)]
	public partial class ObjectUnLockRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

		public long OldInstanceId { get; set; }

		public long InstanceId { get; set; }

	}

	[Message(InnerOpcode.ObjectUnLockResponse)]
	public partial class ObjectUnLockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectGetRequest)]
	public partial class ObjectGetRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

	}

	[Message(InnerOpcode.ObjectGetResponse)]
	public partial class ObjectGetResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long InstanceId { get; set; }

	}

	[Message(InnerOpcode.R2G_GetLoginKey)]
	public partial class R2G_GetLoginKey: IRequest
	{
		public int RpcId { get; set; }

		public long Uid { get; set; }

	}

	[Message(InnerOpcode.G2R_GetLoginKey)]
	public partial class G2R_GetLoginKey: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long Key { get; set; }

	}

// TODO:Obsolete
	[Message(InnerOpcode.G2M_SessionDisconnect)]
	public partial class G2M_SessionDisconnect: IActorLocationMessage
	{
		public int RpcId { get; set; }

		public long ActorId { get; set; }

	}

	[Message(InnerOpcode.G2L_LobbyUnitCreate)]
	public partial class G2L_LobbyUnitCreate: IRequest
	{
		public int RpcId { get; set; }

		public long Uid { get; set; }

		public long GateSessionId { get; set; }

		public int GateAppId { get; set; }

		public int LobbyAppId { get; set; }

	}

	[Message(InnerOpcode.L2G_LobbyUnitCreate)]
	public partial class L2G_LobbyUnitCreate: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long Uid { get; set; }

		public string Json { get; set; }

	}

	[Message(InnerOpcode.G2L_LobbyUnitUpdate)]
	public partial class G2L_LobbyUnitUpdate: IRequest
	{
		public int RpcId { get; set; }

		public long Uid { get; set; }

		public bool IsOnline { get; set; }

	}

	[Message(InnerOpcode.L2G_LobbyUnitUpdate)]
	public partial class L2G_LobbyUnitUpdate: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.G2L_LobbyUnitDestroy)]
	public partial class G2L_LobbyUnitDestroy: IRequest
	{
		public int RpcId { get; set; }

		public long Uid { get; set; }

	}

	[Message(InnerOpcode.L2G_LobbyUnitDestroy)]
	public partial class L2G_LobbyUnitDestroy: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.L2M_SessionDisconnect)]
	public partial class L2M_SessionDisconnect: IActorLocationMessage
	{
		public int RpcId { get; set; }

		public long ActorId { get; set; }

	}

	[Message(InnerOpcode.S2M_RegisterService)]
	public partial class S2M_RegisterService: IRequest
	{
		public int RpcId { get; set; }

		public ComponentWithId Component { get; set; }

	}

	[Message(InnerOpcode.M2S_RegisterService)]
	public partial class M2S_RegisterService: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.M2A_RegisterService)]
	public partial class M2A_RegisterService: IMessage
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public ComponentWithId Component { get; set; }

	}

	[Message(InnerOpcode.S2A_ConnectService)]
	public partial class S2A_ConnectService: IRequest
	{
		public int RpcId { get; set; }

	}

	[Message(InnerOpcode.A2S_ConnectService)]
	public partial class A2S_ConnectService: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.S2L_LockEvent)]
	public partial class S2L_LockEvent: IRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public long Key { get; set; }

		public long Timeout { get; set; }

	}

	[Message(InnerOpcode.L2S_LockEvent)]
	public partial class L2S_LockEvent: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.S2L_UnlockEvent)]
	public partial class S2L_UnlockEvent: IRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public long Key { get; set; }

	}

	[Message(InnerOpcode.L2S_UnlockEvent)]
	public partial class L2S_UnlockEvent: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.L2S_ReceiveUnlockEvent)]
	public partial class L2S_ReceiveUnlockEvent: IMessage
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long Key { get; set; }

		public long Id { get; set; }

	}

}
