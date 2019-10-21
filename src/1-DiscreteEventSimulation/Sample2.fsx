#load "Simulator.fsx"

open System
open PECS.Simulator
let formula lambda (rand:float) = (-1.M / lambda) * decimal (log rand)
let inputGenerator = (formula, 0.9m, 1.0m, Random()) |> Formula
let terminationCondition (state:SystemState) = (state.numberServiced = 1000)
let config = { skipData = 0; printOutputMod = Some 10 }

runSimulation initialState terminationCondition inputGenerator config
