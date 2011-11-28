def getFixFields(message):
	parts = message.split('')
	fields=[]
	for part in parts:
		if part!='\n':
			field={}
			(tag,value) = part.split('=')
			field['tag'] = int(tag)
			field['value'] = value
			fields.append(field)
	return fields

def getFixField(tag, fields):   
	for field in fields:
		if field['tag']==tag:
			return field['value']
	return None

def run():
	file = open('C:\workspace\data\BM&F\MarketData_201105\MarketDataGather_201105_26a31\MarketDataGather_20110526.txt')
	for message in file:
		fields = getFixFields(message)
		
		messageType=getFixField(35, fields)
		if messageType!='X':
			print(messageType)
			
	file.close()
	
run()
