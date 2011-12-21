import os
import fix

# Go through all the files for a given day and writes them to a single file ordered by tick time.

def consolidateDay():
	myFIX = fix.fix()
	securityFiles = []
	
	#securitiesDirectoryName = 'I:\\20111002HG\\'
	securitiesDirectoryName = 'I:\\20111002NYMFUT\\'	
	for unused_subdirNames, unused_dirNames, fileNames in os.walk(securitiesDirectoryName + 'raw\\'):
		for fileName in fileNames:
			securityFile = open(securitiesDirectoryName + 'raw\\' + fileName)
			d={}
			d['file']=(securityFile)
			securityFiles.append(d)
	print('Done opening files.')

	outputFilename = securitiesDirectoryName + 'consolidated.txt'
	outputFile = open(outputFilename, 'w')
	
	# read the first line of every file
	for securityFile in securityFiles:
		securityFile['line']=securityFile['file'].readline()
		fields = myFIX.getFields(securityFile['line'])
		securityFile['sendingTime'] = myFIX.getField(52, fields)
			
	# now loop through until all the files are read
	while(len(securityFiles)>0):
		securityFiles = sorted(securityFiles, key=lambda x: x['sendingTime'])
		
		# write the oldest line to the output file
		outputFile.write(securityFiles[0]['line'])
		
		# read a replacement line from the appropriate file
		securityFiles[0]['line']=securityFiles[0]['file'].readline()
		
		# check if we hit EOF
		if len(securityFiles[0]['line'])==0:
			securityFiles[0]['file'].close()
			securityFiles.pop(0)
		else:
			fields = myFIX.getFields(securityFiles[0]['line'])
			securityFiles[0]['sendingTime'] = myFIX.getField(52, fields)
		
	outputFile.close()

consolidateDay()
