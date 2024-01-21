using System.Net.Http.Json;
using Dunet;

namespace PokemonClient;

public class PokeClient
{
    private readonly HttpClient client;

    public PokeClient(HttpClient httpClient) => client = httpClient;

    public async Task<Result<Exception, Pokemon>> GetPokemonAsync(string name)
    {
        var fetch = () => client.GetFromJsonAsync<Pokemon>(name);

        try
        {
            return await fetch() is Pokemon pokemon
                ? pokemon
                : new Exception($"Unable to retrieve pokemon '{name}'.");
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}

public record Pokemon(int Id, string Name, int Weight, int Height);

[Union]
public partial record Result<TFailure, TSuccess>
{
    partial record Success(TSuccess Value);

    partial record Failure(TFailure Error);
}
