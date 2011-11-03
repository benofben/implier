file = open('C:\\workspace\\data\\CME\\20110927\\secdef.dat_20110927\\secdef.dat')

while file:
        line = file.readline()
        print(line)
        break
       
file.close()
