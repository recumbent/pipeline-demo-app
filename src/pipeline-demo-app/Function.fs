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


type Function() =
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    member __.FunctionHandler (input: Request.Root) (_: ILambdaContext) =
        Response.Root(input.ClientId, input.QuoteId, "sample quote")