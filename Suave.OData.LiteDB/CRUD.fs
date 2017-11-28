namespace Suave.OData.LiteDB
open System
open LiteDB
/// A derivative of Suave.OData. Code adapted from Tamizh's origin
//see https://goo.gl/JketNx
[<AutoOpen>]
module EfCrud =
  let findEntityById (find) (id : int) = async {
    try
      let entity = find id 
      return Some(entity)
    with
    | ex ->
      printfn "%A" ex
      return None
  }
  let deleteEntityById findById deleteById (id : int) = async {
    try
      let entity=findById id
      deleteById id|>ignore
      return Some(entity)
    with
    | ex ->
      printfn "%A" ex
      return None
  }
  let addEntity  add entity = async {
    try
      add entity |> ignore
      return Some(entity),exn ""
    with
    | ex ->
      printfn "%A" ex
      return None,ex
  }
  let updateEntity updateById id entity  = async {
    try
      updateById id entity|>ignore
      return Some entity,exn ""
    with
    | ex ->
      printf "%A" ex
      return None,ex
  }  
  let resource<'a when 'a : not struct and
                'a : equality>
                 name (col:LiteCollection<'a>) = 
      let findById:int->'a =BsonValue>>col.FindById   
      let deleteById (id:int) =
        col.Delete(Query.EQ("_id",BsonValue id))
      let updateById (id:int) entity=col.Update(BsonValue id,entity)
      {
        Name = name
        FindById = findEntityById findById
        Add = addEntity col.Insert
        DeleteById = deleteEntityById findById deleteById
        UpdateById = updateEntity updateById
        Entities = col
      }
  