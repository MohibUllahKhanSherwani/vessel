using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Vessel.API.Filters;
/// <summary>
/// An action filter that automatically validates model state and returns a bad request response on invalid inputs.
/// </summary>
public class ValidationFilter : IActionFilter
{
    /// <summary>
    /// Validates the model state before an action executes.
    /// </summary>
    /// <param name="context">The action executing context.</param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
    /// <summary>
    /// Occurs after an action has executed.
    /// </summary>
    /// <param name="context">The action executed context.</param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}