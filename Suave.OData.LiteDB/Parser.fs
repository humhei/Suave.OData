namespace Suave.OData.LiteDB
open FParsec
open LiteDB

[<RequireQualifiedAccess>]
module ODataParser=
    let private selelctQuery (dbSet:LiteCollection<_>)=
      let str s = pstring s
      let normalCharSnippet=manySatisfy isLetter
      let keyLiteral=normalCharSnippet|>between (str "$") (str "=")
      pipe2 keyLiteral normalCharSnippet (fun key value->
        match key with
        |"select"->dbSet.FindAll()
                   |>Seq.map(fun n->
                   n.GetType().GetProperty(value).GetValue(n))
        |_->Seq.empty
      )
    // let private expandQuery (dbSet:LiteCollection<_>)=
    //   let str s = pstring s
    //   let normalCharSnippet=manySatisfy isLetter
    //   let keyLiteral=normalCharSnippet|>between (str "$") (str "=")
    //   pipe2 keyLiteral normalCharSnippet (fun key value->
    //     match key with
    //     |"expand"->dbSet
    //                |>Seq.map(fun n->
    //                n.GetType().GetProperty(value).GetValue(n))
    //     |_->Seq.empty
    //   )      
    let filter (dbSet:LiteCollection<_>) queryStr=
      match run (selelctQuery dbSet) queryStr with
      |Success(result,_,_)->result
      |Failure(errorMsg,_,_)->failwith (sprintf "Failure: %s" errorMsg)