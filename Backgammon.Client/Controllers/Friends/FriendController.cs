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
    }
}