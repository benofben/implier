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
			security['Legs']=securityDesc.split('-')

			security['Legs'][0]='+' + security['Legs'][0]
			if len(security['Legs'])>1:
				security['Legs'][1]='-' + security['Legs'][1]

			security['Price']=float(myFixParser.getField(270,securities[securityDesc]['OFFER']['OrderBook'][0]))			
			security['Updated']=securities[securityDesc]['OFFER']['Updated']
			simpleSecurities.append(security)
		except TypeError:
			pass

		# Handle bids		
		try:
			security={}
			security['Legs']=securityDesc.split('-')

			security['Legs'][0]='-' + security['Legs'][0]
			if len(security['Legs'])>1:
				security['Legs'][1]='+' + security['Legs'][1]

			security['Price']=float(myFixParser.getField(270,securities[securityDesc]['BID']['OrderBook'][0]))*-1
			security['Updated']=securities[securityDesc]['BID']['Updated']
			simpleSecurities.append(security)
		except TypeError:
			pass
		
	return simpleSecurities
