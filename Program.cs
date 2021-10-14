using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var loadCPU = false;

var CPULoadTask = () => {
    while (true) {
        while (loadCPU) {
            Thread.SpinWait(100000000);
        }
        Thread.Sleep(1000);
    }
}; 
//Estimate number of tasks to run min: 1 max: number of cores - 1 
int maxCPUs = (Environment.ProcessorCount > 1 ) ? Environment.ProcessorCount - 1 : 1; 
for (int cpu = 1; cpu <= maxCPUs; cpu++ ){
    Task.Run(CPULoadTask);
}

var instanceId = !String.IsNullOrEmpty(app.Configuration.GetValue<string>("WEBSITE_INSTANCE_ID")) 
    ? app.Configuration.GetValue<string>("WEBSITE_INSTANCE_ID")
    : "Not running in Azure App Service"; 

var welcomeMsg = !String.IsNullOrEmpty(app.Configuration.GetValue<string>("WELCOME_MSG")) 
    ? app.Configuration.GetValue<string>("WELCOME_MSG")
    : ""; 


app.MapGet("/", (HttpRequest request, HttpResponse response) => {
    if (!String.IsNullOrEmpty(request.Query["load"]) && request.Query["load"] == "true") {
        loadCPU = true;
    } else if(!String.IsNullOrEmpty(request.Query["load"]) && request.Query["load"] == "false") {
        loadCPU = false;
    }
    return response.WriteAsync(
        $@"
        <p>Welcome {welcomeMsg}</p>
        <br>
        <p>CPU loaded: {loadCPU}</p>
        <p><a href='/?load=true'>Generate CPU load</a></p>
        <p><a href='/?load=false'>Stop CPU load</a></p>
        <br>
        <p>CPU Core count: {Environment.ProcessorCount}</p>
        <p>Instance Id: {instanceId}</p>"
    );
});

app.Run();
