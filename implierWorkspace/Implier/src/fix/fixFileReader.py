class fixFileReader:
	def __init__(self, inputFilename, myFixParser):
		self.myFixParser = myFixParser
		self.inputFile = open(inputFilename)
		self.orderbook={}

	def __del__(self):			
		self.inputFile.close()

	def updateOrderBookWithNextLine(self):	
		message=self.inputFile.readline()
		if len(message)==0:
			return None
		
		fields = self.myFixParser.getFields(message)
		fields = self.myFixParser.lookupFields(fields)
		
		msgType = self.myFixParser.getField(35, fields)
		sendingTime = self.myFixParser.getField(52, fields)
		
		print('Sending time is ' + sendingTime + '.  Order book length is ' + str(len(self.orderbook)) + '.') 
		
				
		# skip SecurityStatus
		if msgType == 'f':
			pass
		elif msgType != 'X':
			print('Got a message I do not know how to deal with of type: ' + msgType)
		else:			
			mdEntries = self.myFixParser.getMDEntries(fields)
			for mdEntry in mdEntries:				
				# I'm just going to treat exchange implied prices as standard prices.  Maybe drop this out later.
				
				securityDesc = self.myFixParser.getField(107, mdEntry)
				if not securityDesc in self.orderbook:
					self.orderbook[securityDesc]={}
					self.orderbook[securityDesc]['BID']=[None]*10
					self.orderbook[securityDesc]['OFFER']=[None]*10

				mdEntryType = self.myFixParser.getFieldandLookup(269, mdEntry)
				if mdEntryType=='BID' or mdEntryType=='OFFER':

					quoteCondition = self.myFixParser.getFieldandLookup(276, mdEntry)
					if quoteCondition == 'EXCHANGE_BEST':
						# then this is a "Last Best Price" as described on page 64 of http://www.cmegroup.com/globex/files/SDKFFCore.pdf
						# I'm just going to ignore these.  I think they only have meaning for trade busting rules, but not certain.
						pass
					else:
						mdPriceLevel = int(self.myFixParser.getField(1023, mdEntry))
						index = mdPriceLevel-1
	
						mdUpdateAction = self.myFixParser.getFieldandLookup(279, mdEntry)					
						if mdUpdateAction=='NEW':
							self.orderbook[securityDesc][mdEntryType].insert(index, mdEntry)
							self.orderbook[securityDesc][mdEntryType].pop()
						elif mdUpdateAction=='CHANGE':
							self.orderbook[securityDesc][mdEntryType][index]=mdEntry
						elif mdUpdateAction=='DELETE':
							self.orderbook[securityDesc][mdEntryType].pop(index)
							self.orderbook[securityDesc][mdEntryType].append(9)
						
				elif mdEntryType=='SETTLEMENT_PRICE':
					pass
				elif mdEntryType=='SIMULATED_SELL_PRICE':
					pass
				elif mdEntryType=='SIMULATED_BUY_PRICE':
					pass
				elif mdEntryType=='TRADE':
					pass
				elif mdEntryType=='TRADE_VOLUME':
					pass
				elif mdEntryType=='OPEN_INTEREST':
					pass
				elif mdEntryType=='OPENING_PRICE':
					pass
				elif mdEntryType=='TRADING_SESSION_HIGH_PRICE':
					pass
				elif mdEntryType=='TRADING_SESSION_LOW_PRICE':
					pass
				elif mdEntryType=='SESSION_LOW_OFFER':
					pass
				else:
					print('Got an mdEntryType I do not know how to deal with ' + str(mdEntryType))
					exit()
		
		return self.orderbook
	
	def printOrderBookForSecurity(self, securityDesc, orderbook):
		print(securityDesc)
		for i in range(9,-1,-1):
			try:
				price = self.myFixParser.getField(270,orderbook[securityDesc]['OFFER'][i])
			except TypeError:
				price = None
			print('ASK ' + str(i) + ' ' + str(price))
		for i in range(0,10):
			try:
				price = self.myFixParser.getField(270,orderbook[securityDesc]['BID'][i])
			except TypeError:
				price = None
			print('BID ' + str(i) + ' ' + str(price))
		#input("Press Enter to continue...")
		print('\n\n')