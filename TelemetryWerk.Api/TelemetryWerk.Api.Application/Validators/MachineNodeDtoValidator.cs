using FluentValidation;
using TelemetryWerk.Api.Application.Contracts;

namespace TelemetryWerk.Api.Application.Validators;

public class MachineNodeDtoValidator : AbstractValidator<MachineNodeDto>
{
    public MachineNodeDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Machine ID is required.")
            .Length(2, 50).WithMessage("Machine ID must be between 2 and 50 characters.")
            .Matches(@"^TK-.+$").WithMessage("Machine ID must follow the pattern TK-XXX.");

        RuleFor(x => x.CoreTemperature)
            .InclusiveBetween(-50, 500).WithMessage("Core temperature must be between -50 and 500.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => s is "Running" or "Stopped" or "Warning" or "Maintenance")
            .WithMessage("Status must be 'Running', 'Stopped', 'Warning', or 'Maintenance'.");
    }
}
