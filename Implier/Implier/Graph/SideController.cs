using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implier.Graph
{
    internal class SideKey
    {
        #region Private
        static Dictionary<DateTime, SideKey> bids = new Dictionary<DateTime, SideKey>();

        private SideKey(DateTime dateTime, EntryType type)
        {
            DateTime = dateTime;
            Type = type;
        }

        #endregion

        #region Properties
        public DateTime DateTime { get; private set; }
        public EntryType Type { get; private set; }
        public SideKey Opposite { get; private set; }
        public int Used { get; set; }
        #endregion

        #region Methods
        public override string ToString()
        {
            return ((Type == EntryType.Bid) ? "-" : "+") + DateTime.ToString("MMMyy");
        }
        #endregion

        #region Public Static
        public static SideKey Get(DateTime dateTime, EntryType type)
        {
            SideKey result = null;
            if (bids.TryGetValue(dateTime,out result))
                return type == EntryType.Bid ? result : result.Opposite;

            SideKey bid = new SideKey(dateTime,EntryType.Bid);
            SideKey offer = new SideKey(dateTime, EntryType.Offer);
            bid.Opposite = offer;
            offer.Opposite = bid;

            bids.Add(dateTime, bid);

            return type == EntryType.Bid ? bid : offer;

        }
        public static EntryType OppositeType(EntryType type)
        {
            return type == EntryType.Bid ? EntryType.Offer : EntryType.Bid;
        }

        public static void SetUsed(int value)
        {
            foreach (SideKey key in bids.Values)
            {
                key.Used = value;
            }
        }

        public bool IsNeighbourTo(SideKey sideKey)
        {
            if (sideKey.DateTime.Year == DateTime.Year)
                return Math.Abs(sideKey.DateTime.Month - DateTime.Month) < 2;
            if (sideKey.DateTime.Year - DateTime.Year == 1)
                return sideKey.DateTime.Month == 1 && DateTime.Month == 12;
            if (DateTime.Year - sideKey.DateTime.Year == 1)
                return DateTime.Month == 1 && sideKey.DateTime.Month == 12;
            return false;
        }

        #endregion
    }

    internal class SideController : DisposableBaseObject
    {
        #region Fields
        SafeDictionary<SideKey, List<ContractSide>> sides = new SafeDictionary<SideKey, List<ContractSide>>();
        List<Proposal> proposals = new List<Proposal>();
        List<Proposal> changed = new List<Proposal>();

        #endregion

        internal bool Changed 
        {
            get { return changed.Count > 0; }
            set
            {
                if (value == false)
                    changed.Clear();
            }
        }


        public void Update(Proposal proposal)
        {
            lock (LockObject)
            {
                changed.Add(proposal);
            }
        }

        public void SetProposalsUsed(int value)
        {
            lock (LockObject)
            {
                foreach (Proposal proposal in proposals)
                {
                    proposal.Used = value;
                }
            }
        }

        public void SetSidesUsed(bool value)
        {
            lock (LockObject)
            {
                foreach (List<ContractSide> list in sides.Values)
                    foreach (ContractSide side in list)
                        side.Used = value;
            }
        }

        public void Register(Proposal proposal)
        {
            lock (LockObject)
            {
                proposals.Add(proposal);
                changed.Add(proposal);
            }
        }
        public void UnRegister(Proposal proposal)
        {
            lock (LockObject)
            {
                proposals.Remove(proposal);
            }
        }

        public void Register(ContractSide side)
        {
            lock (LockObject)
            {
                sides[side.SideKey].Add(side);
            }
        }
        public void UnRegister(ContractSide side)
        {
            lock (LockObject)
            {
                sides[side.SideKey].Remove(side);
            }
        }

        public void EnsureInitialized()
        {
            lock (LockObject)
            {
                if (Changed)
                {
                    changed = changed.Distinct().ToList();
                    changed = changed.Where(proposal => !proposal.IsDisposed).ToList();
                    foreach (IProposal proposal in changed)
                    {
                        //Clear current connections for contracts
                        List<SideConnection> toDelete = proposal.GetConnections(connection => true);
                        foreach (SideConnection connection in toDelete)
                            connection.Dispose();

                        //<TODO:1 Update internal connections>
                        for (int i = 0; i < proposal.SideCount - 1; i++)
                        {
                            new SideConnection(proposal.GetSide(i), proposal.GetSide(i + 1), true);
                        }

                        //<TODO:2 Update external connections>
                        for (int i = 0; i < proposal.SideCount; i++)
                        {
                            ContractSide side = proposal.GetSide(i);
                            List<ContractSide> opposites = sides[side.SideKey.Opposite];
                            foreach (ContractSide potentialSide in opposites.Except(proposal.Sides))
                            {
                                new SideConnection(side, potentialSide, false);
                            }
                        }

                    }

                }
                Changed = false;
            }
        }

        internal List<Proposal> SelectProposals(Func<Proposal, bool> func)
        {
            lock (LockObject)
            {
                return proposals.Where(func).ToList();
            }
        }

        internal List<ContractSide> SelectSides(Func<ContractSide,bool> func)
        {
            lock (LockObject)
            {
                List<ContractSide> result = new List<ContractSide>();
                foreach (List<ContractSide> sideList in sides.Values)
                {
                    result.AddRange(sideList.Where(func));
                }
                return result;
            }
        }

        protected override void DoDispose()
        {
            while (proposals.Count>0)
                proposals[0].Dispose();
            proposals.Clear();
            sides.Clear();
            changed.Clear();
        }
    }
}

