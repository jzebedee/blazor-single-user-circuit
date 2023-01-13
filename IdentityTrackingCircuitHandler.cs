using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Security.Claims;

namespace SingleUserCircuit;

public class IdentityTrackingCircuitHandler : CircuitHandler
{
    public Circuit? Circuit { get; private set; }
    public ClaimsPrincipal? AssociatedUser { get; private set; }

    private string UserName => AssociatedUser.Identity.Name;

    private readonly ILogger _logger;
    private readonly GlobalCircuitService _circuitService;
    private readonly AuthenticationStateProvider _authentication;

    public IdentityTrackingCircuitHandler(ILogger<IdentityTrackingCircuitHandler> logger, GlobalCircuitService circuitService, AuthenticationStateProvider authentication)
    {
        _logger = logger;
        _circuitService = circuitService;
        _authentication = authentication;
        _authentication.AuthenticationStateChanged += AuthenticationStateChanged;
    }

    private async void AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var authState = await task;
        AssociatedUser = authState.User;

        _logger.LogInformation("Authentication state changed for circuit {circuit} and user {user}", Circuit?.Id, UserName);

        if (Circuit is not null)
        {
            _circuitService.Attach(Circuit, AssociatedUser);
        }
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Circuit = circuit;

        _logger.LogInformation("Circuit opened for circuit {circuit} and user {user}", circuit.Id, UserName);

        _circuitService.Attach(circuit, AssociatedUser);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit closed for circuit {circuit} and user {user}", circuit.Id, UserName);
        
        _circuitService.Detach(circuit);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}
