

def parse(line):
    tick={}
    fields = line.split('|')
    tick['TradingSession'] = fields[0]
    tick['Scripcode'] = fields[1]
    tick['BuySell'] = fields[2]
    tick['OrderType'] = fields[3]
    tick['Rate'] = fields[4]
    tick['Qty'] = fields[5]
    tick['AvlQty'] = fields[6]
    tick['Order Timestamp'] = fields[7]
    tick['Retention'] = fields[8]
    tick['Audcode'] = fields[9]
    tick['Order_id'] = fields[10]
    
    if len(fields)!=11:
        print(len(fields))
        
    #if tick['Audcode'] == 'A': 
    #    tick['Action_ id'] = fields[11]
    
    return tick

file = open('C:\workspace\data\BSE\BEN Data Reques_It\Order_Dtss_20110908\dtss_order_20110908')
lines = file.readlines()
file.close()

ticks = []
print('First line is: ' + lines[0])
for line in lines:
    ticks.append(parse(line))
    
for key in ticks[0]:
    print(key + ' ' + ticks[0][key])
