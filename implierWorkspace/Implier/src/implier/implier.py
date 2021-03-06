import fix.fixParser
import fix.fixFileReader
import implier.formatter
import implier.algorithm
import time

def run():
	inputFilename = '/Users/benlackey/Documents/implier/output.txt'
	inputFilename = 'C:\implierData\output.txt'
	myFixParser = fix.fixParser.fixParser()
	
	print('Working on file ' + inputFilename)
	myFixFileReader = fix.fixFileReader.fixFileReader(inputFilename, myFixParser)
	
	while(True):
		securities = myFixFileReader.updateOrderBookWithNextLine()

		if securities:
			simpleSecurities = implier.formatter.reformat(securities, myFixParser)
			print('Number of securities is ' + str(len(simpleSecurities)) + '.')

			startTime = time.time()
			implier.algorithm.run(simpleSecurities)
			elapsedTime = time.time() - startTime
			print('It took ' + str(int(elapsedTime)) + ' seconds to run the algorithm.')			
		else:
			# End of file
			break