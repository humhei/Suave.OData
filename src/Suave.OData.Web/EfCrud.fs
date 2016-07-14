namespace Suave.OData.Web
open System.Data.Entity
open Suave.OData.Core

[<AutoOpen>]
module EfCrud =

  let addEntity (db : DbContext) add entity =
    add entity |> ignore
    db.SaveChangesAsync() |> Async.AwaitTask

  let updateEntity (db : DbContext) attach entity =
    db.Entry(entity).State <- EntityState.Modified
    db.SaveChangesAsync() |> Async.AwaitTask

  let findEntityById find (id : int) = async {
    try
      let! entity = find id |> Async.AwaitTask
      if isNull entity then
        return None
      else
        return Some(entity)
    with
    | ex ->
      printfn "%A" ex
      return None
  }

  let deleteEntityById (db : DbContext) find remove (id : int) = async {
      try
        let! entity = find id |> Async.AwaitTask
        if isNull entity then
          return None
        else
          remove entity |> ignore
          let! _ = db.SaveChangesAsync() |> Async.AwaitTask
          return Some(entity)
      with
      | ex ->
        printfn "%A" ex
        return None
    }

  let inline
    resource<'a when 'a : not struct and 'a : equality and 'a : null>
      db name (dbSet : DbSet<'a>) =
        {
          Name = name
          Entities = dbSet
          Add = addEntity db dbSet.Add
          Update = updateEntity db dbSet.Attach
          FindById = findEntityById dbSet.FindAsync
          DeleteById = deleteEntityById db dbSet.FindAsync dbSet.Remove
        }