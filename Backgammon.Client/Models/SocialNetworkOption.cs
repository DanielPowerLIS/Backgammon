using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Client.Models
{
    public sealed class SocialNetworkOption
    {
        public int Id { get; }
        public string Name { get; }
        public bool HasLink { get; }

        public bool IsAvailable => !HasLink;

        public SocialNetworkOption(int id, string name, bool hasLink)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "A social network name is required.",
                    nameof(name));
            }

            Id = id;
            Name = name;
            HasLink = hasLink;
        }
    }
}
