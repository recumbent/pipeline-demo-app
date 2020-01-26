// Learn more about F# at http://fsharp.org

open System
open pipeline_demo_app

[<EntryPoint>]
let main argv =
    let content = aws.readS3 "test-bucket" "quote-01.json"
    aws.writeS3 "test-bucket" (DateTime.UtcNow.ToString("HH-mm-ss-ff")) "Test Content To Write"
    printfn "%s" content
    0 // return an integer exit code
