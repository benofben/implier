import fix.fixParser
import fix.fixFileReader
import implier.formatter
import implier.algorithm
import implier.printSpreadMatrix

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

			implier.algorithm.run(simpleSecurities)
#			implier.printSpreadMatrix.printSpreadMatrix(simpleSecurities)
				
		else:
			# End of file
			break
