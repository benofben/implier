import fixParser
import fixFileReader
import topOfBookHelper
import algorithm

def run():
	inputFilename = 'C:\\implierData\\20111002HG\\consolidated.txt'
	#inputFilename ='C:\\implierData\\20111002NYMFUT\\consolidated.txt'
	myFixParser = fixParser.fixParser()
	myFixFileReader = fixFileReader.fixFileReader(inputFilename, myFixParser)

	while(True):
		orderbook = myFixFileReader.updateOrderBookWithNextLine()
		if orderbook:			
			topOfBook = topOfBookHelper.getTopOfBook(orderbook, myFixParser)
			#topOfBookHelper.printTopOfBook(topOfBook)
			algorithm.run(topOfBook)
		else:
			break

run()