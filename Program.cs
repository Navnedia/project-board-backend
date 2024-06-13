using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProjectBoard.Models;
using ProjectBoard.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

string sampleProjectDataFile = "ProjectSampleData.json";

// Create, Configure, and build web application service:
var builder = WebApplication.CreateBuilder(args);

// Add services to the container:

// Configure Cors policy to allow us to test the application:
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAllOrigins", builder => {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure database dependency injection:
string dbConnectionString = builder.Configuration.GetConnectionString("Projects") 
    ?? "Data Source=./Database/ProjectsDb.db";
builder.Services.AddSqlite<ProjectContext>(dbConnectionString);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuthentication", null);

// Configuring Swagger/OpenAPI services for automatic REST API documentation (https://aka.ms/aspnetcore/swashbuckle):
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config => {
    config.AddSecurityDefinition("basic", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authorization header using the Bearer scheme."
    });

    config.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            }, new string[] {}
        }
    });

    config.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Project Board API",
        Description = "Sharing projects ideas to find your team.",
        Version = "v1",
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline: show swagger documentation when running in developer mode.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

//! I might split the different endpoint modules into seperate files.

// API endpoint to deletes the database and recreates it with sample data loaded from a json file.
app.MapGet("/initialize", async (ProjectContext context) => {
    // Delete and recreate the database:
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    Project[] projects = [];

    // Load in JSON sample data and deserialize to model objects:
    try {
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

        string jsonData = File.ReadAllText(sampleProjectDataFile);
        projects = JsonSerializer.Deserialize<Project[]>(jsonData, options) ?? [];
    } catch (Exception e) {
        Console.WriteLine("Failed to read and deserialize data from JSON File:\n" + e);
    }

    // Add and save the loaded sample projects into the database:
    try {
        foreach (var project in projects) {
            context.Projects.Add(project);
        }

        await context.SaveChangesAsync();
    } catch (Exception e) {
        Console.WriteLine("Issue adding/saving loaded projects to Database:\n" + e);
    }
})
.WithName("InitializeData")
.WithOpenApi();


// API endpoint to create/post a new User.
app.MapPost("/users", async (User user, ProjectContext dbContext) => {
    try {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    } catch (Exception e) {
        Console.WriteLine($"Something went wrong adding new user:\n{e.Message}");
        return Results.BadRequest(e.Message);
    }

    Console.WriteLine("Created new user");
    return Results.Created($"/users/{user.Id}", user);
})
.WithName("CreateUser")
.WithOpenApi();

// API endpoint to POST login request to request authentication of an existing account.
app.MapPost("/login", (User authorizedUser, ProjectContext dbContext) => {
    // Get the user from the database and return it if found.
    var user = dbContext.Users.FirstOrDefault(user => 
        user.Username == authorizedUser.Username
    );

    return (user == null) 
        ? Results.NotFound() 
        : Results.Ok(user);
})
.WithName("UserLogin")
.WithOpenApi()
.RequireAuthorization(new AuthorizeAttribute() {
    AuthenticationSchemes="BasicAuthentication"
});

// API endpoint to request a list of all projects for the authenticated user.
// app.MapGet("/users/projects", async (User authorizedUser, ProjectContext context) => {
//     // await context.Projects.FindAsync(Project => Project.OwnerId == authorizedUser.Id)
// })
// .WithName("GetUsersProjects")
// .WithOpenApi()
// .RequireAuthorization(new AuthorizeAttribute() {
//     AuthenticationSchemes="BasicAuthentication"
// });


// API endpoint to request a list of all project listings.
app.MapGet("/projects", async (ProjectContext context) => (
    await context.Projects.ToListAsync()
))
.WithName("GetProjects")
.WithOpenApi();

// API endpoint to request a project listing with the given id. 
app.MapGet("/projects/{id}", async (int id, ProjectContext context) => {
    Project? project = await context.Projects.FindAsync(id);

    return (project is null) 
        ? Results.NotFound() 
        : Results.Ok(project);
})
.WithName("GetProjectById")
.WithOpenApi();

// API endpoint to create/post a new project listing.
app.MapPost("/projects", async (Project project, ProjectContext context) => {
    context.Projects.Add(project);
    await context.SaveChangesAsync();

    return Results.Created($"/projects/{project.Id}", project);
})
.WithName("CreateProject")
.WithOpenApi();

// API endpoint to update properties of a specified project.
app.MapPut("/projects/{id}", async (int id, Project updatedProject, ProjectContext context) => {
    // Find the project by id and check for null:
    Project? project = await context.Projects.FindAsync(id);
    if (project is null) { return Results.NotFound(); }

    // Make updates to project and save changes to the database while preseving the Id:
    project.Title = updatedProject.Title;
    project.Description = updatedProject.Description;
    project.Summary = updatedProject.Summary;
    project.CoverImageURL = updatedProject.CoverImageURL;
    project.ContactEmail = updatedProject.ContactEmail;
    await context.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("UpdateProject")
.WithOpenApi();

// API endpoint to fully remove a project listing from the database.
app.MapDelete("/projects/{id}", async (int id, ProjectContext context) => {
    // Find the project to be deleted by id and check for null:
    Project? toBeDeleted = await context.Projects.FindAsync(id);
    if (toBeDeleted is null) { return Results.NotFound(); }

    // Remove item from database and commit changes:
    context.Projects.Remove(toBeDeleted);
    await context.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteProject")
.WithOpenApi();

app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
