namespace Suave.OData.EF
open System.Data.Entity

type Db () =
  inherit DbContext("MyDb")

  [<DefaultValue>]
  val mutable people : DbSet<People>
  member public this.People
    with get() = this.people
    and set v = this.people <- v