
/*CreateBuilder Does some of the basic setup of the ASP.NET Core platform
* - Sets up the Kestrel web server (handles requests, usually placed behind a real web server)
* - IIS configuration is integrated with this method
* - specifies the content root directory for the executable code
* - configuration information is read from appsettings.json
*/
var builder = WebApplication.CreateBuilder(args);



/* builder can be used to access the services collection to register other services
* - app becomes type WebApplication which is used to set up the middleware componenets and middleware pipeline
*/
var app = builder.Build();


/* MapGet is a middleware component*/
app.MapGet("/", () => "Hello World!");

/*Makes the app start listening for incoming requests*/
app.Run();
