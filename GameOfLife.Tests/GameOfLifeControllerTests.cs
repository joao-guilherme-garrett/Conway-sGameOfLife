using GameOfLife.Api;
using GameOfLife.Api.Data;
using GameOfLife.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace GameOfLife.Tests
{
    public class GameOfLifeApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameOfLifeContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<GameOfLifeContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<GameOfLifeContext>();
                    db.Database.EnsureCreated();
                }
            });
        }
    }

    public class GameOfLifeControllerTests : IClassFixture<GameOfLifeApiFactory>
    {
        private readonly HttpClient _client;
        private readonly GameOfLifeApiFactory _factory;

        public GameOfLifeControllerTests(GameOfLifeApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostBoard_ThenGetBoard_ReturnsSameBoardState()
        {
            var initialBoardDto = new BoardStateDto
            {
                Cells = new int[][]
                {
                    new int[] { 0, 1, 0 },
                    new int[] { 0, 1, 0 },
                    new int[] { 0, 1, 0 }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/gameoflife/boards", initialBoardDto);
            createResponse.EnsureSuccessStatusCode(); // Throws if not a 2xx status code

            var createResponseContent = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var boardId = createResponseContent.GetProperty("boardId").GetGuid();


            var getResponse = await _client.GetAsync($"/api/gameoflife/boards/{boardId}");
            getResponse.EnsureSuccessStatusCode();

            var getResponseDto = await getResponse.Content.ReadFromJsonAsync<BoardStateDto>();

            Assert.NotNull(getResponseDto);
            Assert.Equal(initialBoardDto.Cells.Length, getResponseDto.Cells.Length);
            Assert.Equal(initialBoardDto.Cells[0].Length, getResponseDto.Cells[0].Length);
        }
    }
}

