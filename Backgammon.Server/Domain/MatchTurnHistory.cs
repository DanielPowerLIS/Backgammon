using System;
using System.Collections.Generic;

namespace Backgammon.Server.Domain
{
    internal sealed class MatchTurnHistory
    {
        private const int EmptyHistoryCount = 0;

        private readonly Stack<TurnSnapshot> _snapshots;

        public bool HasSnapshots
        {
            get
            {
                bool hasSnapshots = _snapshots.Count > EmptyHistoryCount;

                return hasSnapshots;
            }
        }

        public MatchTurnHistory()
        {
            _snapshots = new Stack<TurnSnapshot>();
        }

        public void Save(TurnSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            _snapshots.Push(snapshot);
        }

        public TurnSnapshot TakeLastSnapshot()
        {
            if (!HasSnapshots)
            {
                throw new InvalidOperationException("The turn history has no saved movements.");
            }

            TurnSnapshot snapshot = _snapshots.Pop();

            return snapshot;
        }

        public TurnSnapshot GetLastSnapshot()
        {
            if (!HasSnapshots)
            {
                throw new InvalidOperationException("The turn history has no saved movements.");
            }

            TurnSnapshot snapshot = _snapshots.Peek();

            return snapshot;
        }

        public void Clear()
        {
            _snapshots.Clear();
        }
    }
}