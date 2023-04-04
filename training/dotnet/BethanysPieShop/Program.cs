using BethanysPieShop.Models;

/*CreateBuilder Does some of the basic setup of the ASP.NET Core platform
* - Sets up the Kestrel web server (handles requests, usually placed behind a real web server)
* - IIS configuration is integrated with this method
* - specifies the content root directory for the executable code
* - configuration information is read from appsettings.json
*/
var builder = WebApplication.CreateBuilder(args);

//Add MVC services to the app
builder.Services.AddControllersWithViews(); 
builder.Services.AddScoped<ICategoryRepository, MockCategoryRepository>();
builder.Services.AddScoped<IPieRepository, MockPieRepository>();

//Add custom services
//builder.Services.AddScoped<ILoggerService, LoggerService>();


/* builder can be used to access the services collection to register other services
* - app becomes type WebApplication which is used to set up the middleware componenets and middleware pipeline
*/
var app = builder.Build(); //Middleware components can be added after the WebApplication object is created


/* MapGet is a middleware component that listens for incoming requests to the root of the app then will return Hello World*/
//app.MapGet("/", () => "Hello World!");

app.UseStaticFiles(); //Middleware that will look in the default configuration folder for static files when static files are requested

if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); //Middleware that will show errors inside the executing application in DEV mode
}

app.MapDefaultControllerRoute(); //Middleware that by default will route requests to the correct views

/*Makes the app start listening for incoming requests*/
app.Run();