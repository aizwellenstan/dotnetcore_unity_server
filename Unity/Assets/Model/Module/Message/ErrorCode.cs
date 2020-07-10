namespace ETModel
{
	public static class ErrorCode
	{
		public const int ERR_Success = 0;
		
		// 1-11004 是SocketError请看SocketError定义
		//-----------------------------------
		// 100000 以上，避免跟SocketError冲突
		public const int ERR_MyErrorCode = 100000;
		
		public const int ERR_ActorNoMailBoxComponent = 100003;
		public const int ERR_ActorRemove = 100004;
		public const int ERR_PacketParserError = 100005;
		
		public const int ERR_KcpCantConnect = 102005;
		public const int ERR_KcpChannelTimeout = 102006;
		public const int ERR_KcpRemoteDisconnect = 102007;
		public const int ERR_PeerDisconnect = 102008;
		public const int ERR_SocketCantSend = 102009;
		public const int ERR_SocketError = 102010;
		public const int ERR_KcpWaitSendSizeTooLarge = 102011;

		public const int ERR_WebsocketPeerReset = 103001;
		public const int ERR_WebsocketMessageTooBig = 103002;
		public const int ERR_WebsocketError = 103003;
		public const int ERR_WebsocketConnectError = 103004;
		public const int ERR_WebsocketSendError = 103005;
		public const int ERR_WebsocketRecvError = 103006;
		
		public const int ERR_RpcFail = 102001;
		public const int ERR_ReloadFail = 102003;
		public const int ERR_ConnectGateKeyError = 100105;
        public const int ERR_ActorLocationNotFound = 102004;
		//-----------------------------------
		// 小于这个Rpc会抛异常，大于这个异常的error需要自己判断处理，也就是说需要处理的错误应该要大于该值
		public const int ERR_Exception = 200000;
		
		public const int ERR_NotFoundActor = 200002;
		
		public const int ERR_AccountOrPasswordError = 200102;

        public const int ERR_LocationWaitLock = 200200;
        public const int ERR_LocationUnlockFailed = 200201;

        //------開始頁面相關------

        /*註冊*/
        public const int ERR_SignUpFailed = 300000;
        public const int ERR_AccountSisnUpRepeatly = 300001;
	    public const int ERR_InvalidEmailFormat = 300002;
	    public const int ERR_InvalidPasswordFormat = 300003;
	    public const int ERR_DoubleCheckPasswordFailed = 300004;
        public const int ERR_SignUpByEmailFailed = 300005;
        public const int ERR_SignUpByFirebaseFailed_UserExist = 300006;
        /*登陸*/
        public const int ERR_SignInFailed = 300100;
	    public const int ERR_AccountDoesntExist = 300101;
	    public const int ERR_PasswordIncorrect = 300102;
	    public const int ERR_InvalidToken = 300103;
	    public const int ERR_FBSignInFailed = 300104;
        public const int ERR_AuthenticationIsNull = 300105;
        public const int ERR_AuthenticationTypeError = 300106;
        public const int ERR_InvalidDeviceUniqueIdentifier = 300107;
        public const int ERR_DeviceUniqueIdentifierIsNull = 300108;
        public const int ERR_DeviceUniqueIdentifierIsExist = 300109;

        /*綁定*/
        public const int ERR_LinkFailed = 300150;
        public const int ERR_LinkIsExist = 300151;

        /*登出*/
        public const int ERR_LogoutFailed = 300200;
        /*連線*/
        public const int ERR_SyncPlayerStateError = 300300;
        public const int ERR_RegisterServerRepeatly = 300301;

        //------END------

        //------戰鬥相關400000~400999------

        public const int ERR_RoomIdNotFound = 400000;
        public const int ERR_RoonTypeError = 400001;
        public const int ERR_RoomTeamComponentNull = 400002;
        public const int ERR_RoomTeamIsNotLeader = 400003;
        public const int ERR_RoomTeamStateCanNotToRun = 400004;
        public const int ERR_RoomTeamStateCanNotKick = 400005;
        public const int ERR_RoomTeamCanNotFindPlayerForKick = 400006;
        public const int ERR_RoomTeamStateCanNotInvite = 400007;
        public const int ERR_RoomTeamCanNotFindPlayerForInvite = 400008;
        public const int ERR_RoomTeamMemberIsFull = 400009;
        public const int ERR_RoomTeamStateCanNotEnter = 400010;
        public const int ERR_RoomTeamHaveNotReady = 400011;
        public const int ERR_MapUnitMissing = 400012;
        public const int ERR_TimeNotUp = 400013;
        public const int ERR_NotSupportedType = 400014;

        // 0518 Saitou add roomslimit
        public const int ERR_TooManyRooms = 400013;
        public const int ERR_RoomRoamingMemberIsFull = 400014;

        //------END------

        //------邀請相關401000~401999------

        public const int ERR_InviteIdNotFind = 401000;
        public const int ERR_InviteNotSelf = 401001;

        //------END------

        //------預約相關402000~402999------

        public const int ERR_ReservationIsFull = 402000;
        public const int ERR_ReservationIsNotLeader = 402001;
        public const int ERR_ReservationRoomStateCanNotToRemove = 402002;
        public const int ERR_ReservationIdNotFind = 402003;
        public const int ERR_ReservationRoomNotFind = 402004;
        public const int ERR_ReservationNotTheOwner = 402005;

        //------END------

        //------關係相關301000~301999------

        public const int ERR_AddRelationshipRepeatedly = 301000;
        public const int ERR_RelationshipApplyInfo_NotFind = 301001;
        public const int ERR_RelationshipApplyInfo_NotReceiver = 301003;
        public const int ERR_RelationshipApplyInfo_AddFailed = 301004;

        //------END------

        //------大廳相關302000~302999------

        public const int ERR_LobbyUnitMisiing = 302000;

        //------END------

        //------道具相關303000~303999------

        public const int ERR_EquipmentUnavailable = 303000;
        public const int ERR_EquipmentNotDefined = 303001;
        public const int ERR_EquipmentRecordError = 303002;
        public const int ERR_EquipmentBagOverLimit = 303003;
        public const int ERR_EquipmentBagIsEmpty = 303004;
        public const int ERR_EquipmentInvalidCount = 303005;
        public const int ERR_EquipmentNotEnough = 303006;
        public const int ERR_EquipmentOverOwnedLimit = 303007;

        //------END------

        /*------共用區------*/

        public const int ERR_UpdateFailed = 900000;
        public const int ERR_PlayerDoesntExist = 900001;

        /*------END------*/

        //-----------------------------------
        public static bool IsRpcNeedThrowException(int error)
		{
			if (error == 0)
			{
				return false;
			}

			if (error > ERR_Exception)
			{
				return false;
			}

			return true;
		}
	}
}