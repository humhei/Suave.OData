namespace Suave.OData.Web
open System.Data.Entity
open Suave.OData.Core

[<AutoOpen>]
module EfCrud =

  let addEntity (db : DbContext) add entity =
    add entity |> ignore
    db.SaveChangesAsync() |> Async.AwaitTask

  let inline
    updateEntity<'a when 'a : not struct and 'a : equality and 'a : null>
     (db : DbContext) (dbSet : DbSet<'a>) entity =
        dbSet.Attach(entity) |> ignore
        db.Entry(entity).State <- EntityState.Modified
        db.SaveChangesAsync() |> Async.AwaitTask

  let inline
    findEntityById<'a when 'a : not struct and 'a : equality and 'a : null>
      (dbSet : DbSet<'a>) (id : int) = async {
        try
          let! entity = dbSet.FindAsync(id) |> Async.AwaitTask
          if entity = null then
            return None
          else
            return Some(entity)
        with
        | _ -> return None
      }

  let inline
    resource<'a when 'a : not struct and 'a : equality and 'a : null>
      db name (dbSet : DbSet<'a>) =
        {
          Name = name
          Entities = dbSet
          Add = addEntity db dbSet.Add
          Update = updateEntity db dbSet
          Find = findEntityById dbSet
        }