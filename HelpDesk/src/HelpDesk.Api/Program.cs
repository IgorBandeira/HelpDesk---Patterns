using HelpDesk.Api.Controllers.Attachments.Documentation;
using HelpDesk.Api.Controllers.Collaboration.Documentation;
using HelpDesk.Api.Controllers.IdentityAccess.Documentation;
using HelpDesk.Api.Controllers.ServiceCatalog.Documentation;
using HelpDesk.Api.Controllers.Ticketing.Documentation;
using HelpDesk.Api.DependencyInjection;
using HelpDesk.Infrastructure.Collaboration.DependencyInjection;
using HelpDesk.Infrastructure.Attachments.DependencyInjection;
using HelpDesk.Infrastructure.IdentityAccess.DependencyInjection;
using HelpDesk.Infrastructure.Operations.DependencyInjection;
using HelpDesk.Infrastructure.Operations.Notifications.Email.DependencyInjection;
using HelpDesk.Infrastructure.ServiceCatalog.DependencyInjection;
using HelpDesk.Infrastructure.Shared.DependencyInjection;
using HelpDesk.Infrastructure.Ticketing.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddIdentityAccessInfrastructure();
builder.Services.AddIdentityAccessApi();

builder.Services.AddServiceCatalogInfrastructure();
builder.Services.AddServiceCatalogApi();

builder.Services.AddTicketingInfrastructure();
builder.Services.AddTicketingApi();

builder.Services.AddCollaborationApi();
builder.Services.AddCollaborationInfrastructure();

builder.Services.AddAttachmentsApi();
builder.Services.AddAttachmentsInfrastructure();

builder.Services.AddNotificationInfrastructure();

builder.Services.AddSharedInfrastructure();
builder.Services.AddEmailInfrastructure(builder.Configuration);
builder.Services.AddFileStorageInfrastructure(builder.Configuration);

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HelpDesk",
        Version = "v1",
        Description = "Sistema de gerenciamento completo de tickets de suporte técnico, incluindo abertura, atribuição, comentários, anexos, notificações automáticas por e-mail e monitoramento de SLA (Service Level Agreement). "
    });
    o.EnableAnnotations();
    o.OperationFilter<UsersControllerSwaggerFilter>();
    o.DocumentFilter<UsersControllerTagsDocumentFilter>();
    o.OperationFilter<CategoriesControllerSwaggerFilter>();
    o.DocumentFilter<CategoriesControllerTagsDocumentFilter>();
    o.OperationFilter<TicketsControllerSwaggerFilter>();
    o.DocumentFilter<TicketsControllerTagsDocumentFilter>();
    o.OperationFilter<CommentsControllerSwaggerFilter>();
    o.DocumentFilter<CommentsControllerTagsDocumentFilter>();
    o.OperationFilter<AttachmentsControllerSwaggerFilter>();
    o.DocumentFilter<AttachmentsControllerTagsDocumentFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        o.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }