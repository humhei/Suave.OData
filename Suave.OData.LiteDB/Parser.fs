namespace Suave.OData.LiteDB
open FParsec

[<RequireQualifiedAccess>]
module ODataParser=
    let private itemLiterral (dbSet:seq<_>)=
      let str s = pstring s
      let normalCharSnippet=manySatisfy isLetter
      let keyLiteral=normalCharSnippet|>between (str "$") (str "=")
      pipe2 keyLiteral normalCharSnippet (fun key value->
        match key with
        |"select"->dbSet
                   |>Seq.map(fun n->
                   n.GetType().GetProperty(value).GetValue(n))
        |_->Seq.empty
      )
    let filter dbSet queryStr=
      match run (itemLiterral dbSet) queryStr with
      |Success(result,_,_)->result
      |Failure(errorMsg,_,_)->failwith (sprintf "Failure: %s" errorMsg)