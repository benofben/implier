def reformat(securities, myFixParser):
	'''
	Returns a simple securities with:
		only the top of the order book, dropping all other entries.
		eliminates the distinction between bid and ask, instead expressing everything as an individual security such as +SIK2 with a price.
		prices for buys are positive, sells negative
	'''
	
	simpleSecurities=[]
	
	for securityDesc in securities:	
		### insert logic to handle butterflies later (Silver doesn't seem to have these, or maybe they're in another file)
		security={}
		security['Legs']=securityDesc.split('-')
		if len(security['Legs'])>=3:
			print('Got a butterfly or something similar.')
			exit()
		
		# Handle offers
		try:
			security={}

			temp=securityDesc.split('-')
			if len(temp)==1:
				security['Legs']=[]
				security['Legs'].append('+' + temp[0])
			elif len(temp)==2:
				security['Legs']=set()
				security['Legs'].add('+' + temp[0])
				security['Legs'].add('-' + temp[1])

			for level in securities[securityDesc]['OFFER']['OrderBook']:
				quoteCondition = myFixParser.getFieldandLookup(276, level)
				if quoteCondition == 'IMPLIED_PRICE':
					pass
				else:
					security['Price']=float(myFixParser.getField(270,level))			
					security['Updated']=securities[securityDesc]['OFFER']['Updated']
					simpleSecurities.append(security)
					break
							
		except TypeError:
			pass

		# Handle bids		
		try:
			security={}

			if len(temp)==1:
				security['Legs']=[]
				security['Legs'].append('-' + temp[0])
			elif len(temp)==2:
				security['Legs']=set()
				security['Legs'].add('-' + temp[0])
				security['Legs'].add('+' + temp[1])

			for level in securities[securityDesc]['BID']['OrderBook']:
				quoteCondition = myFixParser.getFieldandLookup(276, level)
				if quoteCondition == 'IMPLIED_PRICE':
					pass
				else:
					security['Price']=float(myFixParser.getField(270,level))*-1
					security['Updated']=securities[securityDesc]['BID']['Updated']
					simpleSecurities.append(security)
					break

		except TypeError:
			pass
		
	return simpleSecurities
