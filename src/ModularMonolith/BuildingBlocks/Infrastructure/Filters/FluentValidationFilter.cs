using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ModularMonolith.BuildingBlocks.Application.Common;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Filters;

public sealed class FluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationFilter(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var value in context.ActionArguments.Values)
        {
            if (value is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(value.GetType());
            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(value);
                var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(e => e.ErrorMessage);
                    context.Result = new BadRequestObjectResult(ApiResponse.ValidationError(errors));
                    return;
                }
            }
        }

        await next();
    }
}
