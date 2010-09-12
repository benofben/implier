using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implier.Graph
{

    internal class ContractSide : DisposableBaseObject
    {
        #region Private

        private IProposal parent = null;
        private bool used = false;

        #endregion

        #region Connections

        private readonly List<SideConnection> internalConnections = new List<SideConnection>();
        private readonly List<SideConnection> externalConnections = new List<SideConnection>();
        private readonly List<SideConnection> connections = new List<SideConnection>();

        internal void DNUAddConnection(SideConnection connection)
        {
            connections.Add(connection);
            if (connection.DNUInsideContractConnection)
                internalConnections.Add(connection);
            else
                externalConnections.Add(connection);
        }

        internal void DNURemoveConnection(SideConnection connection)
        {
            connections.Remove(connection);
            if (connection.DNUInsideContractConnection)
                internalConnections.Remove(connection);
            else
                externalConnections.Remove(connection);
        }

        internal IEnumerable<SideConnection> ExternalConnections
        {
            get { return externalConnections.AsEnumerable(); }
        }

        internal IEnumerable<SideConnection> InternalConnections
        {
            get { return internalConnections.AsEnumerable(); }
        }
        internal IEnumerable<SideConnection> Connections
        {
            get { return connections.AsEnumerable(); }
        }

        #endregion

        #region Override

        protected override void DoDispose()
        {
            SideController.UnRegister(this);
            while (connections.Count > 0)
                connections[0].Dispose();
            Parent = null;
        }

        public override string ToString()
        {
            return SideKey.ToString();
        }
        #endregion

        #region Properties

        private SideController SideController { get; set; }
        internal SideKey SideKey{ get; private set; }
        internal bool Used 
        {
            get
            {
                return used;
            }
            set
            {
                if (used == value)
                    return;
                used = value;
                if (used)
                {
                    Parent.Used++;
                    SideKey.Used++;
                }
                else
                {
                    Parent.Used--;
                    SideKey.Used--;
                }
            }
        }
        internal IProposal Parent
        {
            get
            {
                return parent;
            }
            private set
            {
                if (parent != null)
                    parent.DNURemoveSide(this);
                parent = value;
                if (parent != null)
                    parent.DNUAddSide(this);
            }
        }

        //internal bool CanHaveInput
        //{
        //    get 
        //    {
        //        return Parent.CanBeBought && Parent.IsEveneSide(this) || Parent.CanBeSold && !Parent.IsEveneSide(this);
        //    }
        //}

        //internal bool CanHaveMinusInput
        //{
        //    get
        //    {
        //        return Parent.CanBeSold && !Parent.IsEveneSide(this) || Parent.CanBeBought && Parent.IsEveneSide(this);
        //    }
        //}

        #endregion

        #region Constructor
        internal ContractSide(DateTime date, Proposal parent, SideController sideController)
        {
            SideController = sideController;
            Parent = parent;
            EntryType action = parent.IsEveneSide(this) ? parent.Action : Graph.SideKey.OppositeType(parent.Action);
            SideKey = Graph.SideKey.Get(date, action);
            SideController.Register(this);
        }

        #endregion
    }
}
