using System.Threading.Tasks;

using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.FriendReference;

namespace Backgammon.Client.Controllers.Friends
{
    internal class FriendController
    {
        private readonly FriendClient _friendClient;

        public FriendController()
        {
            _friendClient = new FriendClient();
        }

        public async Task<Friend[]> GetFriendsAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;
            Friend[] friends = await _friendClient.GetFriendsAsync(sessionId);

            return friends;
        }

        public async Task<FriendRequestPending[]> GetFriendRequestsAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;
            FriendRequestPending[] friendRequests = await _friendClient.GetFriendRequestsAsync(sessionId);
            
            return friendRequests;
        }

        public async Task AcceptFriendRequestAsync(int requestId)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;
            await _friendClient.AcceptFriendRequestAsync(sessionId, requestId);
        }

        public async Task RejectFriendRequestAsync(int requestId)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;
            await _friendClient.RejectFriendRequestAsync(sessionId, requestId);
        }

        public async Task SendFriendRequestAsync(string identifier, bool byEmail)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;
            await _friendClient.SendFriendRequestAsync(sessionId, identifier, byEmail);
        }

        public async Task RemoveFriendAsync(int friendAccountId)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            await _friendClient.RemoveFriendAsync(sessionId, friendAccountId);
        }
    }
}
