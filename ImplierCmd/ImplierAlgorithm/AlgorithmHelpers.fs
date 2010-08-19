module AlgorithmHelpers

open DictionaryToList
open System

let rec remove_if l predicate =
    match l with
    | [] -> []
    | x::rest -> if predicate(x) then
                    (remove_if rest predicate)
                 else
                     x::(remove_if rest predicate)

let isSame(spread1, spread2) =
    match spread1 with
    | Spread(securityInfo1, spreadInfo1) -> 
        match spread2 with
        | Spread(securityInfo2, spreadInfo2) when securityInfo1.securityID = securityInfo2.securityID -> true
        | _ -> false
    | _ -> false

let removeSpread(spread, spreads) = 
    remove_if spreads (fun x -> (isSame(x, spread) = true))

let rec computeTradeCost(trade) = 
    match trade with
    | [] -> 0.0
    | security::rest -> 
        match security with
        | Outright(securityInfo, outrightInfo) -> -1.0 * double(securityInfo.side) * securityInfo.price + computeTradeCost(rest)
        | Spread(securityInfo, spreadInfo) -> -1.0 * double(securityInfo.side) * securityInfo.price + computeTradeCost(rest)
        
let printCompletedTrade(trade) = 
    let cost = computeTradeCost(trade)

    if List.length(trade) > 2 && cost >= -1.0 then  
        Console.WriteLine("Found a trade worth " + cost.ToString())

    // later want to see how much this trade is worth
    // if >=0 then do something...
    ()

