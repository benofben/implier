import fix.fixParser
import fix.fixFileReader
import implier.topOfBookHelper
import implier.algorithm
import time

def run():
	inputFilename = 'C:\\implierData\\output.txt'
	myFixParser = fix.fixParser.fixParser()
	
	print('Working on file ' + inputFilename)
	myFixFileReader = fix.fixFileReader.fixFileReader(inputFilename, myFixParser)
	
	while(True):
		orderbook = myFixFileReader.updateOrderBookWithNextLine()
		if orderbook:
			topOfBook = implier.topOfBookHelper.getTopOfBook(orderbook, myFixParser)
			
			#implier.topOfBookHelper.printTopOfBook(topOfBook)
			
			startTime = time.time()
			implier.algorithm.run(topOfBook)
			elapsedTime = time.time() - startTime
			print('It took ' + str(int(elapsedTime)) + ' seconds to rerun the algorithm.')
		else:
			# End of file
			break