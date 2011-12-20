import copy

def run(topOfBook):	
	# first I'm going to turn topOfBook into something I like a bit better....
	securities = reformat(topOfBook)
	
	for security in securities:
		if len(security['Legs']) == 1:
			# then this is an outright and we can start a trade
			trade = {}
			trade['Price']=security['Price']
			trade['UnmatchedLeg']=security['Legs'][0]
			
			securitiesToPass = copy.deepcopy(securities)
			index = securitiesToPass.index(security)
			securitiesToPass.pop(index)
			recursiveSearch(trade, securitiesToPass, 1)
	
def reformat(topOfBook):
	securities = []
	
	for securityDesc in topOfBook:
		if topOfBook[securityDesc]['OFFER']:
			offersecurity = {}
			offersecurity['Price'] = 1 * float(topOfBook[securityDesc]['OFFER'])
			offersecurity['Legs'] = []
			for leg in topOfBook[securityDesc]['LEGS']:
				offersecurity['Legs'].append(1*securityToString(leg))
			securities.append(offersecurity)
			
		if topOfBook[securityDesc]['BID']:
			bidsecurity = {}
			bidsecurity['Price'] = -1 * float(topOfBook[securityDesc]['BID'])
			bidsecurity['Legs'] = []
			for leg in topOfBook[securityDesc]['LEGS']:
				bidsecurity['Legs'].append(-1*securityToString(leg))
			securities.append(bidsecurity)
	
	return securities

### this is crap and needs fixed as there are collisions possible
def securityToString(leg):
	index = 0
	y = 0
	for character in leg:
		if index == 0 and character == '+':
			x = 1
		if index == 0 and character == '-':
			x =-1
		if index != 0:
			y = y + ord(character)*index
		index = index + 1
	return x * y
	
def recursiveSearch(trade, securities, depth):
	
	'''
	#restrict the depth of the search
	depth = depth + 1
	if depth > 5:
		return
	'''
	
	for security in securities:
		for leg in security['Legs']:
			if trade['UnmatchedLeg'] + leg == 0:
				tradeToPass = copy.deepcopy(trade)
				tradeToPass['Price'] = trade['Price'] + security['Price'] 
				
				if len(security['Legs'])==1:
					# then the trade is match
					if(tradeToPass['Price']<0):
						print('Found a trade at ' + str(tradeToPass['Price']))
				elif len(security['Legs'])==2:
					# then we've got a spread and are adding another leg				
					tradeToPass['UnmatchedLeg'] = trade['UnmatchedLeg'] + security['Legs'][0] + security['Legs'][1]
			
					securitiesToPass = copy.deepcopy(securities)
					index = securitiesToPass.index(security)
					securitiesToPass.pop(index)
					recursiveSearch(tradeToPass, securitiesToPass, depth)
