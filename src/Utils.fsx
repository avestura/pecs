namespace PECS

open System

module ConsoleUtils =
    let printColored text color =
        Console.ForegroundColor <- color 
        printf "%s" text
        Console.ResetColor()
        
    let printlnColored text color =
        Console.ForegroundColor <- color
        printfn "%s" text
        Console.ResetColor()
        
    let print text = printf "%s" text
    let println text = printfn "%s" text