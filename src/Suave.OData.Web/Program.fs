namespace Suave.OData.Web

open Suave.OData.EF
open Suave
open Suave.Web
open Suave.Http
open Suave.Successful
open Suave.Filters
open Suave.Operators
open Suave.OData.Core

module Main =
  [<EntryPoint>]
  let main argv =
    let db = new Db()
    startWebServer defaultConfig (path "/people" >=> ODataFilter db.People)
    0
