using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implier.Graph
{
    internal class SideConnection : DisposableBaseObject
    {
        #region Private

        private ContractSide minus = null;
        private ContractSide plus = null;

        public SideConnection()
        {
            DNUInsideContractConnection = false;
        }

        #endregion

        #region Methods

        public ContractSide GetAnother(ContractSide side)
        {
            if (side == Minus)
                return Plus;
            else if (side == Plus)
                return Minus;
            else
                throw new InvalidArgumentExcetipn("Provided <Side> does not relate to the <Connection>");
        }

        internal SideConnection(ContractSide minus,ContractSide plus, bool isInternalConnection)
        {
            DNUInsideContractConnection = isInternalConnection;
            Minus = minus;
            Plus = plus;
        }

        #endregion

        #region Properties

        public bool DNUInsideContractConnection { get; private set; }

        private ContractSide Minus
        {
            get { return minus; }
            set
            {
                if (minus != null)
                    minus.DNURemoveConnection(this);
                minus = value;
                if (minus != null)
                    minus.DNUAddConnection(this);
            }
        }

        private ContractSide Plus
        {
            get { return plus; }
            set
            {
                if (plus != null)
                    plus.DNURemoveConnection(this);
                plus = value;
                if (plus != null)
                    plus.DNUAddConnection(this);
            }
        }

        protected override void DoDispose()
        {
            Plus = null;
            Minus = null;
        }


        #endregion
    }
}