using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Implier.CommonControls.Windows;
using Implier.FIXApplication;
using Implier.Graph;
using Implier.PureArbitrage;
using QuickFix;
using QuickFix42;

//using SecurityEntry = QuickFix42.MarketDataSnapshotFullRefresh;
//using MDDictionary = System.Collections.Generic.Dictionary<System.DateTime, System.Collections.Generic.Dictionary<System.DateTime, QuickFix42.MarketDataSnapshotFullRefresh>>;
//using MDGroupDictionary = System.Collections.Generic.Dictionary<string, object>;

namespace Implier.SpreadMatrix
{
    internal class SpreadMatrixData: SupportableObject
    {
        #region Fields
        DualKeyDictionary<DateTime, DateTime, SecurityEntry> spreadMatrixEntries = new DualKeyDictionary<DateTime, DateTime, SecurityEntry>();
        Dictionary<string, SecurityEntry> securityEntries = new Dictionary<string, SecurityEntry>();
        DateTime minYearMonth = new DateTime();
        DateTime maxYearMonth = new DateTime();
        #endregion

        #region Properties
        public string Exchange { get; private set; }
        public string Symbol { get; private set; }

        internal DateTime MinYearMonth
        {
            get { lock (LockObject) { return minYearMonth; } }
        }

        internal DateTime MaxYearMonth
        {
            get { lock (LockObject) { return maxYearMonth; } }
        }

        internal SideController SideController
        {
            get; private set;
        }

        /// <summary>
        /// Do not forget to lock SpreadMatrixData.LockObject while operating with values
        /// </summary>
        internal IEnumerable<SecurityEntry> Values
        {
            get { lock (LockObject) { return spreadMatrixEntries.Values; } }
        }
        #endregion

        #region Delegates and Events
        //public delegate void AddEntryHandler(object sender, SecurityEntry entry);
        //public event AddEntryHandler OnEntryAdded;
        #endregion

        #region Constructor
        internal SpreadMatrixData(string exchange, string symbol)
        {
            Exchange = exchange;
            Symbol = symbol;
            SideController = new SideController();
        }
        #endregion

        #region Methods
        void AddEntryData(SecurityEntry entry)
        {
            if (entry.SubContractCount > 2)
                return;

            securityEntries.Add(entry.SecurityID, entry);

            /*
            SecurityEntry securityEntry = spreadMatrixEntries.GetValue(datePair.Date1, datePair.Date2);
            if (securityEntry != null)
                securityEntry.Dispose();
            */

            if (entry.HasFutureContract())
            {
                MDDatePair datePair = entry.GetDatePair();

                spreadMatrixEntries.SetValue(datePair.Date1, datePair.Date2, entry);

                SetMinYearMonth(datePair);
                SetMaxYearMonth(datePair);
            }
        }

        public void Add(SecurityEntry entry)
        {
            lock (LockObject)
            {
                AddEntryData(entry);
                RaizeChanged(entry);
            }
        }

        public void Update(MarketDataSnapshotFullRefresh entry)
        {
            lock (LockObject)
            {
                string securityID = entry.getSecurityID().getValue();

                if (securityEntries.ContainsKey(securityID))
                {
                    SecurityEntry securityEntry = securityEntries[securityID];

                    if (securityEntry.HasFutureContract())
                    {
                        securityEntry.UpdateMDEntry(entry, SideController);

                        RaizeChanged(securityEntry);
                    }
                }
            }
        }

        public SecurityEntry Get(MDDatePair datePair)
        {
            lock (LockObject)
            {
                return spreadMatrixEntries.GetValue(datePair.Date1, datePair.Date2);
            }
        }

        public SecurityEntry Get(String securityID)
        {
            lock (LockObject)
            {
                if(!securityEntries.ContainsKey(securityID))
                    return null;

                return securityEntries[securityID];
            }
        }

        public SpreadMatrixDataCell GetDataCell(MDDatePair datePair)
        {
            lock (LockObject)
            {
                SecurityEntry entry = spreadMatrixEntries.GetValue(datePair.GetMinDate(), datePair.GetMaxDate());
                
                if (entry != null)
                {
                    SpreadMatrixDataCell cell = new SpreadMatrixDataCell();
                    cell.FillData(entry);
                    return cell;
                }

                return null;
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            foreach (SecurityEntry entry in securityEntries.Values)
                entry.Dispose();
            securityEntries.Clear();

            foreach (SecurityEntry entry in spreadMatrixEntries.Values)
                entry.Dispose();
            spreadMatrixEntries.Clear();

            minYearMonth = new DateTime();
            maxYearMonth = new DateTime();
        }

        void SetMinYearMonth(MDDatePair datePair)
        {
            if (minYearMonth == new DateTime())
                minYearMonth = datePair.GetMinDate();
            else
                minYearMonth = MDDatePair.GetMinDate(minYearMonth, datePair.GetMinDate());
        }

        void SetMaxYearMonth(MDDatePair datePair)
        {
            if (maxYearMonth == new DateTime())
                maxYearMonth = datePair.GetMaxDate();
            else
                maxYearMonth = MDDatePair.GetMaxDate(maxYearMonth, datePair.GetMaxDate());
        }

        public List<PureArbitrageRow> RunSimpleTest(ref TimeSpan ts)
        {
            lock (LockObject)
            {
                DateTime dt1 = DateTime.Now;
                List<Trade> trades = (new Alg(SideController)).Run();
                DateTime dt2 = DateTime.Now;
                ts = dt2 - dt1;

                return trades.Select(trade => PureArbitrageGrid.NewRow(trade,this)).ToList();
            }
        }

        public List<Trade> RunSimpleTest()
        {
            lock (LockObject)
            {
                List<Trade> trades = (new Alg(SideController)).Run();
                return trades;
            }
        }

        #endregion
    }

    internal class SecurityEntry: DisposableBaseObject
    {
        #region Fields
        SecurityEntry[] subcontracts;
        MDEntryGroup[] MDEntryGroups;
        #endregion

        #region Properties
        public SecurityEntry OwnerEntry { get; private set; }

        [IsCloneInheritable]
        public int SubContractCount { get; private set; }
        [IsCloneInheritable]
        public int MDGroupCount { get; private set; }

        [IsCloneInheritable]
        public string SecurityID { get; private set; }
        [IsCloneInheritable]
        public string Symbol { get; private set; }
        public string SecurityExchange { get; private set; }
        [IsCloneInheritable]
        public string SecurityType { get; private set; }
        [IsCloneInheritable]
        public string MaturityMonthYear { get; private set; }
        [IsCloneInheritable]
        public char Side { get; protected set; }
        //[IsCloneInheritable]
        //public string Currency { get; private set; }
        
        [IsCloneInheritable]
        public string MDReqID { get; private set; }
        [IsCloneInheritable]
        public bool IsMDUpdated { get; private set; }
        [IsCloneInheritable]
        public bool IsInverted { get; private set; }
        #endregion

        #region Constructor
        private SecurityEntry()
        {
            IsMDUpdated = false;
        }

        internal SecurityEntry(SecurityDefinition securityDefinition)
        {
            IsMDUpdated = false;

            SecurityID = securityDefinition.getSecurityID().getValue();
            Symbol = securityDefinition.getSymbol().getValue();
            SecurityExchange = securityDefinition.getSecurityExchange().getValue();
            SecurityType = securityDefinition.getSecurityType().getValue();
            
            if (securityDefinition.isSetMaturityMonthYear())
                MaturityMonthYear = securityDefinition.getMaturityMonthYear().getValue();

            NoRelatedSym noRelatedSym = securityDefinition.getNoRelatedSym();
            SubContractCount = noRelatedSym.getValue();
            subcontracts = new SecurityEntry[SubContractCount];
            SecurityDefinition.NoRelatedSym group = new SecurityDefinition.NoRelatedSym();
            for (uint i = 0; i < SubContractCount; i++)
            {
                securityDefinition.getGroup(i + 1, group);
                InsertSubContract(i, group);
            }

            // whether reverse contract
            if (HasFutureContract() && SubContractCount == 2)
            {
                IsInverted = GetSubContract((uint) GetNearSubContractIndex()).Side == QuickFix.Side.SELL;
            }
        }

        internal SecurityEntry(SecurityDefinition.NoRelatedSym noRelatedSym, SecurityEntry owner)
        {
            IsMDUpdated = false;
            OwnerEntry = owner;

            SecurityID = noRelatedSym.getUnderlyingSecurityID().getValue();
            Symbol = noRelatedSym.getUnderlyingSymbol().getValue();
            SecurityExchange = noRelatedSym.getUnderlyingSecurityExchange().getValue();
            SecurityType = noRelatedSym.getUnderlyingSecurityType().getValue();

            if (noRelatedSym.isSetUnderlyingMaturityMonthYear())
                MaturityMonthYear = noRelatedSym.getUnderlyingMaturityMonthYear().getValue();

            Side = noRelatedSym.getSide().getValue();
            SubContractCount = 0;
            subcontracts = new SecurityEntry[SubContractCount];
        }
        #endregion

        #region Methods
        
        internal bool HasFutureContract()
        {
            return (SecurityType == QuickFix.SecurityType.MULTILEG_INSTRUMENT &&
                    subcontracts.Select(contract => contract.HasFutureContract()).Aggregate((isFut, next) => isFut && next))
                   ||
                   SecurityType == QuickFix.SecurityType.FUTURE;
        }

        internal void UpdateMDEntry(MarketDataSnapshotFullRefresh entry, SideController sideController)
        {
            MDReqID = entry.getMDReqID().getValue();

            ClearGroups();

            MDGroupCount = entry.getNoMDEntries().getValue();
            MDEntryGroups = new MDEntryGroup[MDGroupCount];
            MarketDataSnapshotFullRefresh.NoMDEntries group = new MarketDataSnapshotFullRefresh.NoMDEntries();
            for (uint i = 0; i < MDGroupCount; i++)
            {
                entry.getGroup(i + 1, group);
                InsertGroup(i, group, sideController);
            }

            IsMDUpdated = true;
        }

        public SecurityEntry WeakClone()
        {
            SecurityEntry weakClone = new SecurityEntry();

            Utils.CopyPropertiesAndFields(this, weakClone);

            weakClone.MDEntryGroups = new MDEntryGroup[weakClone.MDGroupCount];
            for (uint i = 0; i < weakClone.MDGroupCount; i++)
                weakClone.MDEntryGroups[i] = GetGroup(i).Clone(weakClone);

            weakClone.subcontracts = new SecurityEntry[weakClone.SubContractCount];
            for (uint i = 0; i < weakClone.SubContractCount; i++)
            {
                weakClone.subcontracts[i] = GetSubContract(i).WeakClone();
                weakClone.subcontracts[i].OwnerEntry = weakClone;
            }

            return weakClone;
        }

        #region MDEntryGroup Methods
        void InsertGroup(uint index, MarketDataSnapshotFullRefresh.NoMDEntries group, SideController sideController)
        {
            MDEntryGroups[index] = new Proposal(group, this, GetDatePair(), sideController);
        }

        void ClearGroups()
        {
            MDGroupCount = 0;
            if (MDEntryGroups != null)
                foreach (MDEntryGroup entryGroup in MDEntryGroups)
                    entryGroup.Dispose();
            MDEntryGroups = new MDEntryGroup[MDGroupCount];
        }

        public MDEntryGroup GetBidGroup()
        {
            return MDEntryGroups.FirstOrDefault(group => group.MDEntryType == MDEntryType.BID);
        }

        public MDEntryGroup GetAskGroup()
        {
            return MDEntryGroups.FirstOrDefault(group => group.MDEntryType == MDEntryType.OFFER);
        }

        public MDEntryGroup GetGroup(uint index)
        {
            return MDEntryGroups[index];
        }

        public int GetGroupIndex(MDEntryGroup entryGroup)
        {
            for (int i = 0; i < MDGroupCount; i++)
            {
                if (MDEntryGroups[i] == entryGroup)
                    return i;
            }
            return -1;
        }
        #endregion

        #region SubContract Methods

        void InsertSubContract(uint index, SecurityDefinition.NoRelatedSym group)
        {
            subcontracts[index] = new SecurityEntry(group, this);
        }

        void ClearSubContract()
        {
            SubContractCount = 0;
            if (subcontracts != null)
                foreach (SecurityEntry entry in subcontracts)
                    entry.Dispose();
            subcontracts = new SecurityEntry[SubContractCount];
        }

        internal int GetNearSubContractIndex()
        {
            if (SubContractCount != 2)
                return -1;

            return GetDatePair().GetMinDate() ==
                   DateTime.ParseExact(GetSubContract(0).MaturityMonthYear, "yyyyMM", null)
                       ? 0
                       : 1;
        }

        internal int GetFarSubContractIndex()
        {
            if (SubContractCount != 2)
                return -1;

            return GetDatePair().GetMaxDate() ==
                   DateTime.ParseExact(GetSubContract(0).MaturityMonthYear, "yyyyMM", null)
                       ? 0
                       : 1;
        }

        public SecurityEntry GetSubContract(uint index)
        {
            return subcontracts[index];
        }

        #endregion

        public MDDatePair GetDatePair()
        {
            switch (SubContractCount)
            {
                case 0:
                    return new MDDatePair(MaturityMonthYear, MaturityMonthYear);
                case 1:
                    return new MDDatePair(GetSubContract(0).MaturityMonthYear, GetSubContract(0).MaturityMonthYear);
                case 2:
                    MDDatePair datePair = new MDDatePair(GetSubContract(0).MaturityMonthYear,
                                                         GetSubContract(1).MaturityMonthYear);
                    return new MDDatePair(datePair.GetMinDate(), datePair.GetMaxDate());
            }

            throw new Exception("Contracts that contains more than 2 subcontracts detected. These contracts currently not supported.");
        }

        protected override void DoDispose()
        {
            OwnerEntry = null;

            ClearGroups();
            ClearSubContract();
        }

        #endregion
    }

    internal class MDEntryGroup: DisposableBaseObject
    {
        #region Properties
        [IsCloneInheritable]
        public char MDEntryType { get; private set; }
        [IsCloneInheritable]
        public double MDEntryPx { get; private set; }
        [IsCloneInheritable]
        public double MDEntrySize { get; private set; }

        public SecurityEntry OwnerEntry { get; private set; }
        #endregion

        #region Constructor
        public MDEntryGroup(SecurityEntry owner)
        {
            OwnerEntry = owner;
        }

        public MDEntryGroup(MarketDataSnapshotFullRefresh.NoMDEntries group, SecurityEntry owner)
        {
            OwnerEntry = owner;

            MDEntryType = group.getMDEntryType().getValue();
            MDEntryPx = group.getMDEntryPx().getValue();
            MDEntrySize = group.getMDEntrySize().getValue();
        }
        #endregion

        #region Methods
        internal MDEntryGroup Clone(SecurityEntry newOwner)
        {
            MDEntryGroup newGroup = new MDEntryGroup(newOwner);

            Utils.CopyPropertiesAndFields(this, newGroup);

            return newGroup;
        }

        protected override void DoDispose()
        {
            OwnerEntry = null;
        }
        #endregion
    }

    internal class MDDatePair
    {
        #region Fields
        const int BaseYear = 2010;
        #endregion

        #region Properties
        public DateTime Date1 { get; private set; }
        public DateTime Date2 { get; private set; }
        #endregion

        #region Constructor
        public MDDatePair(DateTime date1, DateTime date2)
        {
            Date1 = date1;
            Date2 = date2;
        }

        public MDDatePair(string date1, string date2)
        {
            Date1 = DateTime.ParseExact(date1, "yyyyMM", null);
            Date2 = DateTime.ParseExact(date2, "yyyyMM", null);
        }

        public MDDatePair(string symbol, string sep, string SecurityAltId)
        {
            string[] fields = SecurityAltId.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length == 0)
                throw new Exception("Cannot get date data.");

            if (fields.Length > 2)
                throw new Exception("Unknown SecurityAltID field format.");

            for (int i = 0; i < fields.Length; i++)
            {
                DateTime dateTime = DecodeDate(fields[i],symbol);

                if (i == 0)
                    Date1 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);

                Date2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            }
        }
        #endregion

        #region Methods
        static DateTime DecodeDate(string securityAltIdDate, string symbol)
        {
            DateTime dateTime;
            if (DateTime.TryParse(securityAltIdDate, out dateTime))
                return dateTime;

            string securityAltIdDatePart = securityAltIdDate.Remove(0, symbol.Length);

            int month = ConvertMonth(securityAltIdDatePart[0]);

            if (month == 0)
                throw new Exception("Cannot decode month.");

            int dYear;
            bool parsed = Int32.TryParse(securityAltIdDatePart.Remove(0, 1), out dYear);

            if (!parsed)
                throw new Exception("Cannot parse year.");

            string dateString = (BaseYear + dYear).ToString("0000") + month.ToString("00");
            dateTime = DateTime.ParseExact(dateString, "yyyyMM", null);

            return dateTime;
        }

        static int ConvertMonth(char c)
        {
            return "FGHJKMNQUVXZ".IndexOf(c) + 1;
        }

        public DateTime GetMinDate()
        {
            return GetMinDate(Date1, Date2);
        }

        public static DateTime GetMinDate(DateTime date1, DateTime date2)
        {
            return date1 < date2 ? date1 : date2;
        }

        public DateTime GetMaxDate()
        {
            return GetMaxDate(Date1, Date2);
        }

        public static DateTime GetMaxDate(DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }
        #endregion
    }
}
