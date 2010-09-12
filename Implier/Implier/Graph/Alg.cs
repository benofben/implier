using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implier.Graph
{
    internal class Trade
    {
        #region Fields
        private double cost = double.MinValue;
        private double costWODeal = double.MinValue;
        private static double dealCost = Constants.DealCost;
        #endregion

        #region Properties

        public static double DealCost { get { return dealCost; } set { dealCost = value; } }
        public IEnumerable<IProposal> Proposals { get; private set; }
        public IEnumerable<ContractSide> Path { get; private set; }
        public double Quantity { get; private set; }

        public double Cost
        {
            get
            {
                if (cost == double.MinValue)
                    cost = (Proposals.Sum(proposal => proposal.Price) - Path.Count()*DealCost)*Quantity;
                return cost;
            }
        }

        public double CostWODeal
        {
            get
            {
                if (costWODeal == double.MinValue)
                    costWODeal = Proposals.Sum(proposal => proposal.Price) * Quantity;
                return costWODeal;
            }
        }

        #endregion

        public Trade(IEnumerable<ContractSide> path)
        {
            Path = path;
            IEnumerable<IProposal> proposals = path.Select(proposal => proposal.Parent);
            Proposals = proposals.Distinct().ToList();
            Quantity = proposals.Min(proposal => proposal.Quantity);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (IProposal proposal in Proposals)
            {
                sb.Append(proposal.ToString());
                sb.Append(",");
            }
            sb[sb.Length - 1] = ' ';

            sb.Append(": " + Cost);
            sb.Append("\n");
            return sb.ToString();
        }
    }


    internal class PreTrade
    {
        private IEnumerable<ContractSide> path = null;
        public PreTrade()
        {
            Cost = 0;
            path = null;
            Exists = true;
        }

        internal LinkedList<ContractSide> Path 
        {   
            get 
            {
                if (path == null)
                    return null;
                LinkedList<ContractSide> result = new LinkedList<ContractSide>();
                foreach (ContractSide side in path)
                {
                    result.AddLast(side);
                }
                return result;
            }
            set { path = value.ToArray();}
        }

        internal double Cost { get; set; }
        internal bool Exists { get; set; }
        internal bool Empty
        {
            get 
            { 
                return path ==null || path.Count() == 0; 
            }
        }
    }

    internal class Alg
    {
        SideController SideController{ get; set;}
        internal Alg(SideController sideController)
        {
            SideController = sideController;
        }

        DualKeySafeDictionary<SideKey,SideKey,PreTrade> pretrades = new DualKeySafeDictionary<SideKey, SideKey, PreTrade>();

        /// <summary>
        /// function searches the best path between current and to,
        /// in case of path found it adds its cost to currentCost      
        /// </summary>
        /// <param name="currentCost">Reference param, function adds cost of found path to the param</param>
        /// <param name="current">a side to start path from</param>
        /// <param name="to">a destination side</param>
        /// <returns>null if no path exists, empty list if 'current' == 'to'</returns>
        LinkedList<ContractSide> F11BestPathRecursive(ref double currentCost, ContractSide current, ContractSide to)
        {
            if (current == to)
                return new LinkedList<ContractSide>();

            //check if we already know the best path between 'current' and 'to'
            PreTrade preTrade = pretrades[current.SideKey][to.SideKey];


            if (!preTrade.Exists)//we already know that there is no any path between 'current' and 'to'
                return null;

            if (!preTrade.Empty)
            {
                //yes the path exists and we already know the best, we will just use cached values
                currentCost += preTrade.Cost;
                return preTrade.Path;
            }

            //Below we perform search of the next steps of path
            LinkedList<ContractSide> bestPath = null;
            double bestCost = double.MinValue;

            //iterating ALL possible connections
            foreach (SideConnection connection in current.ExternalConnections)
            {
                //this is a next side
                ContractSide next = connection.GetAnother(current);

                //we go into the next side if it was not used in THIS path before
                //AND (either it is spread or our destination side)
                if (next.SideKey.Used==0 && (next == to || next.Parent.SideCount>1))
                {
                    //we add cost of contract only in case we did not go through it for the current path
                    //we could already go through one of side of the contract
                    double nextCost = next.Parent.Used == 0 ? 0 : next.Parent.Price;

                    //we put that the side is used, to prevent usage of them for the next path steps
                    next.Used = true;
                    ContractSide nextStored = next;

                    //in case if it is spread, and not next side is not the destination, 
                    //then we perform step through the spread automatically and update 'next' value
                    if (next.Parent.SideCount>1 && next!=to)
                    {
                        nextStored = next;
                        SideConnection con = next.InternalConnections.First();
                        next = con.GetAnother(next);
                        next.Used = true;
                    }

                    //now we performed single step and can call the same function recursively 
                    //in order to searach the rest of the path
                    LinkedList<ContractSide> curPath = F11BestPathRecursive(ref nextCost, next, to);


                    //ONLY now when we have FULL path constructed between 'next' and 'to'...
                    //we can compare full paths and full costs
                    if (curPath!=null && nextCost > bestCost)
                    {
                        bestCost = nextCost;
                        bestPath = curPath;
                        bestPath.AddFirst(next);
                        if (next!=nextStored) 
                            bestPath.AddFirst(nextStored);
                    }

                    //now we can release the side, so it could be used in other trades 
                    next.Used = false;
                    nextStored.Used = false;
                }
            }

            if (bestCost == double.MinValue)
            {
                preTrade.Exists = false;
                return null;
            }
            preTrade.Cost = bestCost;
            preTrade.Path = bestPath;
            currentCost += bestCost;
            return bestPath;
            //current.
           // .SideKey.Opposite
        }

        IEnumerable<ContractSide> F1BestPath(ContractSide from, ContractSide to)
        {
            from.Used = true;
            double price = from.Parent.Price;
            LinkedList<ContractSide> curPath = F11BestPathRecursive(ref price, from, to);
            from.Used = false;
            if (curPath!=null)
            {
                curPath.AddFirst(from);
            }
            return curPath;
        }

        List<ContractSide> FindAllSimplePairs()
        {

            //simple futures
            List<ContractSide> bidSides = SideController.SelectSides(side => side.Parent.SideCount == 1&&side.SideKey.Type == EntryType.Bid);
            List<ContractSide> offerSides = SideController.SelectSides(side => side.Parent.SideCount == 1 && side.SideKey.Type == EntryType.Offer);
            List<ContractSide> list = new List<ContractSide>(bidSides.Count*offerSides.Count);


            foreach (ContractSide bid in bidSides)
            {
                foreach (ContractSide offer in offerSides)
                {
                    if (bid.SideKey.DateTime!=offer.SideKey.DateTime)
                    {
                        list.Add(bid);
                        list.Add(offer);
                    }
                }
            }

            //spreads
            List<Proposal> spreads =
                SideController.SelectProposals(
                    proposal =>
                    proposal.SideCount == 2 &&
                    !proposal.GetSide(0).SideKey.IsNeighbourTo(proposal.GetSide(1).SideKey));
            list.Capacity = list.Count + spreads.Count*2;

            foreach (IProposal proposal in spreads)
            {
                list.Add(proposal.GetSide(0));
                list.Add(proposal.GetSide(1));
            }

            return list;
        }


        public List<Trade> Run()
        {
            SideController.EnsureInitialized();
            SideController.SetSidesUsed(false);
            SideController.SetProposalsUsed(0);
            SideKey.SetUsed(0);
            pretrades.Clear();

            List<ContractSide> pairs = FindAllSimplePairs();
            List<Trade> result = new List<Trade>(pairs.Count / 2);
            for (int i = 0; i < pairs.Count; i += 2)
            {
                IEnumerable<ContractSide> path = F1BestPath(pairs[i], pairs[i + 1]);

                if (path!=null)
                {
                    result.Add(new Trade(path));
                }
            }
            return result;
        }
    }
}
