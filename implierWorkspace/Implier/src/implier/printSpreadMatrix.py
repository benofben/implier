
def printSpreadMatrix(securities):
	# first, let's get a list of all the legs
	legs=set()
	for security in securities:
		for leg in security['Legs']:
			leg = leg.replace('SI','')
			leg = leg.replace('+','')
			leg = leg.replace('-','')
			leg = leg[::-1]
			legs.add(leg)

	legsList=[]
	for leg in legs:
		legsList.append(leg)
	legs=legsList
	legs.sort()
	
	newLegs=[]
	for leg in legs:
		newLegs.append(leg[::-1])
	legs=newLegs
			
	#now, reformat securities so it's easier to lookup by leg
	spreadMatrix={}
	for security in securities:

		xLeg=None
		yLeg=None
		for leg in security['Legs']:
			leg = leg.replace('SI','')
			if not xLeg:
				xLeg=leg[::-1]
			elif not yLeg:
				yLeg=leg[::-1]

		if not yLeg:
			yLeg=xLeg
		
		if xLeg>yLeg:
			temp=xLeg
			xLeg=yLeg
			yLeg=temp
		
		if not xLeg[::-1] in spreadMatrix:
			spreadMatrix[xLeg[::-1]]={}
		
		spreadMatrix[xLeg[::-1]][yLeg[::-1]]=int(security['Price'])

	#and now we go through the spread matrix we just made and write it to a file
	file = open('spreadMatrix.csv','w')
	line=','
	for leg in legs:
		line=line+leg+','
	file.write(line + '\n')

	for xLeg in legs:
		line=xLeg+','
		for yLeg in legs:
			if xLeg==yLeg:
				if ('+' + xLeg in spreadMatrix) and ('+' + yLeg in spreadMatrix['+'+xLeg]):
					line=line+str(spreadMatrix['+'+xLeg]['+'+yLeg])+'|'
				else:
					line=line+' '+'|'

				if ('-' + xLeg in spreadMatrix) and ('-' + yLeg in spreadMatrix['-'+xLeg]):
					line=line+str(spreadMatrix['-'+xLeg]['-'+yLeg])+','
				else:
					line=line+' '+','
			else:
				if ('+' + xLeg in spreadMatrix) and ('-' + yLeg in spreadMatrix['+'+xLeg]):
					line=line+str(spreadMatrix['+'+xLeg]['-'+yLeg])+'|'
				else:
					line=line+' '+'|'				

				if ('-' + xLeg in spreadMatrix) and ('+' + yLeg in spreadMatrix['-'+xLeg]):
					line=line+str(spreadMatrix['-'+xLeg]['+'+yLeg])+','
				else:
					line=line+' '+','

		file.write(line + '\n')
	file.close()
