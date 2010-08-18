module Algorithm

open System
open System.Collections.Generic
open DictionaryToList
    
let printCompletedTrade(trade) = 
    // later want to see how much this trade is worth
    // if >=0 then do something...
    ()

let rec findTradesWithOutrightsR(trade, outrights, spreads, missingMaturityMonthYear, missingSide) = 

    //first want to try to match an outright with the missing leg
    outrights |> List.iter(
        fun outright ->
            match outright with
            | Outright(securityInfo, outrightInfo) when outrightInfo.maturityMonthYear = missingMaturityMonthYear && securityInfo.side = missingSide -> printCompletedTrade(outright :: trade)
            | _ -> ()
    )
        
    //then we want to see if there are any other paths
    spreads |> List.iter(
        fun spread ->
            match spread with
            | Spread(securityInfo, spreadInfo) when spreadInfo.longUnderlyingMaturityMonthYear = missingMaturityMonthYear && securityInfo.side = 1 && missingSide = 1 -> findTradesWithOutrightsR(trade, outrights, spreads, missingMaturityMonthYear, missingSide)
            | Spread(securityInfo, spreadInfo) when spreadInfo.longUnderlyingMaturityMonthYear = missingMaturityMonthYear && securityInfo.side = -1 && missingSide = -1 -> findTradesWithOutrightsR(trade, outrights, spreads, missingMaturityMonthYear, missingSide)
            | Spread(securityInfo, spreadInfo) when spreadInfo.shortUnderlyingMaturityMonthYear = missingMaturityMonthYear && securityInfo.side = -1 && missingSide = 1 -> findTradesWithOutrightsR(trade, outrights, spreads, missingMaturityMonthYear, missingSide)
            | Spread(securityInfo, spreadInfo) when spreadInfo.shortUnderlyingMaturityMonthYear = missingMaturityMonthYear && securityInfo.side = 1 && missingSide = -1 -> findTradesWithOutrightsR(trade, outrights, spreads, missingMaturityMonthYear, missingSide)
            | _ -> ()
    )
    ()

let findTradesWithOutrights(outrights : Security list, spreads : Security list) =
    outrights |> List.iter(
        fun outright -> 
            match outright with
            | Outright(securityInfo, outrightInfo) -> findTradesWithOutrightsR([outright], outrights, spreads, outrightInfo.maturityMonthYear, securityInfo.side * -1)
            | _ -> ()
    )
    1
    
let findTradesWithSpreadsOnly(spreads) = 
    1

let findTrades(outrightDictionary : Dictionary<string, (string * double * double * double * double)>, spreadDictionary : Dictionary<string, (string * string * double * double * double * double)>) =
    // First we reformat the dictionaries as nonmutable F# lists
    let outrights = getOutrightList(outrightDictionary)
    let spreads = getSpreadList(spreadDictionary)

    // now we have two kinds of trades to find.
    // Trades with outrights - such as Sep10, Sep10/Dec10, Dec10
    // And, trades with spreads only - such as Sep10/Dec10, Dec10/Feb11, Sep10/Feb11
    ignore(findTradesWithOutrights(outrights, spreads))
    ignore(findTradesWithSpreadsOnly(spreads))

    Console.WriteLine("Active list is: " + outrights.Length.ToString() + " " + spreads.Length.ToString())
    1

    
    