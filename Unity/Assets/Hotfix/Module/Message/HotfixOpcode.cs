using ETModel;
namespace ETHotfix
{
	[Message(HotfixOpcode.PlayerBaseInfo)]
	public partial class PlayerBaseInfo {}

	[Message(HotfixOpcode.PlayerCharSetting)]
	public partial class PlayerCharSetting {}

	[Message(HotfixOpcode.RideTotalInfo)]
	public partial class RideTotalInfo {}

	[Message(HotfixOpcode.PlayerRideTotalInfo)]
	public partial class PlayerRideTotalInfo {}

	[Message(HotfixOpcode.PlayerRideRoadInfo)]
	public partial class PlayerRideRoadInfo {}

	[Message(HotfixOpcode.G2C_MessageTip)]
	public partial class G2C_MessageTip : IActorMessage {}

	[Message(HotfixOpcode.G2C_UpdatePlayerRideTotalInfo)]
	public partial class G2C_UpdatePlayerRideTotalInfo : IActorMessage {}

	[Message(HotfixOpcode.MapUnitInfo)]
	public partial class MapUnitInfo {}

	[Message(HotfixOpcode.MapUnitInfo_Global)]
	public partial class MapUnitInfo_Global {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2M_MapUnitCreate)]
	public partial class G2M_MapUnitCreate : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.M2G_MapUnitCreate)]
	public partial class M2G_MapUnitCreate : IResponse {}

	[Message(HotfixOpcode.M2C_MapUnitCreate)]
	public partial class M2C_MapUnitCreate : IActorMessage {}

	[Message(HotfixOpcode.M2C_MapUnitUpdate)]
	public partial class M2C_MapUnitUpdate : IActorMessage {}

	[Message(HotfixOpcode.M2C_MapUnitDestroy)]
	public partial class M2C_MapUnitDestroy : IActorMessage {}

	[Message(HotfixOpcode.M2C_MapUnitCreateAndDestroy)]
	public partial class M2C_MapUnitCreateAndDestroy : IActorMessage {}

	[Message(HotfixOpcode.C2M_MapUnitMove)]
	public partial class C2M_MapUnitMove : IActorLocationMessage {}

	[Message(HotfixOpcode.C2M_LookAtTarget)]
	public partial class C2M_LookAtTarget : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_LookAtTarget)]
	public partial class M2C_LookAtTarget : IActorLocationResponse {}

	[Message(HotfixOpcode.M2C_MapUnitGlobalCreate)]
	public partial class M2C_MapUnitGlobalCreate : IActorMessage {}

	[Message(HotfixOpcode.M2C_MapUnitGlobalDestroy)]
	public partial class M2C_MapUnitGlobalDestroy : IActorMessage {}

	[Message(HotfixOpcode.RoomInfo)]
	public partial class RoomInfo {}

	[Message(HotfixOpcode.RoamingRoomData)]
	public partial class RoamingRoomData {}

//TODO:Obsolete
	[Message(HotfixOpcode.C2G_RoamingGetList)]
	public partial class C2G_RoamingGetList : IRequest {}

//TODO:Obsolete
	[Message(HotfixOpcode.G2C_RoamingGetList)]
	public partial class G2C_RoamingGetList : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_RoamingEnter)]
	public partial class C2G_RoamingEnter : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_RoamingEnter)]
	public partial class G2C_RoamingEnter : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2M_RoamingLeave)]
	public partial class C2M_RoamingLeave : IActorLocationRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.M2C_RoamingLeave)]
	public partial class M2C_RoamingLeave : IActorLocationResponse {}

	[Message(HotfixOpcode.C2M_SaveRideRecord)]
	public partial class C2M_SaveRideRecord : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_SaveRideRecord)]
	public partial class M2C_SaveRideRecord : IActorLocationResponse {}

	[Message(HotfixOpcode.ReservationAllData)]
	public partial class ReservationAllData {}

	[Message(HotfixOpcode.ReservationData)]
	public partial class ReservationData {}

	[Message(HotfixOpcode.ReservationMemberData)]
	public partial class ReservationMemberData {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamReservationGetList)]
	public partial class C2G_TeamReservationGetList : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamReservationGetList)]
	public partial class G2C_TeamReservationGetList : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamReservationCancel)]
	public partial class C2G_TeamReservationCancel : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamReservationCancel)]
	public partial class G2C_TeamReservationCancel : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamReservationJoin)]
	public partial class C2G_TeamReservationJoin : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamReservationJoin)]
	public partial class G2C_TeamReservationJoin : IResponse {}

	[Message(HotfixOpcode.G2C_TeamReservationAdd)]
	public partial class G2C_TeamReservationAdd : IActorMessage {}

	[Message(HotfixOpcode.G2C_TeamReservationRemove)]
	public partial class G2C_TeamReservationRemove : IActorMessage {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamReservationCreate)]
	public partial class C2G_TeamReservationCreate : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamReservationCreate)]
	public partial class G2C_TeamReservationCreate : IResponse {}

	[Message(HotfixOpcode.InviteData)]
	public partial class InviteData {}

	[Message(HotfixOpcode.C2M_TeamInvite)]
	public partial class C2M_TeamInvite : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_TeamInvite)]
	public partial class M2C_TeamInvite : IActorLocationResponse {}

	[Message(HotfixOpcode.G2C_TeamInviteReceiver)]
	public partial class G2C_TeamInviteReceiver : IActorMessage {}

	[Message(HotfixOpcode.G2C_TeamInviteTargerRefuse)]
	public partial class G2C_TeamInviteTargerRefuse : IActorMessage {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamInviteGetList)]
	public partial class C2G_TeamInviteGetList : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamInviteGetList)]
	public partial class G2C_TeamInviteGetList : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamInviteAccept)]
	public partial class C2G_TeamInviteAccept : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamInviteAccept)]
	public partial class G2C_TeamInviteAccept : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamInviteRefuse)]
	public partial class C2G_TeamInviteRefuse : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamInviteRefuse)]
	public partial class G2C_TeamInviteRefuse : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamInviteRefuseAll)]
	public partial class C2G_TeamInviteRefuseAll : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamInviteRefuseAll)]
	public partial class G2C_TeamInviteRefuseAll : IResponse {}

	[Message(HotfixOpcode.TeamMemberData)]
	public partial class TeamMemberData {}

	[Message(HotfixOpcode.TeamRoomData)]
	public partial class TeamRoomData {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamGetList)]
	public partial class C2G_TeamGetList : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamGetList)]
	public partial class G2C_TeamGetList : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamEnter)]
	public partial class C2G_TeamEnter : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamEnter)]
	public partial class G2C_TeamEnter : IResponse {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_TeamCreate)]
	public partial class C2G_TeamCreate : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_TeamCreate)]
	public partial class G2C_TeamCreate : IResponse {}

	[Message(HotfixOpcode.M2C_TeamGoBattleProgressReceiver)]
	public partial class M2C_TeamGoBattleProgressReceiver : IActorMessage {}

	[Message(HotfixOpcode.M2C_TeamGoBattleProgressAllDone)]
	public partial class M2C_TeamGoBattleProgressAllDone : IActorMessage {}

	[Message(HotfixOpcode.M2C_TeamGoBattle)]
	public partial class M2C_TeamGoBattle : IActorMessage {}

// 自己的unit id
// 全體玩家資訊
	[Message(HotfixOpcode.M2C_TeamGoLobby)]
	public partial class M2C_TeamGoLobby : IActorMessage {}

	[Message(HotfixOpcode.M2C_TeamLose)]
	public partial class M2C_TeamLose : IActorMessage {}

// 失去隊伍原因
	[Message(HotfixOpcode.M2C_TeamModifyData)]
	public partial class M2C_TeamModifyData : IActorMessage {}

	[Message(HotfixOpcode.M2C_TeamModifyMember)]
	public partial class M2C_TeamModifyMember : IActorMessage {}

	[Message(HotfixOpcode.M2C_TeamReadyModify)]
	public partial class M2C_TeamReadyModify : IActorMessage {}

	[Message(HotfixOpcode.C2M_TeamGoBattleProgress)]
	public partial class C2M_TeamGoBattleProgress : IActorLocationMessage {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2M_TeamLeave)]
	public partial class C2M_TeamLeave : IActorLocationRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.M2C_TeamLeave)]
	public partial class M2C_TeamLeave : IActorLocationResponse {}

	[Message(HotfixOpcode.C2M_TeamReady)]
	public partial class C2M_TeamReady : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_TeamReady)]
	public partial class M2C_TeamReady : IActorLocationResponse {}

	[Message(HotfixOpcode.C2M_TeamRun)]
	public partial class C2M_TeamRun : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_TeamRun)]
	public partial class M2C_TeamRun : IActorLocationResponse {}

	[Message(HotfixOpcode.C2M_TeamKick)]
	public partial class C2M_TeamKick : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_TeamKick)]
	public partial class M2C_TeamKick : IActorLocationResponse {}

	[Message(HotfixOpcode.C2M_TeamDeliveryLeader)]
	public partial class C2M_TeamDeliveryLeader : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_TeamDeliveryLeader)]
	public partial class M2C_TeamDeliveryLeader : IActorLocationResponse {}

	[Message(HotfixOpcode.C2M_TeamDisband)]
	public partial class C2M_TeamDisband : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_TeamDisband)]
	public partial class M2C_TeamDisband : IActorLocationResponse {}

	[Message(HotfixOpcode.BattleLeaderboardUnitInfo)]
	public partial class BattleLeaderboardUnitInfo {}

	[Message(HotfixOpcode.M2C_BattleLeaderboard)]
	public partial class M2C_BattleLeaderboard : IActorMessage {}

	[Message(HotfixOpcode.G2C_ForceDisconnect)]
	public partial class G2C_ForceDisconnect : IActorMessage {}

	[Message(HotfixOpcode.PlayerStateData)]
	public partial class PlayerStateData {}

// TODO:Obsolete
	[Message(HotfixOpcode.C2G_SyncPlayerState)]
	public partial class C2G_SyncPlayerState : IRequest {}

// TODO:Obsolete
	[Message(HotfixOpcode.G2C_SyncPlayerState)]
	public partial class G2C_SyncPlayerState : IResponse {}

	[Message(HotfixOpcode.C2G_SyncPlayerState_Return)]
	public partial class C2G_SyncPlayerState_Return : IRequest {}

	[Message(HotfixOpcode.G2C_SyncPlayerState_Return)]
	public partial class G2C_SyncPlayerState_Return : IResponse {}

	[Message(HotfixOpcode.C2R_SignUp)]
	public partial class C2R_SignUp : IRequest {}

	[Message(HotfixOpcode.R2C_SignUp)]
	public partial class R2C_SignUp : IResponse {}

	[Message(HotfixOpcode.AuthenticationInfo)]
	public partial class AuthenticationInfo {}

	[Message(HotfixOpcode.C2R_Authentication)]
	public partial class C2R_Authentication : IRequest {}

	[Message(HotfixOpcode.R2C_Authentication)]
	public partial class R2C_Authentication : IResponse {}

//錯誤列表
//需要使用的資料
//連接第三方平台
	[Message(HotfixOpcode.C2G_Logout)]
	public partial class C2G_Logout : IRequest {}

	[Message(HotfixOpcode.G2C_Logout)]
	public partial class G2C_Logout : IResponse {}

	[Message(HotfixOpcode.LinkInfo)]
	public partial class LinkInfo {}

	[Message(HotfixOpcode.C2L_Link)]
	public partial class C2L_Link : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_Link)]
	public partial class L2C_Link : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_UpdateUserProfile)]
	public partial class C2L_UpdateUserProfile : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_UpdateUserProfile)]
	public partial class L2C_UpdateUserProfile : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_UpdateUserEquip)]
	public partial class C2L_UpdateUserEquip : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_UpdateUserEquip)]
	public partial class L2C_UpdateUserEquip : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_UpdateUserLanguage)]
	public partial class C2L_UpdateUserLanguage : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_UpdateUserLanguage)]
	public partial class L2C_UpdateUserLanguage : IActorLocationResponse {}

	[Message(HotfixOpcode.EquipmentInfo)]
	public partial class EquipmentInfo {}

	[Message(HotfixOpcode.UserBagCapacity)]
	public partial class UserBagCapacity {}

	[Message(HotfixOpcode.C2L_GetUserAllEquipment)]
	public partial class C2L_GetUserAllEquipment : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_GetUserAllEquipment)]
	public partial class L2C_GetUserAllEquipment : IActorLocationResponse {}

	[Message(HotfixOpcode.L2C_OnEquipmentsCreated)]
	public partial class L2C_OnEquipmentsCreated : IActorMessage {}

	[Message(HotfixOpcode.L2C_OnEquipmentsDeleted)]
	public partial class L2C_OnEquipmentsDeleted : IActorMessage {}

	[Message(HotfixOpcode.RelationshipSimpleInfo)]
	public partial class RelationshipSimpleInfo {}

	[Message(HotfixOpcode.C2L_GetRelationshipList)]
	public partial class C2L_GetRelationshipList : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_GetRelationshipList)]
	public partial class L2C_GetRelationshipList : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_AddRelationship)]
	public partial class C2L_AddRelationship : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_AddRelationship)]
	public partial class L2C_AddRelationship : IActorLocationResponse {}

	[Message(HotfixOpcode.L2C_NotifyRelationshipState)]
	public partial class L2C_NotifyRelationshipState : IActorMessage {}

	[Message(HotfixOpcode.C2L_RemoveRelationship)]
	public partial class C2L_RemoveRelationship : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_RemoveRelationship)]
	public partial class L2C_RemoveRelationship : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_RefreshStranger)]
	public partial class C2L_RefreshStranger : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_RefreshStranger)]
	public partial class L2C_RefreshStranger : IActorLocationResponse {}

	[Message(HotfixOpcode.RelationshipApplyInfo)]
	public partial class RelationshipApplyInfo {}

	[Message(HotfixOpcode.C2L_GetRelationshipApplyList)]
	public partial class C2L_GetRelationshipApplyList : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_GetRelationshipApplyList)]
	public partial class L2C_GetRelationshipApplyList : IActorLocationResponse {}

	[Message(HotfixOpcode.L2C_NotifyRelationshipApplyState)]
	public partial class L2C_NotifyRelationshipApplyState : IActorMessage {}

	[Message(HotfixOpcode.C2L_RefreshRelationshipApply)]
	public partial class C2L_RefreshRelationshipApply : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_RefreshRelationshipApply)]
	public partial class L2C_RefreshRelationshipApply : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_AcceptRelationshipApply)]
	public partial class C2L_AcceptRelationshipApply : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_AcceptRelationshipApply)]
	public partial class L2C_AcceptRelationshipApply : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_RefuseRelationshipApply)]
	public partial class C2L_RefuseRelationshipApply : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_RefuseRelationshipApply)]
	public partial class L2C_RefuseRelationshipApply : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_QueryRelationshipByUids)]
	public partial class C2L_QueryRelationshipByUids : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_QueryRelationshipByUids)]
	public partial class L2C_QueryRelationshipByUids : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_QueryRelationshipByName)]
	public partial class C2L_QueryRelationshipByName : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_QueryRelationshipByName)]
	public partial class L2C_QueryRelationshipByName : IActorLocationResponse {}

	[Message(HotfixOpcode.C2M_GiveEmoticon)]
	public partial class C2M_GiveEmoticon : IActorLocationMessage {}

	[Message(HotfixOpcode.M2C_GiveEmoticon)]
	public partial class M2C_GiveEmoticon : IActorMessage {}

	[Message(HotfixOpcode.C2M_GiveAisatsu)]
	public partial class C2M_GiveAisatsu : IActorLocationMessage {}

	[Message(HotfixOpcode.M2C_GiveAisatsu)]
	public partial class M2C_GiveAisatsu : IActorMessage {}

	[Message(HotfixOpcode.AnnouncementInfo)]
	public partial class AnnouncementInfo {}

	[Message(HotfixOpcode.C2L_Announcement)]
	public partial class C2L_Announcement : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_Announcement)]
	public partial class L2C_Announcement : IActorLocationResponse {}

	[Message(HotfixOpcode.C2R_Login)]
	public partial class C2R_Login : IRequest {}

	[Message(HotfixOpcode.R2C_Login)]
	public partial class R2C_Login : IResponse {}

	[Message(HotfixOpcode.C2G_LoginGate)]
	public partial class C2G_LoginGate : IRequest {}

	[Message(HotfixOpcode.G2C_LoginGate)]
	public partial class G2C_LoginGate : IResponse {}

	[Message(HotfixOpcode.C2M_TestActorRequest)]
	public partial class C2M_TestActorRequest : IActorLocationRequest {}

	[Message(HotfixOpcode.M2C_TestActorResponse)]
	public partial class M2C_TestActorResponse : IActorLocationResponse {}

	[Message(HotfixOpcode.PlayerInfo)]
	public partial class PlayerInfo : IMessage {}

	[Message(HotfixOpcode.C2G_PlayerInfo)]
	public partial class C2G_PlayerInfo : IRequest {}

	[Message(HotfixOpcode.G2C_PlayerInfo)]
	public partial class G2C_PlayerInfo : IResponse {}

	[Message(HotfixOpcode.C2L_RoamingGetList)]
	public partial class C2L_RoamingGetList : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_RoamingGetList)]
	public partial class L2C_RoamingGetList : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_RoamingEnter)]
	public partial class C2L_RoamingEnter : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_RoamingEnter)]
	public partial class L2C_RoamingEnter : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamGetList)]
	public partial class C2L_TeamGetList : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamGetList)]
	public partial class L2C_TeamGetList : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamInviteAccept)]
	public partial class C2L_TeamInviteAccept : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamInviteAccept)]
	public partial class L2C_TeamInviteAccept : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamEnter)]
	public partial class C2L_TeamEnter : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamEnter)]
	public partial class L2C_TeamEnter : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamCreate)]
	public partial class C2L_TeamCreate : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamCreate)]
	public partial class L2C_TeamCreate : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamReservationJoin)]
	public partial class C2L_TeamReservationJoin : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamReservationJoin)]
	public partial class L2C_TeamReservationJoin : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamReservationCreate)]
	public partial class C2L_TeamReservationCreate : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamReservationCreate)]
	public partial class L2C_TeamReservationCreate : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamInviteGetList)]
	public partial class C2L_TeamInviteGetList : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamInviteGetList)]
	public partial class L2C_TeamInviteGetList : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamInviteRefuseAll)]
	public partial class C2L_TeamInviteRefuseAll : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamInviteRefuseAll)]
	public partial class L2C_TeamInviteRefuseAll : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamInviteRefuse)]
	public partial class C2L_TeamInviteRefuse : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamInviteRefuse)]
	public partial class L2C_TeamInviteRefuse : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamReservationCancel)]
	public partial class C2L_TeamReservationCancel : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamReservationCancel)]
	public partial class L2C_TeamReservationCancel : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamReservationGetList)]
	public partial class C2L_TeamReservationGetList : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamReservationGetList)]
	public partial class L2C_TeamReservationGetList : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_RoamingLeave)]
	public partial class C2L_RoamingLeave : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_RoamingLeave)]
	public partial class L2C_RoamingLeave : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_TeamLeave)]
	public partial class C2L_TeamLeave : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_TeamLeave)]
	public partial class L2C_TeamLeave : IActorLocationResponse {}

	[Message(HotfixOpcode.C2L_SyncPlayerState)]
	public partial class C2L_SyncPlayerState : IActorLocationRequest {}

	[Message(HotfixOpcode.L2C_SyncPlayerState)]
	public partial class L2C_SyncPlayerState : IActorLocationResponse {}

	[Message(HotfixOpcode.L2M_TeamModifyMember)]
	public partial class L2M_TeamModifyMember : IRequest {}

	[Message(HotfixOpcode.M2L_TeamModifyMember)]
	public partial class M2L_TeamModifyMember : IResponse {}

	[Message(HotfixOpcode.L2M_TeamLose)]
	public partial class L2M_TeamLose : IRequest {}

	[Message(HotfixOpcode.M2L_TeamLose)]
	public partial class M2L_TeamLose : IResponse {}

	[Message(HotfixOpcode.L2M_MapUnitCreate)]
	public partial class L2M_MapUnitCreate : IRequest {}

	[Message(HotfixOpcode.M2L_MapUnitCreate)]
	public partial class M2L_MapUnitCreate : IResponse {}

	[Message(HotfixOpcode.L2M_TeamCreate)]
	public partial class L2M_TeamCreate : IRequest {}

	[Message(HotfixOpcode.M2L_TeamCreate)]
	public partial class M2L_TeamCreate : IResponse {}

	[Message(HotfixOpcode.L2M_GetTeamData)]
	public partial class L2M_GetTeamData : IRequest {}

	[Message(HotfixOpcode.M2L_GetTeamData)]
	public partial class M2L_GetTeamData : IResponse {}

	[Message(HotfixOpcode.L2M_GetAllMapUnitInfoOnRoom)]
	public partial class L2M_GetAllMapUnitInfoOnRoom : IRequest {}

	[Message(HotfixOpcode.M2L_GetAllMapUnitInfoOnRoom)]
	public partial class M2L_GetAllMapUnitInfoOnRoom : IResponse {}

	[Message(HotfixOpcode.L2M_GetAllMapUnitGlobalInfoOnRoom)]
	public partial class L2M_GetAllMapUnitGlobalInfoOnRoom : IRequest {}

	[Message(HotfixOpcode.M2L_GetAllMapUnitGlobalInfoOnRoom)]
	public partial class M2L_GetAllMapUnitGlobalInfoOnRoom : IResponse {}

	[Message(HotfixOpcode.L2M_DestroyRoom)]
	public partial class L2M_DestroyRoom : IRequest {}

	[Message(HotfixOpcode.M2L_DestroyRoom)]
	public partial class M2L_DestroyRoom : IResponse {}

	[Message(HotfixOpcode.L2M_SetReservationMember)]
	public partial class L2M_SetReservationMember : IRequest {}

	[Message(HotfixOpcode.M2L_SetReservationMember)]
	public partial class M2L_SetReservationMember : IResponse {}

	[Message(HotfixOpcode.L2M_RunRoomOnTeam)]
	public partial class L2M_RunRoomOnTeam : IRequest {}

	[Message(HotfixOpcode.M2L_RunRoomOnTeam)]
	public partial class M2L_RunRoomOnTeam : IResponse {}

	[Message(HotfixOpcode.L2M_GetTeamMember)]
	public partial class L2M_GetTeamMember : IRequest {}

	[Message(HotfixOpcode.M2L_GetTeamMember)]
	public partial class M2L_GetTeamMember : IResponse {}

	[Message(HotfixOpcode.M2L_CreateInvite)]
	public partial class M2L_CreateInvite : IRequest {}

	[Message(HotfixOpcode.L2M_CreateInvite)]
	public partial class L2M_CreateInvite : IResponse {}

	[Message(HotfixOpcode.L2M_DestroyMapUnit)]
	public partial class L2M_DestroyMapUnit : IRequest {}

	[Message(HotfixOpcode.M2L_DestroyMapUnit)]
	public partial class M2L_DestroyMapUnit : IResponse {}

	[Message(HotfixOpcode.L2M_RoamingLeave)]
	public partial class L2M_RoamingLeave : IRequest {}

	[Message(HotfixOpcode.M2L_RoamingLeave)]
	public partial class M2L_RoamingLeave : IResponse {}

	[Message(HotfixOpcode.L2M_TeamLeave)]
	public partial class L2M_TeamLeave : IRequest {}

	[Message(HotfixOpcode.M2L_TeamLeave)]
	public partial class M2L_TeamLeave : IResponse {}

}
namespace ETHotfix
{
	public static partial class HotfixOpcode
	{
		 public const ushort PlayerBaseInfo = 10001;
		 public const ushort PlayerCharSetting = 10002;
		 public const ushort RideTotalInfo = 10003;
		 public const ushort PlayerRideTotalInfo = 10004;
		 public const ushort PlayerRideRoadInfo = 10005;
		 public const ushort G2C_MessageTip = 10006;
		 public const ushort G2C_UpdatePlayerRideTotalInfo = 10007;
		 public const ushort MapUnitInfo = 10008;
		 public const ushort MapUnitInfo_Global = 10009;
		 public const ushort G2M_MapUnitCreate = 10010;
		 public const ushort M2G_MapUnitCreate = 10011;
		 public const ushort M2C_MapUnitCreate = 10012;
		 public const ushort M2C_MapUnitUpdate = 10013;
		 public const ushort M2C_MapUnitDestroy = 10014;
		 public const ushort M2C_MapUnitCreateAndDestroy = 10015;
		 public const ushort C2M_MapUnitMove = 10016;
		 public const ushort C2M_LookAtTarget = 10017;
		 public const ushort M2C_LookAtTarget = 10018;
		 public const ushort M2C_MapUnitGlobalCreate = 10019;
		 public const ushort M2C_MapUnitGlobalDestroy = 10020;
		 public const ushort RoomInfo = 10021;
		 public const ushort RoamingRoomData = 10022;
		 public const ushort C2G_RoamingGetList = 10023;
		 public const ushort G2C_RoamingGetList = 10024;
		 public const ushort C2G_RoamingEnter = 10025;
		 public const ushort G2C_RoamingEnter = 10026;
		 public const ushort C2M_RoamingLeave = 10027;
		 public const ushort M2C_RoamingLeave = 10028;
		 public const ushort C2M_SaveRideRecord = 10029;
		 public const ushort M2C_SaveRideRecord = 10030;
		 public const ushort ReservationAllData = 10031;
		 public const ushort ReservationData = 10032;
		 public const ushort ReservationMemberData = 10033;
		 public const ushort C2G_TeamReservationGetList = 10034;
		 public const ushort G2C_TeamReservationGetList = 10035;
		 public const ushort C2G_TeamReservationCancel = 10036;
		 public const ushort G2C_TeamReservationCancel = 10037;
		 public const ushort C2G_TeamReservationJoin = 10038;
		 public const ushort G2C_TeamReservationJoin = 10039;
		 public const ushort G2C_TeamReservationAdd = 10040;
		 public const ushort G2C_TeamReservationRemove = 10041;
		 public const ushort C2G_TeamReservationCreate = 10042;
		 public const ushort G2C_TeamReservationCreate = 10043;
		 public const ushort InviteData = 10044;
		 public const ushort C2M_TeamInvite = 10045;
		 public const ushort M2C_TeamInvite = 10046;
		 public const ushort G2C_TeamInviteReceiver = 10047;
		 public const ushort G2C_TeamInviteTargerRefuse = 10048;
		 public const ushort C2G_TeamInviteGetList = 10049;
		 public const ushort G2C_TeamInviteGetList = 10050;
		 public const ushort C2G_TeamInviteAccept = 10051;
		 public const ushort G2C_TeamInviteAccept = 10052;
		 public const ushort C2G_TeamInviteRefuse = 10053;
		 public const ushort G2C_TeamInviteRefuse = 10054;
		 public const ushort C2G_TeamInviteRefuseAll = 10055;
		 public const ushort G2C_TeamInviteRefuseAll = 10056;
		 public const ushort TeamMemberData = 10057;
		 public const ushort TeamRoomData = 10058;
		 public const ushort C2G_TeamGetList = 10059;
		 public const ushort G2C_TeamGetList = 10060;
		 public const ushort C2G_TeamEnter = 10061;
		 public const ushort G2C_TeamEnter = 10062;
		 public const ushort C2G_TeamCreate = 10063;
		 public const ushort G2C_TeamCreate = 10064;
		 public const ushort M2C_TeamGoBattleProgressReceiver = 10065;
		 public const ushort M2C_TeamGoBattleProgressAllDone = 10066;
		 public const ushort M2C_TeamGoBattle = 10067;
		 public const ushort M2C_TeamGoLobby = 10068;
		 public const ushort M2C_TeamLose = 10069;
		 public const ushort M2C_TeamModifyData = 10070;
		 public const ushort M2C_TeamModifyMember = 10071;
		 public const ushort M2C_TeamReadyModify = 10072;
		 public const ushort C2M_TeamGoBattleProgress = 10073;
		 public const ushort C2M_TeamLeave = 10074;
		 public const ushort M2C_TeamLeave = 10075;
		 public const ushort C2M_TeamReady = 10076;
		 public const ushort M2C_TeamReady = 10077;
		 public const ushort C2M_TeamRun = 10078;
		 public const ushort M2C_TeamRun = 10079;
		 public const ushort C2M_TeamKick = 10080;
		 public const ushort M2C_TeamKick = 10081;
		 public const ushort C2M_TeamDeliveryLeader = 10082;
		 public const ushort M2C_TeamDeliveryLeader = 10083;
		 public const ushort C2M_TeamDisband = 10084;
		 public const ushort M2C_TeamDisband = 10085;
		 public const ushort BattleLeaderboardUnitInfo = 10086;
		 public const ushort M2C_BattleLeaderboard = 10087;
		 public const ushort G2C_ForceDisconnect = 10088;
		 public const ushort PlayerStateData = 10089;
		 public const ushort C2G_SyncPlayerState = 10090;
		 public const ushort G2C_SyncPlayerState = 10091;
		 public const ushort C2G_SyncPlayerState_Return = 10092;
		 public const ushort G2C_SyncPlayerState_Return = 10093;
		 public const ushort C2R_SignUp = 10094;
		 public const ushort R2C_SignUp = 10095;
		 public const ushort AuthenticationInfo = 10096;
		 public const ushort C2R_Authentication = 10097;
		 public const ushort R2C_Authentication = 10098;
		 public const ushort C2G_Logout = 10099;
		 public const ushort G2C_Logout = 10100;
		 public const ushort LinkInfo = 10101;
		 public const ushort C2L_Link = 10102;
		 public const ushort L2C_Link = 10103;
		 public const ushort C2L_UpdateUserProfile = 10104;
		 public const ushort L2C_UpdateUserProfile = 10105;
		 public const ushort C2L_UpdateUserEquip = 10106;
		 public const ushort L2C_UpdateUserEquip = 10107;
		 public const ushort C2L_UpdateUserLanguage = 10108;
		 public const ushort L2C_UpdateUserLanguage = 10109;
		 public const ushort EquipmentInfo = 10110;
		 public const ushort UserBagCapacity = 10111;
		 public const ushort C2L_GetUserAllEquipment = 10112;
		 public const ushort L2C_GetUserAllEquipment = 10113;
		 public const ushort L2C_OnEquipmentsCreated = 10114;
		 public const ushort L2C_OnEquipmentsDeleted = 10115;
		 public const ushort RelationshipSimpleInfo = 10116;
		 public const ushort C2L_GetRelationshipList = 10117;
		 public const ushort L2C_GetRelationshipList = 10118;
		 public const ushort C2L_AddRelationship = 10119;
		 public const ushort L2C_AddRelationship = 10120;
		 public const ushort L2C_NotifyRelationshipState = 10121;
		 public const ushort C2L_RemoveRelationship = 10122;
		 public const ushort L2C_RemoveRelationship = 10123;
		 public const ushort C2L_RefreshStranger = 10124;
		 public const ushort L2C_RefreshStranger = 10125;
		 public const ushort RelationshipApplyInfo = 10126;
		 public const ushort C2L_GetRelationshipApplyList = 10127;
		 public const ushort L2C_GetRelationshipApplyList = 10128;
		 public const ushort L2C_NotifyRelationshipApplyState = 10129;
		 public const ushort C2L_RefreshRelationshipApply = 10130;
		 public const ushort L2C_RefreshRelationshipApply = 10131;
		 public const ushort C2L_AcceptRelationshipApply = 10132;
		 public const ushort L2C_AcceptRelationshipApply = 10133;
		 public const ushort C2L_RefuseRelationshipApply = 10134;
		 public const ushort L2C_RefuseRelationshipApply = 10135;
		 public const ushort C2L_QueryRelationshipByUids = 10136;
		 public const ushort L2C_QueryRelationshipByUids = 10137;
		 public const ushort C2L_QueryRelationshipByName = 10138;
		 public const ushort L2C_QueryRelationshipByName = 10139;
		 public const ushort C2M_GiveEmoticon = 10140;
		 public const ushort M2C_GiveEmoticon = 10141;
		 public const ushort C2M_GiveAisatsu = 10142;
		 public const ushort M2C_GiveAisatsu = 10143;
		 public const ushort AnnouncementInfo = 10144;
		 public const ushort C2L_Announcement = 10145;
		 public const ushort L2C_Announcement = 10146;
		 public const ushort C2R_Login = 10147;
		 public const ushort R2C_Login = 10148;
		 public const ushort C2G_LoginGate = 10149;
		 public const ushort G2C_LoginGate = 10150;
		 public const ushort C2M_TestActorRequest = 10151;
		 public const ushort M2C_TestActorResponse = 10152;
		 public const ushort PlayerInfo = 10153;
		 public const ushort C2G_PlayerInfo = 10154;
		 public const ushort G2C_PlayerInfo = 10155;
		 public const ushort C2L_RoamingGetList = 10156;
		 public const ushort L2C_RoamingGetList = 10157;
		 public const ushort C2L_RoamingEnter = 10158;
		 public const ushort L2C_RoamingEnter = 10159;
		 public const ushort C2L_TeamGetList = 10160;
		 public const ushort L2C_TeamGetList = 10161;
		 public const ushort C2L_TeamInviteAccept = 10162;
		 public const ushort L2C_TeamInviteAccept = 10163;
		 public const ushort C2L_TeamEnter = 10164;
		 public const ushort L2C_TeamEnter = 10165;
		 public const ushort C2L_TeamCreate = 10166;
		 public const ushort L2C_TeamCreate = 10167;
		 public const ushort C2L_TeamReservationJoin = 10168;
		 public const ushort L2C_TeamReservationJoin = 10169;
		 public const ushort C2L_TeamReservationCreate = 10170;
		 public const ushort L2C_TeamReservationCreate = 10171;
		 public const ushort C2L_TeamInviteGetList = 10172;
		 public const ushort L2C_TeamInviteGetList = 10173;
		 public const ushort C2L_TeamInviteRefuseAll = 10174;
		 public const ushort L2C_TeamInviteRefuseAll = 10175;
		 public const ushort C2L_TeamInviteRefuse = 10176;
		 public const ushort L2C_TeamInviteRefuse = 10177;
		 public const ushort C2L_TeamReservationCancel = 10178;
		 public const ushort L2C_TeamReservationCancel = 10179;
		 public const ushort C2L_TeamReservationGetList = 10180;
		 public const ushort L2C_TeamReservationGetList = 10181;
		 public const ushort C2L_RoamingLeave = 10182;
		 public const ushort L2C_RoamingLeave = 10183;
		 public const ushort C2L_TeamLeave = 10184;
		 public const ushort L2C_TeamLeave = 10185;
		 public const ushort C2L_SyncPlayerState = 10186;
		 public const ushort L2C_SyncPlayerState = 10187;
		 public const ushort L2M_TeamModifyMember = 10188;
		 public const ushort M2L_TeamModifyMember = 10189;
		 public const ushort L2M_TeamLose = 10190;
		 public const ushort M2L_TeamLose = 10191;
		 public const ushort L2M_MapUnitCreate = 10192;
		 public const ushort M2L_MapUnitCreate = 10193;
		 public const ushort L2M_TeamCreate = 10194;
		 public const ushort M2L_TeamCreate = 10195;
		 public const ushort L2M_GetTeamData = 10196;
		 public const ushort M2L_GetTeamData = 10197;
		 public const ushort L2M_GetAllMapUnitInfoOnRoom = 10198;
		 public const ushort M2L_GetAllMapUnitInfoOnRoom = 10199;
		 public const ushort L2M_GetAllMapUnitGlobalInfoOnRoom = 10200;
		 public const ushort M2L_GetAllMapUnitGlobalInfoOnRoom = 10201;
		 public const ushort L2M_DestroyRoom = 10202;
		 public const ushort M2L_DestroyRoom = 10203;
		 public const ushort L2M_SetReservationMember = 10204;
		 public const ushort M2L_SetReservationMember = 10205;
		 public const ushort L2M_RunRoomOnTeam = 10206;
		 public const ushort M2L_RunRoomOnTeam = 10207;
		 public const ushort L2M_GetTeamMember = 10208;
		 public const ushort M2L_GetTeamMember = 10209;
		 public const ushort M2L_CreateInvite = 10210;
		 public const ushort L2M_CreateInvite = 10211;
		 public const ushort L2M_DestroyMapUnit = 10212;
		 public const ushort M2L_DestroyMapUnit = 10213;
		 public const ushort L2M_RoamingLeave = 10214;
		 public const ushort M2L_RoamingLeave = 10215;
		 public const ushort L2M_TeamLeave = 10216;
		 public const ushort M2L_TeamLeave = 10217;
	}
}
