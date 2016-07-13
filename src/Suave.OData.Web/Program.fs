// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open Suave.OData.EF

[<EntryPoint>]
let main argv =
    let db = new Db()
    db.People
    |> Seq.iter (fun p -> printfn "%s,%s" p.FirstName p.LastName)
    0 // return an integer exit code
