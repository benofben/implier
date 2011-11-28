import os

# Consolidate the HG files for a 2011 10 26 into a single file

securitiesDirectoryName = 'I:\\20111026HG\\raw\\'
	
def consolidateDay(exchange):
	securityFiles = []
	
	for unused_subdirNames, unused_dirNames, fileNames in os.walk(securitiesDirectoryName + exchange + '\\'):
		for fileName in fileNames:
			securityFile = open(securitiesDirectoryName + exchange + '\\' + fileName)
			securityFiles.append(securityFile)
	print('Done opening files for exchange ' + exchange)

	for securityFile in securityFiles:
		securityFile.close()
	print('Done closing files for exchange ' + exchange)

consolidateDay()
