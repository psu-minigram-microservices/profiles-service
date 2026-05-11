namespace Minigram.IntegrationTests.Controllers
{
    using System.Net;
    using System.Net.Http.Json;
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Dto;
    using Minigram.IntegrationTests.Common;
    using Minigram.Profile;
    using Minigram.Profile.ApplicationContext.Models;
    using Minigram.Profile.Controllers.Dto;
    using ProfileEntity = Minigram.Profile.ApplicationContext.Models.Profile;

    public class TestRelationController
        : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private const string BaseUrl = "/api/v1.0/profiles/relations";

        private readonly CustomWebApplicationFactory<Program> _factory;

        public TestRelationController(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        public Task InitializeAsync() => _factory.ResetDatabaseAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        // GET /profiles/relations

        [Fact]
        public async Task GetByStatus_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{BaseUrl}?status=Friend&type=Outgoing");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetByStatus_WhenStatusMissing_Returns400()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.GetAsync($"{BaseUrl}?type=Outgoing");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetByStatus_WhenTypeMissing_Returns400()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.GetAsync($"{BaseUrl}?status=Friend");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetByStatus_WhenCallerHasNoProfile_Returns404()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.GetAsync($"{BaseUrl}?status=Friend&type=Outgoing");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByStatus_Outgoing_ReturnsReceiverProfiles()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync("Me");
            var friendA = await SeedProfileAsync("FriendA");
            var friendB = await SeedProfileAsync("FriendB");
            var stranger = await SeedProfileAsync("Stranger");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddRangeAsync(
                    NewRelation(meId, friendA.Id, tStatus.Friend),
                    NewRelation(meId, friendB.Id, tStatus.Friend),
                    NewRelation(meId, stranger.Id, tStatus.None));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var paged = await client.GetFromJsonAsync<PagedResponse<ProfileResponseDto>>(
                $"{BaseUrl}?status=Friend&type=Outgoing", TestJsonOptions.Default);

            Assert.NotNull(paged);
            Assert.Equal(2, paged!.Count);
            Assert.Equal(2, paged.Data.Count);
            Assert.Contains(paged.Data, p => p.Id == friendA.Id);
            Assert.Contains(paged.Data, p => p.Id == friendB.Id);
            Assert.DoesNotContain(paged.Data, p => p.Id == stranger.Id);
        }

        [Fact]
        public async Task GetByStatus_Incoming_ReturnsSenderProfiles()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync("Me");
            var requesterA = await SeedProfileAsync("RequesterA");
            var requesterB = await SeedProfileAsync("RequesterB");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddRangeAsync(
                    NewRelation(requesterA.Id, meId, tStatus.None),
                    NewRelation(requesterB.Id, meId, tStatus.None));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var paged = await client.GetFromJsonAsync<PagedResponse<ProfileResponseDto>>(
                $"{BaseUrl}?status=None&type=Incoming", TestJsonOptions.Default);

            Assert.NotNull(paged);
            Assert.Equal(2, paged!.Count);
            Assert.Contains(paged.Data, p => p.Id == requesterA.Id);
            Assert.Contains(paged.Data, p => p.Id == requesterB.Id);
        }

        [Fact]
        public async Task GetByStatus_WithPagination_PagesResults()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync("Me");

            await _factory.SeedAsync(async db =>
            {
                for (int i = 0; i < 4; i++)
                {
                    var receiver = new ProfileEntity
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        Name = $"Friend{i}",
                    };
                    await db.Profiles.AddAsync(receiver);
                    await db.Relations.AddAsync(NewRelation(meId, receiver.Id, tStatus.Friend));
                }
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var paged = await client.GetFromJsonAsync<PagedResponse<ProfileResponseDto>>(
                $"{BaseUrl}?status=Friend&type=Outgoing&page=0&perPage=2", TestJsonOptions.Default);

            Assert.NotNull(paged);
            Assert.Equal(4, paged!.Count);
            Assert.Equal(2, paged.Data.Count);
        }

        // GET /profiles/relations/{receiverId}

        [Fact]
        public async Task Get_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}?type=Outgoing");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_WithoutTypeQueryParam_Returns400()
        {
            var (userId, _) = await SeedCurrentUserProfileAsync();
            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_WhenRelationDoesNotExist_Returns404()
        {
            var (userId, _) = await SeedCurrentUserProfileAsync();
            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}?type=Outgoing");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_Outgoing_ReturnsRelationWithReceiverProfile()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync();
            var receiver = await SeedProfileAsync("Receiver");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddAsync(NewRelation(meId, receiver.Id, tStatus.Friend));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var dto = await client.GetFromJsonAsync<RelationResponseDto>(
                $"{BaseUrl}/{receiver.Id}?type=Outgoing", TestJsonOptions.Default);

            Assert.NotNull(dto);
            Assert.Equal(tStatus.Friend, dto!.Status);
            Assert.Equal(receiver.Id, dto.Profile.Id);
            Assert.Equal("Receiver", dto.Profile.Name);
        }

        [Fact]
        public async Task Get_Incoming_ReturnsRelation()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync();
            var sender = await SeedProfileAsync("Sender");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddAsync(NewRelation(sender.Id, meId, tStatus.None));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.GetAsync($"{BaseUrl}/{sender.Id}?type=Incoming");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<RelationResponseDto>(
                TestJsonOptions.Default);
            Assert.NotNull(dto);
            Assert.Equal(tStatus.None, dto!.Status);
        }

        // POST /profiles/relations/send/{receiverId}

        [Fact]
        public async Task Send_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsync($"{BaseUrl}/send/{Guid.NewGuid()}", content: null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Send_WhenSenderHasNoProfile_Returns404()
        {
            var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

            var response = await client.PostAsync($"{BaseUrl}/send/{Guid.NewGuid()}", content: null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Send_CreatesRelationWithStatusNone()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync();
            var receiver = await SeedProfileAsync("Receiver");

            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PostAsync($"{BaseUrl}/send/{receiver.Id}", content: null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var relation = await _factory.ReadAsync(db =>
                db.Relations.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.SenderId == meId && r.ReceiverId == receiver.Id));
            Assert.NotNull(relation);
            Assert.Equal(tStatus.None, relation!.Status);
        }

        // POST /profiles/relations/reply/{senderId}

        [Fact]
        public async Task Reply_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsync(
                $"{BaseUrl}/reply/{Guid.NewGuid()}?status=Accepted", content: null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Reply_WithoutStatusQueryParam_Returns400()
        {
            var (userId, _) = await SeedCurrentUserProfileAsync();
            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PostAsync($"{BaseUrl}/reply/{Guid.NewGuid()}", content: null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Reply_WhenRelationDoesNotExist_Returns404()
        {
            var (userId, _) = await SeedCurrentUserProfileAsync();
            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PostAsync(
                $"{BaseUrl}/reply/{Guid.NewGuid()}?status=Accepted", content: null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Reply_Accepted_SetsStatusToFriendAndCreatesReverse()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync();
            var sender = await SeedProfileAsync("Sender");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddAsync(NewRelation(sender.Id, meId, tStatus.None));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PostAsync(
                $"{BaseUrl}/reply/{sender.Id}?status=Accepted", content: null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var relations = await _factory.ReadAsync(db =>
                db.Relations.AsNoTracking().ToListAsync());
            Assert.Equal(2, relations.Count);

            var original = relations.Single(r => r.SenderId == sender.Id && r.ReceiverId == meId);
            var reverse = relations.Single(r => r.SenderId == meId && r.ReceiverId == sender.Id);
            Assert.Equal(tStatus.Friend, original.Status);
            Assert.Equal(tStatus.Friend, reverse.Status);
        }

        [Fact]
        public async Task Reply_AcceptedWithExistingReverse_DoesNotDuplicate()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync();
            var sender = await SeedProfileAsync("Sender");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddRangeAsync(
                    NewRelation(sender.Id, meId, tStatus.None),
                    NewRelation(meId, sender.Id, tStatus.None));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PostAsync(
                $"{BaseUrl}/reply/{sender.Id}?status=Accepted", content: null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var relations = await _factory.ReadAsync(db =>
                db.Relations.AsNoTracking().ToListAsync());
            Assert.Equal(2, relations.Count);

            var original = relations.Single(r => r.SenderId == sender.Id && r.ReceiverId == meId);
            Assert.Equal(tStatus.Friend, original.Status);
        }

        [Fact]
        public async Task Reply_Rejected_DeletesRelation()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync();
            var sender = await SeedProfileAsync("Sender");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddAsync(NewRelation(sender.Id, meId, tStatus.None));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PostAsync(
                $"{BaseUrl}/reply/{sender.Id}?status=Rejected", content: null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var remaining = await _factory.ReadAsync(db =>
                db.Relations.AsNoTracking().AnyAsync(r => r.SenderId == sender.Id && r.ReceiverId == meId));
            Assert.False(remaining);
        }

        [Fact]
        public async Task Reply_Blocked_SetsStatusToBlocked()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync();
            var sender = await SeedProfileAsync("Sender");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddAsync(NewRelation(sender.Id, meId, tStatus.None));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.PostAsync(
                $"{BaseUrl}/reply/{sender.Id}?status=Blocked", content: null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var relation = await _factory.ReadAsync(db =>
                db.Relations.AsNoTracking()
                    .FirstAsync(r => r.SenderId == sender.Id && r.ReceiverId == meId));
            Assert.Equal(tStatus.Blocked, relation.Status);
        }

        // DELETE /profiles/relations/{receiverId}

        [Fact]
        public async Task Delete_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync($"{BaseUrl}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenRelationDoesNotExist_Returns404()
        {
            var (userId, _) = await SeedCurrentUserProfileAsync();
            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"{BaseUrl}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_RemovesRelation()
        {
            var (userId, meId) = await SeedCurrentUserProfileAsync();
            var receiver = await SeedProfileAsync("Receiver");

            await _factory.SeedAsync(async db =>
            {
                await db.Relations.AddAsync(NewRelation(meId, receiver.Id, tStatus.Friend));
            });

            var client = _factory.CreateAuthenticatedClient(userId);

            var response = await client.DeleteAsync($"{BaseUrl}/{receiver.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var remaining = await _factory.ReadAsync(db =>
                db.Relations.AsNoTracking()
                    .AnyAsync(r => r.SenderId == meId && r.ReceiverId == receiver.Id));
            Assert.False(remaining);
        }

        // Helpers

        private async Task<(Guid UserId, Guid ProfileId)> SeedCurrentUserProfileAsync(string name = "Me")
        {
            var userId = Guid.NewGuid();
            var profileId = Guid.NewGuid();

            await _factory.SeedAsync(async db =>
            {
                await db.Profiles.AddAsync(new ProfileEntity
                {
                    Id = profileId,
                    UserId = userId,
                    Name = name,
                });
            });

            return (userId, profileId);
        }

        private async Task<ProfileEntity> SeedProfileAsync(string name)
        {
            var profile = new ProfileEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = name,
            };

            await _factory.SeedAsync(async db => await db.Profiles.AddAsync(profile));
            return profile;
        }

        private static Relation NewRelation(Guid senderId, Guid receiverId, tStatus status)
        {
            return new Relation
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = status,
            };
        }
    }
}
