import fix
myFIX = fix.fix()

file = open('I:\\20111026\\secdef.dat')
outputFile = open('c:\\secdefout.txt', 'w')

i=0
while file:
	message = file.readline()
	'''
	fields = myFIX.getFields(message)
	fields = myFIX.lookupFields(fields)	
	
	for field in fields:
		outputFile.write(str(field) + '\n')
	outputFile.write('\n\n')
	
	#messageType = fix.getFixField(35, fields)
	'''
	i=i+1
	
print(i)
file.close()
outputFile.close
