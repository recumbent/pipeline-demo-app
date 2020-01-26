// Learn more about F# at http://fsharp.org

open System
open pipeline_demo_app

[<EntryPoint>]
let main argv =
    let paramName = """/demo/quote/2/secret"""
    aws.writeSSM paramName "Really secret secret"

    let content = aws.readS3 "test-bucket" "quote-01.json"
    aws.writeS3 "test-bucket" (DateTime.UtcNow.ToString("HH-mm-ss-ff")) "Test Content To Write"
    
    let param = aws.readSSM paramName

    printfn "%s" content

    printfn "Secret: %O" param

    let pathParams = aws.readSSMPath """/demo"""
    
    printfn "Params: %A" pathParams.Parameters 

    0 // return an integer exit code
