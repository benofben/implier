import copy

def run(securities):
	# split securities into outrights and spreads
	outrights=[]
	spreads=[]
	for security in securities:
		if len(security['Legs'])==1:
			outrights.append(security)
		elif len(security['Legs'])==2:
			spreads.append(security)
	
	# compute 1st generation implieds from the outrights
	impliedSpreads=[]
	for outright1 in outrights:
		for outright2 in outrights:
			#check the outrights are not the same maturity
			if outright1['Legs'][0][1:]==outright2['Legs'][0][1:]:
				pass
			#now make sure the outrights aren't on the same side, eg +,+
			elif outright1['Legs'][0][0]==outright2['Legs'][0][0]:
				pass
			else:
				impliedSpread={}
				impliedSpread['Legs']=set()
				impliedSpread['Legs'].add(outright1['Legs'][0])
				impliedSpread['Legs'].add(outright2['Legs'][0])
				impliedSpread['Price']=outright1['Price']+outright2['Price']
				impliedSpread['Updated']=True
				impliedSpread['ImpliedFromOutrights']=True
				impliedSpreads.append(impliedSpread)

	# see if the implied spreads are better priced than the spread and replace them if so.
	# also add the implieds if a spread is not present
	for impliedSpread in impliedSpreads:
		match=False
		for spread in spreads:
			if impliedSpread['Legs']==spread['Legs']:
				match=True
				if impliedSpread['Price']<spread['Price']:
					index=spreads.index(spread)
					spreads.pop(index)
					spreads.append(impliedSpread)
		if not match:
			spreads.append(impliedSpread)

	# split spreads into positive and negative
	positiveSpreads=[]
	negativeSpreads=[]
	for spread in spreads:
		if spread['Price']>0:
			positiveSpreads.append(spread)
		else:
			negativeSpreads.append(spread)	
		
	trade=[]
	negativeSpreads.sort(key=spreadKey, reverse=False)
	search(negativeSpreads, trade)
	
	'''
	for spread in spreads:
		if spread['Updated']==True and spread['Price']<-500:
			spreadsToPass = copy.deepcopy(spreads)
			recursiveSearch(None, spread, spreadsToPass, outrights, 1)			
	'''

def spreadKey(spread):
	return str(spread['Legs'])

def search(negativeSpreads, trade):	
	for negativeSpread in negativeSpreads:
		
		#enforce an ordering, so we don't do the same thing multiple times
		if not trade or spreadKey(negativeSpread)>spreadKey(trade[-1]):
			
			#check if any of these legs are already in the trade
			legAlreadyInTrade=False
			for leg in negativeSpread['Legs']:
				for security in trade:
					if leg in security['Legs']:
						legAlreadyInTrade=True
			
			if not legAlreadyInTrade:
				tradeToPass = copy.deepcopy(trade)
				tradeToPass.append(negativeSpread)
				tradeToPass.sort(key=spreadKey, reverse=False)

				negativeSpreadsToPass = copy.deepcopy(negativeSpreads)
				index = negativeSpreadsToPass.index(negativeSpread)
				negativeSpreadsToPass.pop(index)
				
				search(negativeSpreadsToPass, tradeToPass)

def recursiveSearch(trade, security, securities, outrights, depth):
	# I don't think there's any nice way to ensure that there are no subcycles in trades made up of outrights only.
	# For example: (-H2,+K3), (+H2,-K2), (-H2,+K2), (+H2,-K3)
	# Should find a way to deal with this later.
		
	if depth==1:
		trade={}
		trade['UnmatchedLegs']=copy.deepcopy(security['Legs'])
		trade['Securities']=[]
		trade['Securities'].append([security['Legs'],security['Price']])
		trade['Price']=security['Price']
	else:
		#try to match this security to the unmatched legs		
		foundAMatch = False
		if security['Legs'][0]==reverseLeg(trade['UnmatchedLegs'][0]):
			trade['UnmatchedLegs'].pop(0)
			trade['UnmatchedLegs'].append(security['Legs'][1])
			foundAMatch=True
		elif security['Legs'][1]==reverseLeg(trade['UnmatchedLegs'][0]):
			trade['UnmatchedLegs'].pop(0)
			trade['UnmatchedLegs'].append(security['Legs'][0])
			foundAMatch=True
		elif len(trade['UnmatchedLegs'])==2 and security['Legs'][0]==reverseLeg(trade['UnmatchedLegs'][1]):
			trade['UnmatchedLegs'].pop(1)
			trade['UnmatchedLegs'].append(security['Legs'][1])
			foundAMatch=True
		elif len(trade['UnmatchedLegs'])==2 and security['Legs'][1]==reverseLeg(trade['UnmatchedLegs'][1]):
			trade['UnmatchedLegs'].pop(1)
			trade['UnmatchedLegs'].append(security['Legs'][0])
			foundAMatch=True

		if foundAMatch:
			trade['Price']=trade['Price']+security['Price']
			trade['Securities'].append([security['Legs'],security['Price']])
		else:
			return

		# check if we just added a leg that cancels with our pre-existing unmatched leg, in which case we found a trade
		if trade['UnmatchedLegs'][0]==reverseLeg(trade['UnmatchedLegs'][1]):
			if(trade['Price']<1000):
				print('Found a trade for ' + str(trade['Price']) + ' with depth ' + str(depth) + '.  ' + str(trade['Securities']))
			return
		else:
			#let's try capping this off with outrights...
			if reverseLeg(trade['UnmatchedLegs'][0]) in outrights and reverseLeg(trade['UnmatchedLegs'][1]) in outrights:
				leg1=reverseLeg(trade['UnmatchedLegs'][0])
				leg2=reverseLeg(trade['UnmatchedLegs'][1])
				price=trade['Price']+outrights[leg1]+outrights[leg2]
				if price<1000:
					trade['Securities'].append([leg1, outrights[leg1]])
					trade['Securities'].append([leg2, outrights[leg2]])
					print('Found an outright trade for ' + str(trade['Price']) + ' with depth ' + str(depth) + '.  ' + str(trade['Securities']))
					exit()
	
	# Give up
	if trade['Price']>0:
		return
		
	index = securities.index(security)
	securities.pop(index)
	for security in securities:
		if reverseLeg(trade['UnmatchedLegs'][0])==security['Legs'][0] or reverseLeg(trade['UnmatchedLegs'][0])==security['Legs'][1] or reverseLeg(trade['UnmatchedLegs'][1])==security['Legs'][0] or reverseLeg(trade['UnmatchedLegs'][1])==security['Legs'][1]:
			tradeToPass = copy.deepcopy(trade)
			securitiesToPass = copy.deepcopy(securities)
			recursiveSearch(tradeToPass, security, securitiesToPass, outrights, depth+1)

def reverseLeg(leg):
	if leg[0]=='+':
		return leg.replace('+','-')
	elif leg[0]=='-':
		return leg.replace('-','+')
