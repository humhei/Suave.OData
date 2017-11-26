module Tests.OData

open Expecto
open System.IO
open LiteDB
open LiteDB.FSharp
open Types
open Suave.Testing
open Suave
open Suave.OData.LiteDB
open Json
let useDatabase (f: LiteRepository -> WebPart) = 
    let mapper = FSharpBsonMapper()
    use memoryStream = new MemoryStream()
    use db = new LiteRepository(memoryStream, mapper)
    f db
let odataRouter = 
  useDatabase<| fun db->
    let defaultCompany=
      {Id =0
       Name ="test"}  
    db.Insert(defaultCompany)|>ignore
    resource "odata/login" (db.Database.GetCollection<Company>()) |> OData.CRUD
let runWithConfig = runWith defaultConfig
  
let ODataTests =
  testList "ODataTests" [
    testCase "OData GetById Test" <| fun _ -> 
      let res=
        runWithConfig odataRouter
        |>req GET "odata/login(1)" None
        |>ofJson<Company>
      Expect.equal res.Name  "test" "CLIType DBRef Token Test Corrently"
  ]
