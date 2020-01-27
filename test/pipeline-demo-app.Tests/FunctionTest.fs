namespace pipeline_demo_app.Tests


open Xunit
open Amazon.Lambda.TestUtilities

open pipeline_demo_app


module FunctionTest =

    // Stub functions
    let ReadFromCache clientId quoteId = 
        match clientId with
        | 1 -> Some(Response.Root(clientId, quoteId, "cached quote"))
        | _ -> None

    let FetchFromApi clientId quoteId = 
        Some(Response.Root(clientId, quoteId, "api quote"))

    let DontCallMe _ _ =
        failwith "Shouldn't have been called" 

    [<Fact>]
    let ``Invoke Quote Lambda Function``() =
        // Invoke the lambda function handler and confirm the string was upper cased.
        let lambdaFunction = helpers.Handler ReadFromCache DontCallMe
        let context = TestLambdaContext()
        let request = { ClientId = 1; QuoteId = 2 }
        let result = lambdaFunction request // context

        Assert.Equal(request.ClientId, result.ClientId)
        Assert.Equal(request.QuoteId, result.QuoteId)
        Assert.Equal("cached quote", result.Text)

    [<Fact>]
    let ``Invoke Quote Lambda Function for no cache``() =
        // Invoke the lambda function and confirm the string was upper cased.
        let lambdaFunction = helpers.Handler ReadFromCache FetchFromApi
        let context = TestLambdaContext()
        let request = { ClientId = 2; QuoteId = 1 }
        let result = lambdaFunction request // context

        Assert.Equal(request.ClientId, result.ClientId)
        Assert.Equal(request.QuoteId, result.QuoteId)
        Assert.Equal("api quote", result.Text)

    // let ``Data in cache should not call api``
    // let ``Data in cache should not call SSM``
    // let ``No data in cache should call api with parans``

    [<EntryPoint>]
    let main _ = 0
