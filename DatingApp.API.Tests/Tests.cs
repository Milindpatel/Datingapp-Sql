using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DatingApp.API.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Verifies_correct_password_only()
    {
        var (hash, salt) = PasswordHasher.Hash("correct horse");
        Assert.True(PasswordHasher.Verify("correct horse", hash, salt));
        Assert.False(PasswordHasher.Verify("wrong horse", hash, salt));
    }

    [Fact]
    public void Verifies_values_padded_by_binary512_columns()
    {
        var (hash, salt) = PasswordHasher.Hash("correct horse");
        var paddedHash = new byte[512];
        var paddedSalt = new byte[512];
        hash.CopyTo(paddedHash, 0);
        salt.CopyTo(paddedSalt, 0);

        Assert.True(PasswordHasher.Verify("correct horse", paddedHash, paddedSalt));
        Assert.False(PasswordHasher.Verify("wrong horse", paddedHash, paddedSalt));
    }

    [Fact]
    public void Rejects_missing_or_short_values()
    {
        Assert.False(PasswordHasher.Verify("x", null, null));
        Assert.False(PasswordHasher.Verify("x", new byte[10], new byte[10]));
    }
}

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _db = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
        builder.ConfigureServices(services =>
        {
            // Swap SQL Server for an in-memory database.
            services.RemoveAll<DbContextOptions<DataContext>>();
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<DataContext>));
            services.AddDbContext<DataContext>(o => o.UseInMemoryDatabase(_db));
        });
    }
}

public class AuthApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private CancellationToken Ct => TestContext.Current.CancellationToken;
    private record TokenResponse(string Token);
    private record MeResponse(string Username);

    [Fact]
    public async Task Register_login_and_call_protected_endpoint()
    {
        var client = factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", new { username = "Alice", password = "password123" }, Ct);
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "alice", password = "password123" }, Ct);
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<TokenResponse>(Ct))!.Token;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await client.GetFromJsonAsync<MeResponse>("/api/auth/me", Ct);
        Assert.Equal("alice", me!.Username);
    }

    [Fact]
    public async Task Duplicate_usernames_are_rejected()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new { username = "bob", password = "password123" }, Ct);
        var again = await client.PostAsJsonAsync("/api/auth/register", new { username = "BOB", password = "password123" }, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
    }

    [Fact]
    public async Task Short_passwords_are_rejected()
    {
        var res = await factory.CreateClient().PostAsJsonAsync("/api/auth/register", new { username = "carol", password = "short" }, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Wrong_password_and_unknown_user_get_401()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new { username = "dave", password = "password123" }, Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { username = "dave", password = "nope-nope" }, Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { username = "nobody", password = "password123" }, Ct)).StatusCode);
    }

    [Fact]
    public async Task Protected_endpoint_requires_token()
    {
        var res = await factory.CreateClient().GetAsync("/api/auth/me", Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
