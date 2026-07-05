using TelemetryWerk.Api.Domain.Entities;

namespace TelemetryWerk.Api.Application.Contracts;

public record MachineStateUpdateMessage(string Action, MachineNode Machine);
