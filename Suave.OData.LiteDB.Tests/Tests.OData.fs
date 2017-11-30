module Tests.OData

open Expecto
open System.IO
open LiteDB
open LiteDB.FSharp
open Types
open Suave.Testing
open Suave
open Suave.OData.LiteDB
open Suave.OData.LiteDB.Json
open System.Net.Http
open LiteDB.FSharp.Help
open FParsec.Internals
let pass() = Expect.isTrue true "passed"
let fail() = Expect.isTrue false "failed"
//set port mutable to run test in parallel
let mutable port=9000    
let runTestCtx()= 
  let odataRouter() = 
    let useDatabase (f: LiteRepository -> WebPart) = 
      let mapper = FSharpBsonMapper()
      mapper.Entity<Order>().DbRef(toLinq(<@fun c->c.Company@>))|>ignore
      mapper.Entity<Order>().DbRef(toLinq(<@fun c->c.EOrders@>))|>ignore  
      let memoryStream = new MemoryStream()
      let db = new LiteRepository(memoryStream, mapper)
      f db    
    useDatabase<| fun db->
      let c1={Id=1;Name="c1"}
      db.Insert(c1)|>ignore
      db.Insert({Id=2;Name="c2"})|>ignore
      db.Insert({Id=1;Company=c1;EOrders=[]})|>ignore
      choose[
         resource "odata/company" (db.Database.GetCollection<Company>()) |> OData.CRUD
         resource "odata/order" (db.Database.GetCollection<Order>()) |> OData.CRUD
      ]
  port<-port+1
  runWith 
    {defaultConfig with 
      bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" port ]} <|odataRouter()
let toStringContent entity=entity|>toJson|>fun args->new StringContent(args)|>Some
let (|IsEmpty|IsNonEmpty|) (input:seq<_>)=
  if Seq.isEmpty input then IsEmpty else IsNonEmpty

let ODataTests =
  testList "ODataTests" [
    testCase "OData GetEntityById Test" <| fun _ -> 
       runTestCtx()
       |>req GET "odata/company(1)" None
       |>ofJson<Company>
       |>function | {Id=1;Name="c1"}->pass()
                  | _->fail()
    testCase "OData GetEntities Test" <| fun _ -> 
        runTestCtx()
        |>req GET "odata/company" None
        |>ofJson<list<Company>> 
        |>function |IsNonEmpty->pass()
                   |IsEmpty->fail()
    testCase "OData Add Entity Test" <| fun _ -> 
      runTestCtx()
      |>req POST "odata/company" (toStringContent {Id=3;Name="newCompany"})
      |>ofJson<Company>
      |>function |{Id=3;Name="newCompany"}->pass()
                 |_->fail()
    testCase "OData Delete Entity Test" <| fun _ -> 
        runTestCtx()
        |>req DELETE "odata/company(2)" None
        |>ofJson<Company>
        |>function | {Id=2;Name="c2"}->pass()
                   | _->fail()
    testCase "OData Update Entity Test" <| fun _ -> 
      runTestCtx()
        |>req PUT "odata/company(2)" (toStringContent {Id=2;Name="updatedCompany"}  )
        |>ofJson<Company>
        |>function | {Id=2;Name="updatedCompany"} ->pass()
                   | _->fail()
    testCase "OData  $select Query Test" <| fun _ -> 
      runTestCtx()
        |>reqQuery GET "odata/company" "$select=Name"
        |>ofJson<list<string>> 
        |>function |IsNonEmpty->pass()
                   |IsEmpty->fail()
         
    testCase "OData  $expand entity Query Test" <| fun _ -> 
      runTestCtx()
        |>reqQuery GET "odata/order" "$expand=Company"
        |>ofJson<Order list> 
        |>Seq.forall(fun c-> isNotNull (box c.Company))
        |>function |true->pass()
                   |false->fail()
  ]