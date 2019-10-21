#load "../Utils.fsx"

namespace PECS
open System
open ConsoleUtils

module Simulator =
    
    type HardcodedInput = { enterTimes:decimal list; serviceTimes:decimal list }
    type Formula = decimal -> float -> decimal
    
    type ServerStatus =
        | Free
        | Busy

    type EventTime =
        | Never
        | Time of decimal
    with override this.ToString() = match this with | Never -> "Never (+∞)" | Time x -> string x
    
    type EventsTimes = { nextEnter:EventTime; nextExit:EventTime }
    
    type SystemState =
       { serverStatus : ServerStatus
         snapshot : int
         timesOfArrival : decimal list
         timeOfLastEvent : decimal
         clock : decimal
         prevClock : decimal
         eventList : EventsTimes
         numberServiced : int
         totalDelay : decimal
         areaUnderQt : decimal
         areaUnderBt : decimal
         enterCount : int
         ``E[S]`` : decimal
         exitCount : int }
       with member state.numberInQueue = state.timesOfArrival.Length
            member state.canGetNextServiceTime = state.enterCount >= state.exitCount
            member state.canStrictlyGetNextServiceTime = state.enterCount > state.exitCount + 1

    
    type TerminationCondition = (SystemState -> bool)

    type SystemInput =
        | Data of HardcodedInput
        | Formula of Formula * lambda:decimal * mu:decimal * Random

    type NextData = decimal * SystemInput
    
    type Configuration =
        { skipData : int; printOutputMod : int option }
    
    let initialState =
        { serverStatus = Free
          snapshot = 0
          timesOfArrival = []
          timeOfLastEvent = 0.m
          clock = 0.m
          prevClock = 0.m
          eventList = {nextEnter = Never; nextExit = Never}
          numberServiced = 0
          totalDelay = 0.m
          areaUnderQt = 0.m
          areaUnderBt = 0.m
          enterCount = 0
          ``E[S]`` = 0.m
          exitCount = 0 }

    let getNextEnter input =
        match input with
        | Data d -> 
            match d.enterTimes with
            | [] -> (-1.m,  Data {d with enterTimes = []})
            | x::xs -> (x, Data {d with enterTimes = xs})
        | Formula (f, lambda, _, r) ->
            let rnd = r.NextDouble()
            f lambda rnd, input

    let getNextService input =
        match input with
        | Data d ->
            match d.serviceTimes with
            | [] -> (-1.m,  Data {d with serviceTimes = []})
            | x::xs -> (x, Data {d with serviceTimes = xs})
        | Formula (f, _, mu, r) ->
            let rnd = r.NextDouble()
            f mu rnd, input
    
    let (|EnterNext|ExitNext|NoEvent|) = function
        | Never, Never -> NoEvent
        | Time a, Never -> EnterNext a
        | Never, Time b -> ExitNext b
        | Time a, Time b when a < b -> EnterNext a
        | Time a, Time b when a > b -> ExitNext b
        | Time a, Time _ -> ExitNext a

    // Increment enter time
    let (>+) (evList:EventsTimes) amount =
        match evList.nextEnter with
        | Never -> { evList with nextEnter = Time amount }
        | Time x -> { evList with nextEnter = x + amount |> Time }
        
    // Increment exit time
    let (+>) (evList:EventsTimes) amount =
        match evList.nextExit with
        | Never -> { evList with nextExit = Time amount }
        | Time x -> { evList with nextExit = x + amount |> Time }
        
    let printSystemState (state:SystemState) (input:SystemInput) =
        printfn "=========== Snapshot %d ============" state.snapshot
        print "Clock: "; printlnColored (string state.clock) ConsoleColor.Yellow
        print "Next Enter: "; printlnColored (string state.eventList.nextEnter) ConsoleColor.Yellow
        print "Next Exit: "; printlnColored (string state.eventList.nextExit) ConsoleColor.Yellow
        print "Area under B(t): "; printlnColored (string state.areaUnderBt) ConsoleColor.Yellow
        print "Area under Q(t): "; printlnColored (string state.areaUnderQt) ConsoleColor.Yellow
        print "Total Delay: "; printlnColored (string state.totalDelay) ConsoleColor.Yellow
        print "Number Services: "; printlnColored (string state.numberServiced) ConsoleColor.Yellow
        let st = state.serverStatus
        print "System Status: "; printlnColored (string st) (if st = Busy then ConsoleColor.Red else ConsoleColor.Green)
        print "Times of Arrival: "; printfn "%A" state.timesOfArrival
        match input with
        | Data x ->
            print "Enter Times: "; printfn "%A" x.enterTimes
            print "Service Times: "; printfn "%A" x.serviceTimes
        | _ -> ()
        printfn "=========== End Snapshot %d ========" state.snapshot
        printfn ""    

    let printFinalResult (state:SystemState) =
        let n = decimal state.numberServiced
        let clk = state.clock
        let Wq = state.totalDelay / n
        let Lq = state.areaUnderQt / clk
        let ro = state.areaUnderBt / clk
        let L = Lq + ro
        let eS = state.``E[S]``  / n
        let W = Wq + eS
        printfn "======= Final Results ======="
        print "  Wq: "; printColored (string Wq) ConsoleColor.Yellow; printlnColored " min/customer" ConsoleColor.Green
        print "  Lq: "; printColored (string Lq) ConsoleColor.Yellow; printlnColored " customers" ConsoleColor.Green
        print "  ro: "; printColored (string ro) ConsoleColor.Yellow; printlnColored " (dimensionless)" ConsoleColor.Green
        print "   L: "; printColored (string  L) ConsoleColor.Yellow; printlnColored " customers" ConsoleColor.Green
        print "E[S]: "; printColored (string eS) ConsoleColor.Yellow; printlnColored " min" ConsoleColor.Green
        print "   W: "; printColored (string  W) ConsoleColor.Yellow; printlnColored " min" ConsoleColor.Green
        printfn ""
    
    let doPrint func i (config:Configuration) =
        match config.printOutputMod with
        | None -> func ()
        | Some x -> if i % x = 0 then func () else (printlnColored "Skip..." ConsoleColor.DarkGray)
    
    let rec runSimulation (state:SystemState) (termination:TerminationCondition) (input:SystemInput) (config:Configuration) =
        doPrint (fun () -> printSystemState state input) state.snapshot config
        let clk, evList = state.clock, state.eventList
        match termination state, (evList.nextEnter, evList.nextExit) with
        | true, _ -> printFinalResult state
        | false, (ent, exit) ->
            match ent, exit with
            | NoEvent ->
                let next, newInput = getNextEnter input
                let newState = { state with
                                    eventList = evList >+ next
                                    snapshot = state.snapshot + 1 }

                runSimulation newState termination newInput config

            | EnterNext nextEventTime ->
                let nextEnt, newEntInput = getNextEnter input
                let isFree = (state.serverStatus = Free)
                let nextServe, newServeInput = getNextService newEntInput
                let canExit = state.canGetNextServiceTime
                let applyServe (e:EventsTimes) =
                    if not canExit then {e with nextExit = Never } else if isFree then e +> nextEventTime +> nextServe else e
                let newAreaBtIncrement = if state.serverStatus = Busy then nextEventTime - clk |> decimal else 0.m
                let newAreaQtIncrement = decimal state.numberInQueue * (nextEventTime - clk)
                let newState = { state with
                                   clock = nextEventTime
                                   prevClock = clk
                                   eventList = (evList >+ nextEnt) |> applyServe
                                   numberServiced = if isFree then state.numberServiced + 1 else state.numberServiced
                                   areaUnderBt = state.areaUnderBt + newAreaBtIncrement
                                   areaUnderQt = state.areaUnderQt + newAreaQtIncrement
                                   timesOfArrival = if isFree then state.timesOfArrival else state.timesOfArrival@[nextEventTime]
                                   serverStatus = Busy
                                   ``E[S]`` = state.``E[S]`` + (if isFree && canExit then nextServe else 0.m)
                                   snapshot = state.snapshot + 1
                                   enterCount = state.enterCount + 1 }
                
                let newInput = if isFree && canExit then newServeInput else newEntInput 
                
                runSimulation newState termination newInput config
                
            | ExitNext nextEventTime -> 
                let next, newServeInput = getNextService input
                let canGetNext = state.canStrictlyGetNextServiceTime
                let newAreaBtIncrement = if state.serverStatus = Busy then nextEventTime - clk else 0.m
                let newAreaQtIncrement = decimal state.numberInQueue * (nextEventTime - clk)
                let newArrival, newServerStatus, newNumberServiced, popedValue =
                    match state.timesOfArrival, state.numberServiced with
                    | [], n -> [], Free, n, None
                    | x::xs, n -> xs, Busy, n + 1, Some x
                               
                let newState = { state with
                                   clock = nextEventTime
                                   prevClock = clk
                                   eventList = if canGetNext then evList +> next else {evList with nextExit = Never}
                                   timesOfArrival = newArrival
                                   totalDelay = state.totalDelay + (if popedValue.IsSome then (nextEventTime - popedValue.Value) else 0.m)
                                   areaUnderBt = state.areaUnderBt + newAreaBtIncrement
                                   areaUnderQt = state.areaUnderQt + newAreaQtIncrement
                                   serverStatus = newServerStatus
                                   numberServiced = newNumberServiced
                                   snapshot = state.snapshot + 1
                                   ``E[S]`` = state.``E[S]`` + if canGetNext then next else 0.m
                                   exitCount = state.exitCount + 1 }

                let newInput = if canGetNext then newServeInput else input                
                
                runSimulation newState termination newInput config

