using PokemonClient;
using static PokemonClient.Result<System.Exception, PokemonClient.Pokemon>;

const string baseUrl = "https://pokeapi.co/api/v2/pokemon/";
using var httpClient = new HttpClient() { BaseAddress = new Uri(baseUrl) };
var pokemonClient = new PokeClient(httpClient);

while (true)
{
    Console.Write("Enter a pokemon name: ");
    var pokemonName = Console.ReadLine() ?? "";
    var output = await pokemonClient.GetPokemonAsync(pokemonName) switch
    {
        Success(var value) => value.ToString(),
        Failure(var error) => error.Message,
    };
    Console.WriteLine(output);
}
