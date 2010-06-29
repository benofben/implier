using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implier.Graph
{


    public class Trade
    {
        #region Fields
        private double cost = double.MinValue;
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

        #endregion

        public Trade(IEnumerable<ContractSide> path)
        {
            Path = path;
            IEnumerable<IProposal> proposals = path.Select(proposal => proposal.Parent);
            Proposals = proposals.Distinct();
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

    public class Alg
    {
        SideController SideController{ get; set;}
        internal Alg(SideController sideController)
        {
            SideController = sideController;
        }

        DualKeySafeDictionary<SideKey,SideKey,PreTrade> pretrades = new DualKeySafeDictionary<SideKey, SideKey, PreTrade>();

        LinkedList<ContractSide> F11BestPathRecursive(ref double currentCost, ContractSide current, ContractSide to)
        {
            if (current == to)
                return new LinkedList<ContractSide>();

            PreTrade preTrade = pretrades[current.SideKey][to.SideKey];
            if (!preTrade.Exists)
                return null;
            if (!preTrade.Empty)
            {
                currentCost += preTrade.Cost;
                return preTrade.Path;
            }

            //ContractSide current = currentPath.Last.Value;
            LinkedList<ContractSide> bestPath = null;
            double bestCost = double.MinValue;
            foreach (SideConnection connection in current.ExternalConnections)
            {
                ContractSide next = connection.GetAnother(current);
                if (next.SideKey.Used==0 && (next == to || next.Parent.SideCount>1))
                {
                    double nextCost = next.Parent.Used == 0 ? 0 : next.Parent.Price;
                    next.Used = true;
                    ContractSide nextStored = next;
                    if (next.Parent.SideCount>1 && next!=to)
                    {
                        nextStored = next;
                        SideConnection con = next.InternalConnections.First();
                        next = con.GetAnother(next);
                        next.Used = true;
                    }
                    LinkedList<ContractSide> curPath = F11BestPathRecursive(ref nextCost, next, to);
                    if (curPath!=null && nextCost > bestCost)
                    {
                        bestCost = nextCost;
                        bestPath = curPath;
                        bestPath.AddFirst(next);
                        if (next!=nextStored) 
                            bestPath.AddFirst(nextStored);
                    }
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
