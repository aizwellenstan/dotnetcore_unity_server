using System;
using System.Net;
using ETModel;
using MongoDB.Bson;

namespace ETHotfix
{
    [MessageHandler(AppType.Lobby)]
    public class M2L_CreateInviteHandler : AMRpcHandler<M2L_CreateInvite, L2M_CreateInvite>
    {
        protected override void Run(Session session, M2L_CreateInvite message, Action<L2M_CreateInvite> reply)
        {
            RunAsync(session, message, reply);
        }

        private async void RunAsync(Session session, M2L_CreateInvite message, Action<L2M_CreateInvite> reply)
        {
            L2M_CreateInvite response = new L2M_CreateInvite();
            try
            {
                var invite = await Game.Scene.GetComponent<InviteComponent>().CreateInvite(message.InviteInfo);
                response.InviteId = invite.Id;
                response.Json = invite.ToJson();
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
