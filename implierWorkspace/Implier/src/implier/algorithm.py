import copy

def run(securities):

	for security in securities:
		#if security['Updated']==True:
		if security['Updated']==True and len(security['Legs'])==2 and security['Price']<-500:
			#then we need to run a search from this point
			securitiesToPass = copy.deepcopy(securities)
			recursiveSearch(None, security, securitiesToPass, 1)

def recursiveSearch(trade, security, securities, depth):
	# I don't think there's any nice way to ensure that there are no subcycles in trades made up of outrights only.
	# For example: (-H2,+K3), (+H2,-K2), (-H2,+K2), (+H2,-K3)
	# Should find a way to deal with this later.
		
	if depth==1:
		trade={}
		trade['UnmatchedLegs']=copy.deepcopy(security['Legs'])
		trade['Securities']=[]
		trade['Securities'].append(copy.deepcopy(security['Legs']))
		trade['Price']=security['Price']
		trade['SpreadOnlyPrice']=0
	else:
		#try to match this security to the unmatched legs
		
		assert len(security['Legs'])==1 or len(security['Legs'])==2
		assert len(trade['UnmatchedLegs'])==1 or len(trade['UnmatchedLegs'])==2
		
		foundAMatch = False
		
		#outright
		if len(security['Legs'])==1:
			if security['Legs'][0]==reverseLeg(trade['UnmatchedLegs'][0]):
				trade['UnmatchedLegs'].pop(0)
				foundAMatch=True
			elif len(trade['UnmatchedLegs'])==2 and security['Legs'][0]==reverseLeg(trade['UnmatchedLegs'][1]):
				trade['UnmatchedLegs'].pop(1)
				foundAMatch=True

		#spread
		elif len(security['Legs'])==2:
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

			# check if we just added a leg that cancels with our pre-existing unmatched leg
			if len(trade['UnmatchedLegs'])==2 and trade['UnmatchedLegs'][0]==reverseLeg(trade['UnmatchedLegs'][1]):
				trade['UnmatchedLegs'].pop(0)
				trade['UnmatchedLegs'].pop(0)
			
		if foundAMatch:
			trade['Price']=trade['Price']+security['Price']
			trade['Securities'].append(security['Legs'])				
		else:
			return
	
	assert(depth==len(trade['Securities']))						
	
	# Check if we're done
	if len(trade['UnmatchedLegs'])==0:
		assert(len(trade['Securities'])!=1)
		if(trade['Price']<0):
			print('Found a trade for ' + str(trade['Price']) + ' with depth ' + str(depth) + '.  ' + str(trade['Securities']))
		else:
			pass
		return

	if len(security['Legs'])==2:
		trade['SpreadOnlyPrice']=trade['SpreadOnlyPrice']+security['Price']

	# give up on looking deeper
	if trade['SpreadOnlyPrice']>0 or depth>5:
		return
	
	index = securities.index(security)
	securities.pop(index)

	for security in securities:
		tradeToPass = copy.deepcopy(trade)
		securitiesToPass = copy.deepcopy(securities)
		recursiveSearch(tradeToPass, security, securitiesToPass, depth+1)

def reverseLeg(leg):
	if leg[0]=='+':
		return leg.replace('+','-')
	elif leg[0]=='-':
		return leg.replace('-','+')
	else:
		print('Something is wrong with this leg.')
		exit()