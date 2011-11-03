def parse(line):
    tick={}
    tick['CommodityCode'] = line[0:2]
    tick['ContractType'] = line[3]
    tick['MonthCode'] = line[5]
    tick['DeliveryYear'] = line[7:11]
    tick['StrikePrice'] = line[12:19]
    tick['TradeDate'] = line[20:28]
    tick['Logtime'] = line[29:35]
    tick['PriceIndicator'] = line[36]
    tick['Price'] = line[38:48]
    tick['MessageCode'] = line[49]
    tick['AmendCode'] = line[51]
    tick['Volume'] = line[53:58]
    return tick

file = open('C:\\workspace\\data\\SGX\\WEBPXTICK_DT-20110916\\tmp\\WEBPXTICK_DT-20110916.txt')
lines = file.readlines()
file.close()


#First line is header information
firstLine=True
ticks = []
print('Second line is: ' + lines[1])

for line in lines:
    if firstLine==0:
        firstLine = False
        pass
    else:    
        tick = parse(line)
        
        if tick['ContractType']=='F':
            ticks.append(tick)

print('I have ' + str(len(ticks)) + ' futures ticks.')

commodityCodes={}
for tick in ticks:
    commodityCodes[tick['CommodityCode']]=0   

print('I have ' + str(len(commodityCodes)) + ' different futures commodity codes.')
    
securityID={}
for tick in ticks:
    securityID[tick['CommodityCode']+tick['MonthCode']+tick['DeliveryYear']]=0   
print('I have ' + str(len(securityID)) + ' different futures securityIDs.')