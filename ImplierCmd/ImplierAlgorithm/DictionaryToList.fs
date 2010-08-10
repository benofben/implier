module DictionaryToList

open System
open System.Collections.Generic

let rec addToOutrights(list, outrightMapEnumerator : IEnumerator<string * string * double * double * double * double >) = 
    if outrightMapEnumerator.MoveNext() then 
        let newList = 
            match outrightMapEnumerator.Current with
            | (securityID, maturityMonthYear, bidPrice, bidSize, askPrice, askSize) when bidSize<>0.0 && askSize<>0.0 -> (securityID, maturityMonthYear, "Sell", bidPrice, bidSize) :: (securityID, maturityMonthYear, "Buy", askPrice, askSize) :: list
            | (securityID, maturityMonthYear, bidPrice, bidSize, _, _) when bidSize<>0.0 -> (securityID, maturityMonthYear, "Sell", bidPrice, bidSize) :: list
            | (securityID, maturityMonthYear, _, _, askPrice, askSize) when askSize<>0.0 -> (securityID, maturityMonthYear, "Buy", askPrice, askSize) :: list
            | _ -> list
        addToOutrights(newList, outrightMapEnumerator)
    else
        list

let rec addToSpreads(list, spreadMapEnumerator : IEnumerator<string * string * string * double * double * double * double >) = 
    if spreadMapEnumerator.MoveNext() then 
        let newList = 
            match spreadMapEnumerator.Current with
            | (securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, bidPrice, bidSize, askPrice, askSize) when bidSize<>0.0 && askSize<>0.0 -> (securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, "Sell", bidPrice, bidSize) :: (securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, "Buy", askPrice, askSize) :: list
            | (securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, bidPrice, bidSize, _, _) when bidSize<>0.0 -> (securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, "Sell", bidPrice, bidSize) :: list
            | (securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, _, _, askPrice, askSize) when askSize<>0.0 -> (securityID, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, "Buy", askPrice, askSize) :: list
            | _ -> list
        addToSpreads(newList, spreadMapEnumerator)
    else
        list

let getOutrightTuple(key, value) = 
    let maturityMonthYear, bidPrice, bidSize, askPrice, askSize = value
    (key, maturityMonthYear, bidPrice, bidSize, askPrice, askSize)

let getSpreadTuple(key, value) = 
    let longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, bidPrice, bidSize, askPrice, askSize = value
    (key, longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, bidPrice, bidSize, askPrice, askSize)

let getOutrightList(outrights : Dictionary<string, (string * double * double * double * double)>) =
    let outrightMap = outrights |> Seq.map (fun kvp -> getOutrightTuple(kvp.Key, kvp.Value))
    let mutable outrightMapEnumerator = outrightMap.GetEnumerator()
    addToOutrights([], outrightMapEnumerator)

let getSpreadList(spreads : Dictionary<string, (string * string * double * double * double * double)>) =
    let spreadMap = spreads |> Seq.map (fun kvp -> getSpreadTuple(kvp.Key, kvp.Value))
    let mutable spreadMapEnumerator = spreadMap.GetEnumerator()
    addToSpreads([], spreadMapEnumerator)