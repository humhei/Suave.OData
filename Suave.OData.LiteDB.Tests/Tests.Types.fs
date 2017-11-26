module Tests.Types

open System
[<CLIMutable>]
type Company=
  {Id :int
   Name :string}   
[<CLIMutable>]    
type EOrder=
  { Id :int
    OrderNumRange :string }   
[<CLIMutable>]    
type Order=
  { Id :int
    Company :Company
    EOrders:EOrder list}
