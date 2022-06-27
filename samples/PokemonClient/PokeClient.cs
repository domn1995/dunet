using Dunet;
using System.Net.Http.Json;

namespace PokemonClient;

// Some aliases to make code more terse and less noisy.
using PokemonResult = Result<string, Pokemon>;
using static Result<string, Pokemon>;

public class PokeClient
{
    private readonly HttpClient client;

    public PokeClient(HttpClient httpClient) => client = httpClient;

    public async Task<PokemonResult> GetPokemonAsync(string name)
    {
        var fetch = () => client.GetFromJsonAsync<Pokemon>(name);

        try
        {
            return await fetch() is Pokemon pokemon
                ? new Success(pokemon)
                : new Failure($"Unable to retrieve pokemon '{name}'.");
        }
        catch (Exception ex)
        {
            return new Failure(ex.Message);
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
