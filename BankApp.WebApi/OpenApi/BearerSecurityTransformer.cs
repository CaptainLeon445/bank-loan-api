using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace BankApp.WebApi.OpenApi;

// Declares JWT bearer auth in the OpenAPI document so Scalar shows a token field,
// and marks only [Authorize] endpoints as requiring it (auth endpoints stay open).
public sealed class BearerSecurityTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private const string SchemeId = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the token from /api/auth/login or /api/auth/register (without the 'Bearer ' prefix)."
        };
        return Task.CompletedTask;
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        bool requiresAuth = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();
        if (!requiresAuth) return Task.CompletedTask;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = SchemeId } }] = new List<string>()
        });
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Missing, invalid or expired token" });
        return Task.CompletedTask;
    }
}
