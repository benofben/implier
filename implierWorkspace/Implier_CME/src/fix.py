import xml.dom.minidom

class fix:	
	def __init__(self):
		self.FIXDictionary = self.getFIXDictionary()
	
	def getField(self, tag, fields):   
		for field in fields:
			if field['tag']==tag:
				return field['value']
		return None	

	def getFields(self, message):
		parts = message.split('')
		fields=[]
		for part in parts:
			if part!='\n':
				field={}
				(tag,value) = part.split('=')
				field['tag'] = tag
				field['value'] = value
				fields.append(field)
		return fields
	
	def lookupField(self, field):
		newfield = {}	
		newfield['tag'] = field['tag']
		newfield['value'] = field['value']  
		
		try:
			newfield['name']=self.FIXDictionary[field['tag']]['name']
		except KeyError:
			pass
		
		try:
			newfield['value2']=self.FIXDictionary[field['tag']][field['value']]
		except KeyError:
			pass
				
		return newfield

	def lookupFields(self, fields):
		newFields = []
		for field in fields:
			newFields.append(self.lookupField(field))
		
		return newFields

	def getFIXDictionary(self):
		# gives a dictionary that looks like:
		#	d[fix field number]['name'] = fix field name
		#	d[fix field number][field value] = fix field value description
		d={}
		
		fixDictionaryFilename = 'C:\\implierWorkspace\\Implier_CME\\FIX50SP1.xml'
		
		fixDictionaryFile = open(fixDictionaryFilename)
		fixDictionaryXMLString = fixDictionaryFile.read()
		fixDictionaryFile.close()

		doc = xml.dom.minidom.parseString(fixDictionaryXMLString)
		fields = doc.getElementsByTagName('field')
		for field in fields:
			number = field.getAttribute('number')
			name =  field.getAttribute('name')
			d[number] = {}
			d[number]['name'] = name
			
			values = field.getElementsByTagName('value')
			for value in values:
				description = value.getAttribute('description')
				enum = value.getAttribute('enum')
				d[number][enum] = description

		return d