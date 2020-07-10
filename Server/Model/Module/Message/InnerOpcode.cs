namespace ETModel
{
	public static partial class InnerOpcode
	{
		 public const ushort M2A_Reload = 1001;
		 public const ushort A2M_Reload = 1002;
		 public const ushort G2G_LockRequest = 1003;
		 public const ushort G2G_LockResponse = 1004;
		 public const ushort G2G_LockReleaseRequest = 1005;
		 public const ushort G2G_LockReleaseResponse = 1006;
		 public const ushort DBSaveRequest = 1007;
		 public const ushort DBSaveBatchResponse = 1008;
		 public const ushort DBSaveBatchRequest = 1009;
		 public const ushort DBSaveResponse = 1010;
		 public const ushort DBQueryRequest = 1011;
		 public const ushort DBQueryResponse = 1012;
		 public const ushort DBQueryBatchRequest = 1013;
		 public const ushort DBQueryBatchResponse = 1014;
		 public const ushort DBQueryJsonRequest = 1015;
		 public const ushort DBQueryJsonResponse = 1016;
		 public const ushort DBQueryJsonCountRequest = 1017;
		 public const ushort DBQueryJsonSkipLimitRequest = 1018;
		 public const ushort DBQueryJsonCountResponse = 1019;
		 public const ushort DBDeleteJsonRequest = 1020;
		 public const ushort DBDeleteJsonResponse = 1021;
		 public const ushort CacheQueryByIdRequest = 1022;
		 public const ushort CacheQueryByUniqueRequest = 1023;
		 public const ushort CacheCreateRequest = 1024;
		 public const ushort CacheUpdateByIdRequest = 1025;
		 public const ushort CacheQueryResponse = 1026;
		 public const ushort CacheDeleteByIdRequest = 1027;
		 public const ushort CacheDeleteByUniqueRequest = 1028;
		 public const ushort CacheDeleteResponse = 1029;
		 public const ushort CacheGetAllIdsRequest = 1030;
		 public const ushort CacheGetAllIdsResponse = 1031;
		 public const ushort ObjectAddRequest = 1032;
		 public const ushort ObjectAddResponse = 1033;
		 public const ushort ObjectRemoveRequest = 1034;
		 public const ushort ObjectRemoveResponse = 1035;
		 public const ushort ObjectLockRequest = 1036;
		 public const ushort ObjectLockResponse = 1037;
		 public const ushort ObjectUnLockRequest = 1038;
		 public const ushort ObjectUnLockResponse = 1039;
		 public const ushort ObjectGetRequest = 1040;
		 public const ushort ObjectGetResponse = 1041;
		 public const ushort R2G_GetLoginKey = 1042;
		 public const ushort G2R_GetLoginKey = 1043;
		 public const ushort G2M_SessionDisconnect = 1044;
		 public const ushort G2L_LobbyUnitCreate = 1045;
		 public const ushort L2G_LobbyUnitCreate = 1046;
		 public const ushort G2L_LobbyUnitUpdate = 1047;
		 public const ushort L2G_LobbyUnitUpdate = 1048;
		 public const ushort G2L_LobbyUnitDestroy = 1049;
		 public const ushort L2G_LobbyUnitDestroy = 1050;
		 public const ushort L2M_SessionDisconnect = 1051;
		 public const ushort S2M_RegisterService = 1052;
		 public const ushort M2S_RegisterService = 1053;
		 public const ushort M2A_RegisterService = 1054;
		 public const ushort S2A_ConnectService = 1055;
		 public const ushort A2S_ConnectService = 1056;
		 public const ushort S2L_LockEvent = 1057;
		 public const ushort L2S_LockEvent = 1058;
		 public const ushort S2L_UnlockEvent = 1059;
		 public const ushort L2S_UnlockEvent = 1060;
		 public const ushort L2S_ReceiveUnlockEvent = 1061;
	}
}
