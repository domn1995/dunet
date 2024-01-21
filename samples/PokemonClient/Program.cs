using PokemonClient;

const string baseUrl = "https://pokeapi.co/api/v2/pokemon/";
using var httpClient = new HttpClient() { BaseAddress = new Uri(baseUrl) };
var pokemonClient = new PokeClient(httpClient);

while (true)
{
    Console.Write("Enter a pokemon name: ");
    var pokemonName = Console.ReadLine() ?? "";
    var output = await pokemonClient
        .GetPokemonAsync(pokemonName)
        .MatchAsync(
            static success => success.Value.ToString(),
            static failure => failure.Error.Message
        );
    Console.WriteLine(output);
}
