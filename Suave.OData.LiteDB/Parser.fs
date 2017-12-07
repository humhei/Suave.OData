namespace Suave.OData.LiteDB
open LiteDB
open Suave.Utils.AsyncExtensions
open System.Collections.Generic
[<RequireQualifiedAccess>]
module ODataParser=
    let private select (dbSet:LiteCollection<_>) value=
        dbSet.FindAll()
            |>Seq.map(fun n->
            n.GetType().GetProperty(value).GetValue(n))
    let private expand (dbSet:LiteCollection<_>) value=
    
        dbSet.Include(value).FindAll()|>Seq.map(box)    
    let private (|QueryAble|QueryDisable|) input= if Seq.isEmpty input then  QueryDisable else QueryAble

    let filter (dbSet:LiteCollection<_>) (querydict:Map<string,string>)=
      match querydict with 
      |QueryDisable -> dbSet.FindAll()|>Seq.map(box)
      |QueryAble -> 
        let k,v=querydict|>Map.toSeq|>Seq.head
        match k with
          |"$select"-> select dbSet v
          |"$expand"-> expand dbSet v
          |_ -> failwith "Unexcepted query"

