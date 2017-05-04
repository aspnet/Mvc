namespace FSharpWebSite.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc

type HomeController () =
    inherit Controller()

    member this.Index () =
        this.View()
