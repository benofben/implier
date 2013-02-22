import fix.fixParser
import fix.fixFileReader
import implier.formatter
import implier.algorithm
import time

def run():
	inputFilename = '/Users/benlackey/Documents/implier/output.txt'
	myFixParser = fix.fixParser.fixParser()
	
	print('Working on file ' + inputFilename)
	myFixFileReader = fix.fixFileReader.fixFileReader(inputFilename, myFixParser)
	
	while(True):
		securities = myFixFileReader.updateOrderBookWithNextLine()

		if securities:
			simpleSecurities = implier.formatter.reformat(securities, myFixParser)
			print('Number of securities is ' + str(len(simpleSecurities)) + '.')

			if len(simpleSecurities)==198:
				printSpreadMatrix(simpleSecurities)
				exit()
				
			'''
			startTime = time.time()
			implier.algorithm.run(simpleSecurities)
			elapsedTime = time.time() - startTime
			print('It took ' + str(int(elapsedTime)) + ' seconds to run the algorithm.')			
			'''
		else:
			# End of file
			break
		
def printSpreadMatrix(securities):	
	# first, let's get a list of all the legs
	legs=set()
	for security in securities:
		for leg in security['Legs']:
			#ignore the mini contract, XS
			if not 'XS' in leg:
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
		if not 'XS' in security['Legs'][0]:
			xLeg=security['Legs'][0].replace('SI','')
			if len(security['Legs'])==2:
				yLeg=security['Legs'][1].replace('SI','')
			else:
				yLeg=security['Legs'][0].replace('SI','')
				
			if not xLeg in spreadMatrix:
				spreadMatrix[xLeg]={}
			spreadMatrix[xLeg][yLeg]=int(security['Price'])

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