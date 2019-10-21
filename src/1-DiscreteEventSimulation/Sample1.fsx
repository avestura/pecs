#load "Simulator.fsx"

open System
open PECS.Simulator

let mapDec x = x |> List.map decimal

let input = Data {
    enterTimes   = mapDec [0.4; 1.2; 0.5; 1.7; 0.2; 1.6; 0.2; 1.4; 1.9]
    serviceTimes = mapDec [2.0; 0.7; 0.2; 1.1; 3.7; 0.6]
}
let terminationCondition (state:SystemState) = (state.numberServiced = 6 || state.snapshot = 30)
let config = { skipData = 0; printOutputMod = None }

runSimulation initialState terminationCondition input config