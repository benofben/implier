module OutrightTrades

open DictionaryToList
open AlgorithmHelpers

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
            | Spread(securityInfo, spreadInfo) when spreadInfo.longUnderlyingMaturityMonthYear = missingMaturityMonthYear && securityInfo.side = 1 && missingSide = 1 -> findTradesWithOutrightsR(spread :: trade, outrights, removeSpread(spread, spreads), spreadInfo.shortUnderlyingMaturityMonthYear, 1)
            | Spread(securityInfo, spreadInfo) when spreadInfo.longUnderlyingMaturityMonthYear = missingMaturityMonthYear && securityInfo.side = -1 && missingSide = -1 -> findTradesWithOutrightsR(spread :: trade, outrights, removeSpread(spread, spreads), spreadInfo.shortUnderlyingMaturityMonthYear, -1)
            | Spread(securityInfo, spreadInfo) when spreadInfo.shortUnderlyingMaturityMonthYear = missingMaturityMonthYear && securityInfo.side = -1 && missingSide = 1 -> findTradesWithOutrightsR(spread :: trade, outrights, removeSpread(spread, spreads), spreadInfo.longUnderlyingMaturityMonthYear, 1)
            | Spread(securityInfo, spreadInfo) when spreadInfo.shortUnderlyingMaturityMonthYear = missingMaturityMonthYear && securityInfo.side = 1 && missingSide = -1 -> findTradesWithOutrightsR(spread :: trade, outrights, removeSpread(spread, spreads), spreadInfo.longUnderlyingMaturityMonthYear, -1)
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