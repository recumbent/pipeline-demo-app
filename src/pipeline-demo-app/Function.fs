namespace pipeline_demo_app

open Amazon.Lambda.Core

open System
open FSharp.Data

type Request = JsonProvider<""" { "clientId": 1, "quoteId": 2 } """>
type Response = JsonProvider<""" { "clientId": 1, "quoteId": 2, "text": "this is a quote" } """>
type ApiResponse = JsonProvider<""" { "author": "Douglas Adams", "quote": "this is a quote" } """>

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
()

module helpers =
    let ReadFromCache clientId quoteId = 
        match clientId with
        | 1 -> Some(Response.Root(clientId, quoteId, "cached quote"))
        | _ -> None

    let FetchFromApi clientId quoteId = 
        Some(Response.Root(clientId, quoteId, "api quote"))

type Function() =
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    member __.FunctionHandler (input: Request.Root) (_: ILambdaContext) =
        match helpers.ReadFromCache input.ClientId input.QuoteId with
        | Some(quote) -> quote
        | None ->
            (helpers.FetchFromApi input.ClientId input.QuoteId).Value
