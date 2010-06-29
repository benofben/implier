using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Implier.CommonControls.Windows;
using Implier.FIXApplication;
using Implier.Graph;
using Implier.PureArbitrage;
using QuickFix;
using QuickFix42;

//using MDEntry = QuickFix42.MarketDataSnapshotFullRefresh;
//using MDDictionary = System.Collections.Generic.Dictionary<System.DateTime, System.Collections.Generic.Dictionary<System.DateTime, QuickFix42.MarketDataSnapshotFullRefresh>>;
//using MDGroupDictionary = System.Collections.Generic.Dictionary<string, object>;

namespace Implier.SpreadMatrix
{
    public class SpreadMatrixData: WindowSupportableObject
    {
        #region Fields

        DualKeyDictionary<DateTime, DateTime, MDEntry> entries = new DualKeyDictionary<DateTime, DateTime, MDEntry>();
        DateTime minYearMonth = new DateTime();
        DateTime maxYearMonth = new DateTime();
        
        #endregion

        #region Properties

        internal string Exchange { get; private set; }
        internal string Symbol { get; private set; }


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
        internal IEnumerable<MDEntry> Values
        {
            get { return entries.Values; }
        }


        #endregion

        #region Delegates and Events

        //public delegate void AddEntryHandler(object sender, MDEntry entry);
        //public event AddEntryHandler OnEntryAdded;

        #endregion

        #region Methods

        internal SpreadMatrixData(string exchange, string symbol)
        {
            Exchange = exchange;
            Symbol = symbol;
            SideController = new SideController();
        }

        void AddEntryData(MDEntry entry)
        {
            MDDatePair datePair = entry.GetDatePair();

            MDEntry oldEntry = entries.GetValue(datePair.Date1, datePair.Date2);
            
            if (oldEntry != null)
                oldEntry.Dispose();

            entries.SetValue(datePair.Date1, datePair.Date2, entry);

            SetMinYearMonth(datePair);
            SetMaxYearMonth(datePair);
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

        public void Add(MDEntry entry)
        {
            lock (LockObject)
            {
                if (entry.GroupCount > 0)
                {
                    AddEntryData(entry);

                    RaizeChanged(entry);
                }
            }
        }
                
        public MDEntry Get(MDDatePair datePair)
        {
            lock (LockObject)
            {
                return entries.GetValue(datePair.Date1, datePair.Date2);
            }
        }
        
        public MDEntry GetWeakCopy(MDDatePair datePair)
        {
            lock (LockObject)
            {
                MDEntry entry = entries.GetValue(datePair.Date1, datePair.Date2);

                return entry != null ? entry.WeakClone() : null;
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            foreach (MDEntry entry in entries.Values )
            {
                for (uint i=0;i<entry.GroupCount;i++)
                    ((Proposal) entry.GetGroup(i)).Dispose();
            }
            entries.Clear();
            minYearMonth = new DateTime();
            maxYearMonth = new DateTime();
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

        #endregion
    }

    public class MDEntry: DisposableBaseObject
    {
        #region Fields
        const int SecurityAltIdTag = 10455;
        const string SecurityAltIdSep = "-";
        MDEntryGroup[] groups;
        #endregion

        #region Properties
        [IsCloneInheritable]
        public int GroupCount { get; private set; }
        [IsCloneInheritable]
        public string Symbol { get; private set; }
        //[IsCloneInheritable]
        //public string Currency { get; private set; }
        [IsCloneInheritable]
        public string SecurityAltID { get; private set; }
        [IsCloneInheritable]
        public string SecurityExchange { get; private set; }
        [IsCloneInheritable]
        public string SecurityType { get; private set; }
        [IsCloneInheritable]
        public string SecurityID { get; private set; }
        [IsCloneInheritable]
        public string MDReqID { get; private set; }
        #endregion

        #region Constructor
        private MDEntry(){ }

        internal MDEntry(MarketDataSnapshotFullRefresh entry, SideController sideController)
        {
            GroupCount = entry.getNoMDEntries().getValue();
            Symbol = entry.getSymbol().getValue();

            // prod FIX adapter isn't providing the currency field for some reason
            //Currency = entry.getField(new Currency()).getValue();
            
            SecurityAltID = entry.getField(new StringField(SecurityAltIdTag)).getValue();
            SecurityExchange = entry.getSecurityExchange().getValue();
            SecurityType = entry.getSecurityType().getValue();
            SecurityID = entry.getSecurityID().getValue();
            MDReqID = entry.getMDReqID().getValue();

            groups = new Proposal[GroupCount];
            MarketDataSnapshotFullRefresh.NoMDEntries group = new MarketDataSnapshotFullRefresh.NoMDEntries();
            for (uint i = 0; i < GroupCount; i++)
            {
                entry.getGroup(i + 1, group);
                InsertGroup(i, group, sideController);
            }
        }
        #endregion

        #region Methods
        public MDEntry WeakClone()
        {
            MDEntry weakClone = new MDEntry();

            Utils.CopyPropertiesAndFields(this, weakClone);

            weakClone.groups = new MDEntryGroup[weakClone.GroupCount];
            for (uint i = 0; i < weakClone.GroupCount; i++)
                weakClone.groups[i] = GetGroup(i).Clone(weakClone);

            return weakClone;
        }

        void InsertGroup(uint index, MarketDataSnapshotFullRefresh.NoMDEntries group, SideController sideController)
        {
            groups[index] = new Proposal(group, this, GetDatePair(), sideController);
        }

        public MDEntryGroup GetGroup(uint index)
        {
            return groups[index];
        }

        public int GetIndex(MDEntryGroup entryGroup)
        {
            for (int i = 0; i < GroupCount; i++)
            {
                if (groups[i] == entryGroup)
                    return i;
            }
            return -1;
        }

        public MDEntryGroup GetBidGroup()
        {
            return groups.FirstOrDefault(mdEntryGroup => mdEntryGroup.EntryType == MDEntryType.BID);
        }

        public MDEntryGroup GetAskGroup()
        {
            return groups.FirstOrDefault(mdEntryGroup => mdEntryGroup.EntryType == MDEntryType.OFFER);
        }

        public MDDatePair GetDatePair()
        {
            return new MDDatePair(Symbol, SecurityAltIdSep, SecurityAltID);
        }

        protected override void DoDispose()
        {
            foreach (MDEntryGroup entryGroup in groups)
                entryGroup.Dispose();
            //throw new NotImplementedException();
        }
        #endregion
    }

    public class MDEntryGroup: DisposableBaseObject
    {
        #region Properties
        [IsCloneInheritable]
        public char EntryType { get; private set; }
        [IsCloneInheritable]
        public double EntryPx { get; private set; }
        [IsCloneInheritable]
        public double EntrySize { get; private set; }
        [IsCloneInheritable]
        public int PositionNo { get; private set; }
        public MDEntry OwnerEntry { get; private set; }
        #endregion

        #region Constructor
        public MDEntryGroup(MDEntry owner)
        {
            OwnerEntry = owner;
        }

        public MDEntryGroup(MarketDataSnapshotFullRefresh.NoMDEntries group, MDEntry owner)
        {
            OwnerEntry = owner;

            MDEntryType MDEntryType = new MDEntryType();
            MDEntryPx MDEntryPx = new MDEntryPx();
            MDEntrySize MDEntrySize = new MDEntrySize();
            MDEntryPositionNo MDEntryPositionNo = new MDEntryPositionNo();

            group.get(MDEntryType);
            group.get(MDEntryPx);
            group.get(MDEntrySize);
            group.get(MDEntryPositionNo);

            EntryType = MDEntryType.getValue();
            EntryPx = MDEntryPx.getValue();
            EntrySize = MDEntrySize.getValue();
            PositionNo = MDEntryPositionNo.getValue();
        }
        #endregion

        #region Methods
        internal MDEntryGroup Clone(MDEntry newOwner)
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

    public class MDDatePair
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
