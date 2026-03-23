using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace HelpDesk.IntegrationTests.Collaboration
{
    public class Comments_Delete_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Comments_Delete_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Delete_Should_Return_Ok_When_Author_Deletes_Comment()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = new Infrastructure.IdentityAccess.Models.UserEntity
            {
                Name = "Author",
                Email = "author_delete1@x.com",
                Role = "Requester"
            };
            db.Users.Add(author);
            await db.SaveChangesAsync();

            var ticket = new Infrastructure.Ticketing.Models.TicketEntity
            {
                Title = "Ticket",
                Description = "Desc",
                Status = "Aberto",
                RequesterId = author.Id
            };
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            var comment = new CommentEntity
            {
                TicketId = ticket.Id,
                AuthorId = author.Id,
                Visibility = "Público",
                Message = "Comentário",
                CreatedAt = DateTime.UtcNow
            };
            db.Add(comment);
            await db.SaveChangesAsync();

            var http = new HttpRequestMessage(HttpMethod.Delete, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", author.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Return_Forbidden_When_User_Is_Not_Author()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = new Infrastructure.IdentityAccess.Models.UserEntity
            {
                Name = "Author",
                Email = "author_delete2@x.com",
                Role = "Requester"
            };

            var outsider = new Infrastructure.IdentityAccess.Models.UserEntity
            {
                Name = "Out",
                Email = "out_delete2@x.com",
                Role = "Requester"
            };

            db.Users.AddRange(author, outsider);
            await db.SaveChangesAsync();

            var ticket = new Infrastructure.Ticketing.Models.TicketEntity
            {
                Title = "Ticket",
                Description = "Desc",
                Status = "Aberto",
                RequesterId = author.Id
            };
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            var comment = new CommentEntity
            {
                TicketId = ticket.Id,
                AuthorId = author.Id,
                Visibility = "Público",
                Message = "Comentário",
                CreatedAt = DateTime.UtcNow
            };
            db.Add(comment);
            await db.SaveChangesAsync();

            var http = new HttpRequestMessage(HttpMethod.Delete, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", outsider.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Return_BadRequest_When_Ticket_Is_Inactive()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = new Infrastructure.IdentityAccess.Models.UserEntity
            {
                Name = "Author",
                Email = "author_delete3@x.com",
                Role = "Requester"
            };
            db.Users.Add(author);
            await db.SaveChangesAsync();

            var ticket = new Infrastructure.Ticketing.Models.TicketEntity
            {
                Title = "Ticket",
                Description = "Desc",
                Status = "Fechado",
                RequesterId = author.Id
            };
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            var comment = new CommentEntity
            {
                TicketId = ticket.Id,
                AuthorId = author.Id,
                Visibility = "Público",
                Message = "Comentário",
                CreatedAt = DateTime.UtcNow
            };
            db.Add(comment);
            await db.SaveChangesAsync();

            var http = new HttpRequestMessage(HttpMethod.Delete, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", author.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Return_NotFound_When_Comment_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = new Infrastructure.IdentityAccess.Models.UserEntity
            {
                Name = "Author",
                Email = "author_delete4@x.com",
                Role = "Requester"
            };
            db.Users.Add(author);
            await db.SaveChangesAsync();

            var ticket = new Infrastructure.Ticketing.Models.TicketEntity
            {
                Title = "Ticket",
                Description = "Desc",
                Status = "Aberto",
                RequesterId = author.Id
            };
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            var http = new HttpRequestMessage(HttpMethod.Delete, $"/api/tickets/{ticket.Id}/comments/999999");
            http.Headers.Add("userId", author.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}