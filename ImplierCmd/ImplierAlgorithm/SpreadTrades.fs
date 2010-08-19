module SpreadTrades

open DictionaryToList
open AlgorithmHelpers

let rec findTradesWithSpreadsR(trade, spreads, missingMaturityMonthYear1, missingSide1, missingMaturityMonthYear2, missingSide2) = 
    //first want to try to match a spread with the missing legs
    // there are two missing legs.  One is the explicitly named one.
    // The other is the shortMaturity for the last trade in the list
    spreads |> List.iter(
        fun spread ->
            match spread with
            | Spread(securityInfo, spreadInfo) when 
                spreadInfo.longUnderlyingMaturityMonthYear = missingMaturityMonthYear1 && securityInfo.side = missingSide1 && 
                spreadInfo.shortUnderlyingMaturityMonthYear = missingMaturityMonthYear2 && securityInfo.side = missingSide2 -> 
                    printCompletedTrade(spread :: trade)
            | Spread(securityInfo, spreadInfo) when 
                spreadInfo.shortUnderlyingMaturityMonthYear = missingMaturityMonthYear1 && securityInfo.side = missingSide1 * -1 &&
                spreadInfo.longUnderlyingMaturityMonthYear = missingMaturityMonthYear2 && securityInfo.side = missingSide2 ->
                    printCompletedTrade(spread :: trade)
            | _ -> ()
    )
        
    //then we want to see if there are any other paths
    spreads |> List.iter(
        fun spread ->
            match spread with
            | Spread(securityInfo, spreadInfo) when spreadInfo.longUnderlyingMaturityMonthYear = missingMaturityMonthYear1 && securityInfo.side = 1 && missingSide1 = 1 -> findTradesWithSpreadsR(spread :: trade, removeSpread(spread, spreads), spreadInfo.shortUnderlyingMaturityMonthYear, 1, missingMaturityMonthYear2, missingSide2)
            | Spread(securityInfo, spreadInfo) when spreadInfo.longUnderlyingMaturityMonthYear = missingMaturityMonthYear1 && securityInfo.side = -1 && missingSide1 = -1 -> findTradesWithSpreadsR(spread :: trade, removeSpread(spread, spreads), spreadInfo.shortUnderlyingMaturityMonthYear, -1, missingMaturityMonthYear2, missingSide2)
            | Spread(securityInfo, spreadInfo) when spreadInfo.shortUnderlyingMaturityMonthYear = missingMaturityMonthYear1 && securityInfo.side = -1 && missingSide1 = 1 -> findTradesWithSpreadsR(spread :: trade, removeSpread(spread, spreads), spreadInfo.longUnderlyingMaturityMonthYear, 1, missingMaturityMonthYear2, missingSide2)
            | Spread(securityInfo, spreadInfo) when spreadInfo.shortUnderlyingMaturityMonthYear = missingMaturityMonthYear1 && securityInfo.side = 1 && missingSide1 = -1 -> findTradesWithSpreadsR(spread :: trade, removeSpread(spread, spreads), spreadInfo.longUnderlyingMaturityMonthYear, -1, missingMaturityMonthYear2, missingSide2)
            | _ -> ()
    )
    ()

let findTradesWithSpreadsOnly(spreads : Security list) =
    spreads |> List.iter(
        fun spread -> 
            match spread with
            | Spread(securityInfo, spreadInfo) -> 
                //always try to match the long leg.  We'll leave the short one hanging until we try to reconnect it when we're done.
                findTradesWithSpreadsR([spread], removeSpread(spread, spreads), spreadInfo.longUnderlyingMaturityMonthYear, securityInfo.side * -1, spreadInfo.shortUnderlyingMaturityMonthYear, securityInfo.side)
            | _ -> ()
    )
    1