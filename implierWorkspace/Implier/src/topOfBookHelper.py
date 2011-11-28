def getTopOfBook(orderBook, myFixParser):
	topOfBook={}
	
	for securityDesc in orderBook:
		topOfBook[securityDesc]={}

		topOfBook[securityDesc]['LEGS']=securityDesc.split('-')
		topOfBook[securityDesc]['LEGS'][0]='+' + topOfBook[securityDesc]['LEGS'][0]
		if len(topOfBook[securityDesc]['LEGS'])>1:
			topOfBook[securityDesc]['LEGS'][1]='-' + topOfBook[securityDesc]['LEGS'][1]
		
		try:
			topOfBook[securityDesc]['BID']=float(myFixParser.getField(270,orderBook[securityDesc]['BID'][0]))
		except TypeError:
			topOfBook[securityDesc]['BID']=None
		
		try:
			topOfBook[securityDesc]['OFFER']=float(myFixParser.getField(270,orderBook[securityDesc]['OFFER'][0]))
		except TypeError:
			topOfBook[securityDesc]['OFFER']=None
			
		# if the bid/ask is null, delete it
		if not topOfBook[securityDesc]['BID'] and not topOfBook[securityDesc]['OFFER']:
			topOfBook.pop(securityDesc)
		
	return topOfBook

def printTopOfBook(topOfBook):
	for securityDesc in topOfBook:
		if topOfBook[securityDesc]['BID'] or topOfBook[securityDesc]['OFFER']:
			print(securityDesc)
			print(topOfBook[securityDesc]['LEGS'])
			print('OFFER ' + str(topOfBook[securityDesc]['OFFER']))
			print('BID ' + str(topOfBook[securityDesc]['BID']))
			print('\n\n')
