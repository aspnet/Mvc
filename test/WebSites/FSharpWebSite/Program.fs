namespace FSharpWebSite

open System.IO
open Microsoft.AspNetCore.Hosting


module Program =

    [<EntryPoint>]
    let main args =
        let host = 
            WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseKestrel()
                .UseIISIntegration()
                .Build()

        host.Run()
        
        0
