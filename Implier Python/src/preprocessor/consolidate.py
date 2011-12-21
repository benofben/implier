import os
import fix.fixParser as fixParser

def consolidateDays():
	#securitiesDirectoryName = 'I:\\nym\\'
	securitiesDirectoryName = 'C:\\Users\\ben\\Desktop\\si1week\\'
	
	consolidateDay(securitiesDirectoryName, securitiesDirectoryName + '..\\output.txt')

# Go through all the files for a given day and writes them to a single file ordered by tick time.
def consolidateDay(securitiesDirectoryName, outputFilename):
	myFixParser = fixParser.fixParser()
	securityFiles = []
	
	for unused_subdirNames, unused_dirNames, fileNames in os.walk(securitiesDirectoryName):
		for fileName in fileNames:
			securityFile = open(securitiesDirectoryName + '\\' + fileName)
			d={}
			d['file']=(securityFile)
			securityFiles.append(d)
	print('Done opening files.')

	outputFile = open(outputFilename, 'w')
	
	# read the first line of every file
	for securityFile in securityFiles:
		securityFile['line']=securityFile['file'].readline()
		fields = myFixParser.getFields(securityFile['line'])
		securityFile['sendingTime'] = myFixParser.getField(52, fields)
			
	# now loop through until all the files are read
	while(len(securityFiles)>0):
		securityFiles = sorted(securityFiles, key=lambda x: x['sendingTime'])
		
		# write the oldest line to the output file if it's useful (an order book update)
		if needToPrintLine(securityFiles[0]['line'], myFixParser):
			outputFile.write(securityFiles[0]['line'])
		
		# read a replacement line from the appropriate file
		securityFiles[0]['line']=securityFiles[0]['file'].readline()
		
		# check if we hit EOF
		if len(securityFiles[0]['line'])==0:
			securityFiles[0]['file'].close()
			securityFiles.pop(0)
		else:
			fields = myFixParser.getFields(securityFiles[0]['line'])
			securityFiles[0]['sendingTime'] = myFixParser.getField(52, fields)
		
	outputFile.close()

# returns True if line is useful, False if it can be dropped.
def needToPrintLine(message, myFixParser):
	fields = myFixParser.getFields(message)
	fields = myFixParser.lookupFields(fields)
	msgType = myFixParser.getField(35, fields)
	
	# skip SecurityStatus
	if msgType == 'f':
		pass
	elif msgType != 'X':
		print('Got a message I do not know how to deal with of type: ' + msgType)
	else:
		mdEntries = myFixParser.getMDEntries(fields)
		for mdEntry in mdEntries:				
			# I'm just going to treat exchange implied prices as standard prices.  Maybe drop this out later.
			
			mdEntryType = myFixParser.getFieldandLookup(269, mdEntry)
			if mdEntryType=='BID' or mdEntryType=='OFFER':
				return True
			elif mdEntryType=='SETTLEMENT_PRICE':
				pass
			elif mdEntryType=='SIMULATED_SELL_PRICE':
				pass
			elif mdEntryType=='SIMULATED_BUY_PRICE':
				pass
			elif mdEntryType=='TRADE':
				pass
			elif mdEntryType=='TRADE_VOLUME':
				pass
			elif mdEntryType=='OPEN_INTEREST':
				pass
			elif mdEntryType=='OPENING_PRICE':
				pass
			elif mdEntryType=='TRADING_SESSION_HIGH_PRICE':
				pass
			elif mdEntryType=='TRADING_SESSION_LOW_PRICE':
				pass
			else:
				print('Got an mdEntryType I do not know how to deal with ' + str(mdEntryType))
	
	return False
