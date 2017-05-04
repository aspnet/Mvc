namespace FSharpWebSite

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection


type Startup () =

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc() |> ignore

    member this.Configure(app: IApplicationBuilder) =
        app.UseDeveloperExceptionPage() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseMvcWithDefaultRoute() |> ignore
