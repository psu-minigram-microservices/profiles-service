namespace Minigram.IntegrationTests.Controllers
{
    using System.Net;
    using System.Net.Http.Json;
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Dto;
    using Minigram.IntegrationTests.Common;
    using Minigram.Profile;
    using Minigram.Profile.Controllers.Dto;
    using ProfileEntity = Minigram.Profile.ApplicationContext.Models.Profile;

    public class TestProfileController
        : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private const string BaseUrl = "/api/v1.0/profiles";

        private readonly CustomWebApplicationFactory<Program> _factory;

        public TestProfileController(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        public Task InitializeAsync() => _factory.ResetDatabaseAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        // GET /profiles

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(BaseUrl);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsAllProfiles()
        {
            await _factory.SeedAsync(async db =>
            {
                await db.Profiles.AddRangeAsync(
                    NewProfile(name: "Vlad"),
                    NewProfile(name: "Elena"),
                    NewProfile(name: "Oleg"));
            });

            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var paged = await client.GetFromJsonAsync<PagedResponse<ProfileResponseDto>>(
                BaseUrl, TestJsonOptions.Default);

            Assert.NotNull(paged);
            Assert.Equal(3, paged!.Count);
            Assert.Equal(3, paged.Data.Count);
            Assert.Contains(paged.Data, p => p.Name == "Vlad");
            Assert.Contains(paged.Data, p => p.Name == "Elena");
            Assert.Contains(paged.Data, p => p.Name == "Oleg");
        }

        [Fact]
        public async Task GetAll_WithPagination_ReturnsRequestedPage()
        {
            await _factory.SeedAsync(async db =>
            {
                for (int i = 0; i < 5; i++)
                {
                    await db.Profiles.AddAsync(NewProfile(name: $"User{i}"));
                }
            });

            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var paged = await client.GetFromJsonAsync<PagedResponse<ProfileResponseDto>>(
                $"{BaseUrl}?page=1&perPage=2", TestJsonOptions.Default);

            Assert.NotNull(paged);
            Assert.Equal(5, paged!.Count);
            Assert.Equal(2, paged.Data.Count);
        }

        // GET /profiles/me

        [Fact]
        public async Task GetMe_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{BaseUrl}/me");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMe_WhenProfileDoesNotExist_Returns404()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.GetAsync($"{BaseUrl}/me");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMe_WhenProfileExists_ReturnsProfile()
        {
            var userId = Guid.NewGuid();
            var profileId = Guid.NewGuid();

            await _factory.SeedAsync(async db =>
            {
                await db.Profiles.AddAsync(new ProfileEntity
                {
                    Id = profileId,
                    UserId = userId,
                    Name = "Me",
                    PhotoUrl = "https://example.com/me.png",
                });
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var profile = await client.GetFromJsonAsync<ProfileResponseDto>(
                $"{BaseUrl}/me", TestJsonOptions.Default);

            Assert.NotNull(profile);
            Assert.Equal(profileId, profile!.Id);
            Assert.Equal("Me", profile.Name);
            Assert.Equal("https://example.com/me.png", profile.PhotoUrl);
        }

        // GET /profiles/{id}

        [Fact]
        public async Task GetById_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenProfileDoesNotExist_Returns404()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenProfileExists_ReturnsProfile()
        {
            var profileId = Guid.NewGuid();

            await _factory.SeedAsync(async db =>
            {
                await db.Profiles.AddAsync(new ProfileEntity
                {
                    Id = profileId,
                    UserId = Guid.NewGuid(),
                    Name = "Other",
                    PhotoUrl = null,
                });
            });

            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var profile = await client.GetFromJsonAsync<ProfileResponseDto>(
                $"{BaseUrl}/{profileId}", TestJsonOptions.Default);

            Assert.NotNull(profile);
            Assert.Equal(profileId, profile!.Id);
            Assert.Equal("Other", profile.Name);
            Assert.Null(profile.PhotoUrl);
        }

        // POST /profiles

        [Fact]
        public async Task Create_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(BaseUrl, new ProfileRequestDto
            {
                Name = "New",
                PhotoUrl = null,
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithValidData_Returns201AndPersistsProfile()
        {
            var userId = Guid.NewGuid();
            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PostAsJsonAsync(BaseUrl, new ProfileRequestDto
            {
                Name = "Brand New",
                PhotoUrl = "https://example.com/avatar.png",
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<ProfileResponseDto>(
                TestJsonOptions.Default);
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.Id);
            Assert.Equal("Brand New", created.Name);
            Assert.Equal("https://example.com/avatar.png", created.PhotoUrl);

            var persisted = await _factory.ReadAsync(db =>
                db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId));
            Assert.NotNull(persisted);
            Assert.Equal("Brand New", persisted!.Name);
        }

        [Fact]
        public async Task Create_WithMissingName_Returns400()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.PostAsJsonAsync(BaseUrl, new { photoUrl = (string?)null });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithInvalidPhotoUrl_Returns400()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.PostAsJsonAsync(BaseUrl, new ProfileRequestDto
            {
                Name = "Bad URL",
                PhotoUrl = "not-a-valid-url",
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // PUT /profiles

        [Fact]
        public async Task Update_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.PutAsJsonAsync(BaseUrl, new ProfileRequestDto
            {
                Name = "Updated",
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Update_WhenProfileDoesNotExist_Returns404()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.PutAsJsonAsync(BaseUrl, new ProfileRequestDto
            {
                Name = "Updated",
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithValidData_PersistsChanges()
        {
            var userId = Guid.NewGuid();
            var profileId = Guid.NewGuid();

            await _factory.SeedAsync(async db =>
            {
                await db.Profiles.AddAsync(new ProfileEntity
                {
                    Id = profileId,
                    UserId = userId,
                    Name = "Old",
                    PhotoUrl = "https://example.com/old.png",
                });
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PutAsJsonAsync(BaseUrl, new ProfileRequestDto
            {
                Name = "New Name",
                PhotoUrl = "https://example.com/new.png",
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var persisted = await _factory.ReadAsync(db =>
                db.Profiles.AsNoTracking().FirstAsync(p => p.Id == profileId));
            Assert.Equal("New Name", persisted.Name);
            Assert.Equal("https://example.com/new.png", persisted.PhotoUrl);
        }

        private static ProfileEntity NewProfile(
            Guid? id = null,
            Guid? userId = null,
            string name = "User",
            string? photoUrl = null)
        {
            return new ProfileEntity
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Name = name,
                PhotoUrl = photoUrl,
            };
        }
    }
}
