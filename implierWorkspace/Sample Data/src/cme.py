
def parse(line):
	tick={}
	tick['TradeDate'] = line[0:7]
	tick['TradeTime'] = line[8:13]
	tick['TradeSequenceNumber'] = line[14:21]
	tick['SessionIndicator'] = line[22]
	tick['TickerSymbol'] = line[23:25]
	tick['FOIIndicator'] = line[26]
	tick['DeliveryDate'] = line[27:30]
	tick['TradeQuantity'] = line[31:35]
	tick['StrikePrice'] = line[36:42]
	tick['StrikePriceDecimalLocator'] = line[43]
	tick['TradePrice'] = line[44:50]
	tick['TradePriceDecimalLocator'] = line[51]
	tick['AskBidType'] = line[52]
	tick['IndicativeQuoteType'] = line[53]
	tick['MarketQuote'] = line[54]
	tick['CloseOpenType'] = line[55]
	tick['ValidOpenException'] = line[56:57]
	tick['PostClose'] = line[58]
	tick['CancelCodeType'] = line[59]
	tick['InsertCodeType'] = line[60]
	tick['FastLateIndicator'] = line[61]
	tick['CabinetIndicator'] = line[62]
	tick['BookIndicator'] = line[63]
	tick['EntryDate'] = line[64:69]
	return tick

file = open('C:\workspace\data\CME\XCME_LN_FUT_110110.TXT')
lines = file.readlines()
file.close()

ticks = []
for line in lines:
	ticks.append(parse(line))
	
deliveryDates={}
for tick in ticks:
	deliveryDates[tick['DeliveryDate']]=0
	
for key in deliveryDates:
	print(key)
	
