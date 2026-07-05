using FluentValidation;
using TelemetryWerk.Api.Application.Contracts;

namespace TelemetryWerk.Api.Application.Validators;

public class UpdateNodeRequestDtoValidator : AbstractValidator<UpdateNodeRequestDto>
{
    public UpdateNodeRequestDtoValidator()
    {
        RuleFor(x => x.CoreTemperature)
            .InclusiveBetween(-50, 500).When(x => x.CoreTemperature.HasValue)
            .WithMessage("Core temperature must be between -50 and 500.");

        RuleFor(x => x.Status)
            .Must(s => s is "Running" or "Stopped" or "Warning" or "Maintenance")
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage("Status must be 'Running', 'Stopped', 'Warning', or 'Maintenance'.");
    }
}
