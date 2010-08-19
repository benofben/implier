module Algorithm

open System
open System.Collections.Generic
open DictionaryToList
open OutrightTrades
open SpreadTrades


let findTrades(outrightDictionary : Dictionary<string, (string * double * double * double * double)>, spreadDictionary : Dictionary<string, (string * string * double * double * double * double)>) =
    // First we reformat the dictionaries as nonmutable F# lists
    let outrights = getOutrightList(outrightDictionary)
    let spreads = getSpreadList(spreadDictionary)

    // now we have two kinds of trades to find.
    // Trades with outrights - such as Sep10, Sep10/Dec10, Dec10
    // And, trades with spreads only - such as Sep10/Dec10, Dec10/Feb11, Sep10/Feb11
    ignore(findTradesWithOutrights(outrights, spreads))
    ignore(findTradesWithSpreadsOnly(spreads))

    //Console.WriteLine("Active list is: " + outrights.Length.ToString() + " " + spreads.Length.ToString())
    1

    
    