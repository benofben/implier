module Algorithm

open System
open System.Collections.Generic
open DictionaryToList

let findTrades(outrightDictionary : Dictionary<string, (string * double * double * double * double)>, spreadDictionary : Dictionary<string, (string * string * double * double * double * double)>) =
    // first we reformat the dictionaries as nonmutable F# lists
    let outrights= getOutrightList(outrightDictionary)
    let spreads= getSpreadList(spreadDictionary)

    // now we have two kinds of trades to find.

    // First, trades with outrights - such as Sep10, Sep10/Dec10, Dec10

    // Now, trades with spreads only - such as Sep10/Dec10, Dec10/Feb11, Sep10/Feb11
   
    1

    
    