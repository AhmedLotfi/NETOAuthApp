using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "GitHub";
    options.DefaultChallengeScheme = "GitHub";
})
.AddOAuth("GitHub", options =>
{
    options.ClientId = "Ov23liNiqZ4HgelGOmW2";
    options.ClientSecret = "2b03b9afdd08e00c47ba31e8c8bcace02cccb407";
    options.CallbackPath = new PathString("/signin-github");

    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";

    // Define the data we want from the user
    options.Scope.Add("user:email");

    options.SaveTokens = true;

    // Event handler to get user info from GitHub after authentication
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

    options.Events.OnCreatingTicket = async context =>
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
        response.EnsureSuccessStatusCode();

        JsonDocument user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        context.RunClaimActions(user.RootElement);
    };
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
