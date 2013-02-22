import fix.fixParser
import fix.fixFileReader
import implier.formatter
import implier.algorithm
import implier.printSpreadMatrix
import time

def run():
	inputFilename = '/Users/benlackey/Documents/implier/output.txt'
	myFixParser = fix.fixParser.fixParser()
	
	print('Working on file ' + inputFilename)
	myFixFileReader = fix.fixFileReader.fixFileReader(inputFilename, myFixParser)
	
	while(True):
		securities = myFixFileReader.updateOrderBookWithNextLine()
		
#		for security in securities:
#			myFixFileReader.printOrderBookForSecurity(security)

		if securities:
			simpleSecurities = implier.formatter.reformat(securities, myFixParser)
			print('Number of securities is ' + str(len(simpleSecurities)) + '.')

			if len(simpleSecurities)==88:
				implier.printSpreadMatrix.printSpreadMatrix(simpleSecurities)
				implier.printSpreadMatrix.printSpreadMatrix2(simpleSecurities)
				exit()
				
#			startTime = time.time()
			implier.algorithm.run(simpleSecurities)
#			elapsedTime = time.time() - startTime
#			print('It took ' + str(int(elapsedTime)) + ' seconds to run the algorithm.')			
		else:
			# End of file
			break
