using PokemonClient;

const string baseUrl = "https://pokeapi.co/api/v2/pokemon/";
using var httpClient = new HttpClient() { BaseAddress = new Uri(baseUrl) };
var pokemonClient = new PokeClient(httpClient);

while (true)
{
    Console.Write("Enter a pokemon name: ");
    var pokemonName = Console.ReadLine() ?? "";
    var result = await pokemonClient.GetPokemonAsync(pokemonName);
    var output = result.Match(success => success.Value.ToString(), failure => failure.Error);
    Console.WriteLine(output);
}
