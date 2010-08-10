#light
namespace ImplierAlgorithm.SpreadMatrix
open System
open Algorithm
open System.Collections.Generic

type SpreadMatrix() = 
    let mutable outrightDictionary : Dictionary<string, (string * double * double * double * double)> = new Dictionary<string, (string * double * double * double * double)>()
    let mutable spreadDictionary : Dictionary<string, (string * string * double * double * double * double)> = new Dictionary<string, (string * string * double * double * double * double)>()

    member this.CreateOutright(securityID : string, maturityMonthYear : string) = 
        outrightDictionary.Add(securityID, (maturityMonthYear, 0.0, 0.0, 0.0, 0.0))

    member this.CreateSpread(securityID : string, longUnderlyingMaturityMonthYear: string, shortUnderlyingMaturityMonthYear : string) = 
        spreadDictionary.Add(securityID, (longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, 0.0, 0.0, 0.0, 0.0))

    member x.Update(securityID : string, bidPrice : double, bidSize : double, askPrice : double, askSize : double) =
        let result, value = outrightDictionary.TryGetValue(securityID)
        if result then 
            let maturityMonthYear, _, _, _, _ = value
            outrightDictionary.[securityID] <- (maturityMonthYear, bidPrice, bidSize, askPrice, askSize)
        
        let result, value = spreadDictionary.TryGetValue(securityID)
        if result then
            let longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, _, _, _, _ = value
            spreadDictionary.[securityID] <- (longUnderlyingMaturityMonthYear, shortUnderlyingMaturityMonthYear, bidPrice, bidSize, askPrice, askSize)

        findTrades(outrightDictionary, spreadDictionary)

