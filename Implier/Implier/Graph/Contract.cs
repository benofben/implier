using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.VisualStyles;
using Implier.SpreadMatrix;
using QuickFix42;

namespace Implier.Graph
{
    [Flags]
    internal enum EntryType
    {
        Bid,
        Offer
    }
    internal interface IProposal
    {
        #region Private

        void DNUAddSide(ContractSide side);
        void DNURemoveSide(ContractSide side);

        #endregion

        #region Sides
        int SideCount { get; }
        ContractSide GetSide(int i);

        bool IsEveneSide(ContractSide side);

        IEnumerable<ContractSide> Sides{ get;}

        #endregion

        #region Properties

        double Price { get; set; }
        double Quantity { get; set; }
        EntryType Action { get; set; }
        int Used { get; set; }
        #endregion

        #region Methods

        List<SideConnection> GetConnections(Func<SideConnection, bool> func);
        #endregion
    }


    internal class Proposal : MDEntryGroup, IProposal
    {
        class SideComparer: Comparer<ContractSide>
        {
            public override int Compare(ContractSide x, ContractSide y)
            {
                return x.SideKey.DateTime.CompareTo(y.SideKey.DateTime);
            }
        }


        #region Fields
        List<ContractSide> sides = new List<ContractSide>();
        private bool sidesChanged = false;
        #endregion

        #region Methods

        public List<SideConnection> GetConnections(Func<SideConnection,bool> func)
        {
            List<SideConnection> connections = new List<SideConnection>();

            foreach (ContractSide side in sides)
                connections.AddRange(side.Connections.Where(func));

            return connections;
        }

        void EnsureValidated()
        {
            if (sidesChanged)
            {
                //sides.Sort(new SideComparer());
            }
            sidesChanged = false;
        }

        #endregion

        internal Proposal(MarketDataSnapshotFullRefresh.NoMDEntries group, SecurityEntry securityOwner, MDDatePair datePair, SideController sideController)
            : base(group, securityOwner)
        {
            SideController = sideController;
            switch (base.MDEntryType)
            {
                case QuickFix.MDEntryType.BID:
                    Action = !securityOwner.IsInverted ? Graph.EntryType.Bid : Graph.EntryType.Offer;
                    Price = base.MDEntryPx;
                    break;

                case QuickFix.MDEntryType.OFFER:
                    Action = !securityOwner.IsInverted ? Graph.EntryType.Offer : Graph.EntryType.Bid;
                    Price = -base.MDEntryPx;
                    break;

                default:
                    throw new Exception("Undefined entry type.");
            }

            //Price *= (!securityOwner.IsInverted ? 1 : -1);
            Quantity = base.MDEntrySize;

            new ContractSide(datePair.Date1, this, SideController);

            if (datePair.Date2 != datePair.Date1)
                new ContractSide(datePair.Date2, this, SideController);

            SideController.Register(this);
        }

        #region Override

        public void DNUAddSide(ContractSide side)
        {
            sides.Add(side);
            sidesChanged = true;
        }

        public void DNURemoveSide(ContractSide side)
        {
            sides.Remove(side);
            sidesChanged = true;
        }

        public int SideCount
        {
            get
            {
                EnsureValidated();
                return sides.Count;
            }
        }

        public ContractSide GetSide(int i)
        {
            EnsureValidated();
            return sides[i];
        }

        public bool IsEveneSide(ContractSide side)
        {
            EnsureValidated();
            return (sides.IndexOf(side) % 2) == 0;
        }

        public IEnumerable<ContractSide> Sides
        {
            get { return sides.AsEnumerable(); }
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            SideController.UnRegister(this);
            while (SideCount>0)
                GetSide(0).Dispose();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            //foreach (ContractSide side in Sides)
            for (int i = 0; i < SideCount;i++ )
            {
                sb.Append(GetSide(i).SideKey.ToString());
                if (i != SideCount - 1)
                    sb.Append("/");
            }
            return sb.ToString();
        }

        #endregion

        #region Properties
        public int Used { get; set; }

        public double Price { get; set; }

        public double Quantity { get; set; }
        public EntryType Action { get; set; }

        private SideController SideController
        {
            get; set;
        }

        #endregion

    }
}
