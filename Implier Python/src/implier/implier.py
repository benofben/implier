import fix.fixParser
import fix.fixFileReader
import implier.topOfBookHelper
import implier.algorithm
import time

def run():
	inputFilename = 'C:\\Users\\ben\\Desktop\\output.txt'
	myFixParser = fix.fixParser.fixParser()
	
	print('Working on file ' + inputFilename)
	myFixFileReader = fix.fixFileReader.fixFileReader(inputFilename, myFixParser)

	startTime = time.time()
	
	while(True):		
		orderbook = myFixFileReader.updateOrderBookWithNextLine()
		if orderbook:			
			topOfBook = implier.topOfBookHelper.getTopOfBook(orderbook, myFixParser)
			
			#topOfBookHelper.printTopOfBook(topOfBook)
			implier.algorithm.run(topOfBook)
		else:
			# End of file
			break

		elapsedTime = time.time() - startTime
		print('It took ' + str(int(elapsedTime)) + ' seconds to rerun the algorithm.')
