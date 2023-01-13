using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SingleUserCircuit;

public sealed class GlobalCircuitService
{
    public ConcurrentDictionary<Circuit, ClaimsPrincipal?> Circuits { get; }

    public event EventHandler? CircuitsChanged;

    private readonly ILogger _logger;

    void OnCircuitsChanged()
        => CircuitsChanged?.Invoke(this, EventArgs.Empty);

    public GlobalCircuitService(ILogger<GlobalCircuitService> logger)
    {
        Circuits = new();
        _logger = logger;
    }

    public void Attach(Circuit circuit, ClaimsPrincipal? user)
    {
        ArgumentNullException.ThrowIfNull(circuit);

        var circuitUser = Circuits.AddOrUpdate(circuit, user, (key, oldUser) => user);
        _logger.LogInformation("Circuit {circuit} attached to user {user}", circuit.Id, circuitUser.Identity.Name);

        OnCircuitsChanged();
    }

    public void Detach(Circuit circuit)
    {
        if (!Circuits.TryRemove(circuit, out var removedUser))
        {
            return;
        }

        _logger.LogInformation("Circuit {circuit} detached from user {user}", circuit.Id, removedUser.Identity.Name);

        OnCircuitsChanged();
    }
}
