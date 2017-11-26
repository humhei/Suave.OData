module Runner

open Expecto
open Expecto.Logging
open Tests.OData

let testConfig =  
    { Expecto.Tests.defaultConfig with 
         parallelWorkers = 1
         verbosity = LogLevel.Debug }

let liteDbTests = 
    testList "All tests" [  
        ODataTests
    ]


[<EntryPoint>]
let main argv = runTests testConfig liteDbTests