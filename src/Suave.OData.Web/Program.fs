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
    let people = {
      Name = "people"
      DbSet = db.People
      SaveChanges = db.SaveChangesAsync
    }
    startWebServer defaultConfig (OData.CRUD people)
    0
