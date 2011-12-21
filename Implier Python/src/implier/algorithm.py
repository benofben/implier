import copy

# flips the +/- on a single leg (string) of a security
def switchLegSide(leg):
	if leg[0]=='+':
		return leg.replace('+', '-')
	elif leg[0]=='-':
		return leg.replace('-', '+')

# flips the +/- on legs of a security	
def switchLegsSides(legs):
	newLegs=[]
	for leg in legs:
		newLegs.append(switchLegSide(leg))
	return newLegs

def getUnmatchedLegs(legs):
	unmatchedLegs = copy.copy(legs)
	
	for leg in legs:
		oppositeLeg = switchLegSide(leg)

		try:
			# if this succeed then these legs cancel out
			oppositeIndex = unmatchedLegs.index(oppositeLeg)
			index = unmatchedLegs.index(leg)
			
			# this index is still current
			unmatchedLegs.pop(oppositeIndex)
			
			# because we've popped, the index has changed, so need to get it again
			index = unmatchedLegs.index(leg)
			unmatchedLegs.pop(index)

		except ValueError:
			# no match for leg
			pass
	
	return unmatchedLegs

def run(topOfBook):	
	# try to build a trade for each security (and each side)
	for securityDesc in topOfBook:
		for side in ['BID', 'OFFER']:
			if topOfBook[securityDesc][side]:
			
				trade = {}
				trade['securities'] = []
				trade['securities'].append(securityDesc)
		
				if side == 'OFFER': # then we're buying this security
					trade['legs'] = topOfBook[securityDesc]['LEGS']
					trade['price'] = topOfBook[securityDesc][side]
				elif side == 'BID': # then we're selling the security
					trade['legs'] = switchLegsSides(topOfBook[securityDesc]['LEGS'])
					trade['price'] = -1 * topOfBook[securityDesc][side]	
		
				recursiveSearch(trade, copy.deepcopy(topOfBook))
	
def recursiveSearch(trade, topOfBook):
	# first need to make sure that any securities in the trade are no longer in the book (regardless of side)
	for securityDesc in trade['securities']:
		try:
			topOfBook.pop(securityDesc)
		except KeyError:
			pass
	
	unmatchedLegs = getUnmatchedLegs(trade['legs'])

	if len(unmatchedLegs)>2:
		#then this is a bug!
		print(len(unmatchedLegs))
		exit(1)

	if len(unmatchedLegs)==0:
		if trade['price'] <= 0:
			pass
			print('Trade is balanced. Price is ' + str(trade['price']) + ' Number of legs is ' + str(len(trade['securities'])) + ': ' + str(trade['legs']))
		return

	for unmatchedLeg in unmatchedLegs:
		missingLeg = switchLegSide(unmatchedLeg)
		
		for securityDesc in topOfBook:
			
			# first trying buying a side
			try:
				topOfBook[securityDesc]['LEGS'].index(missingLeg)
				if topOfBook[securityDesc]['OFFER']:
					newTrade = copy.deepcopy(trade)
					newTrade['securities'].append(securityDesc)
					for leg in topOfBook[securityDesc]['LEGS']:
						newTrade['legs'].append(leg)
					newTrade['price'] += topOfBook[securityDesc]['OFFER']
					recursiveSearch(newTrade, copy.deepcopy(topOfBook))
			except ValueError:
				pass	
			
			# now try selling as well
			try:
				topOfBook[securityDesc]['LEGS'].index(switchLegSide(missingLeg))
				if topOfBook[securityDesc]['BID']:
					newTrade = copy.deepcopy(trade)
					newTrade['securities'].append(securityDesc)
					for leg in topOfBook[securityDesc]['LEGS']:
						newTrade['legs'].append(switchLegSide(leg))
					newTrade['price'] -= topOfBook[securityDesc]['BID']
					recursiveSearch(newTrade, copy.deepcopy(topOfBook))
			except ValueError:
				pass

	# If we get here then there aren't enough legs to balance the trade
	#print('Cannot balance the trade. ' + str(trade['legs']))