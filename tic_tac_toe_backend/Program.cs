using TicTacToeBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "Tic Tac Toe Backend";
    settings.Description = "Minimal Tic Tac Toe API with in-memory state.";
    settings.Version = "v1";
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register game service as singleton (in-memory state)
builder.Services.AddSingleton<GameService>();

var app = builder.Build();

app.UseCors("AllowAll");

// Configure OpenAPI/Swagger
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.Path = "/docs";
});

// Health check endpoint
app.MapGet("/", () => new { message = "Healthy" });

/*
 PUBLIC_INTERFACE
 GET /api/game/state
 Returns the current game state including board, currentPlayer, status and winner.
*/
app.MapGet("/api/game/state", (GameService game) =>
{
    /*
        Returns:
            200 OK with:
            {
                "board": string[3][3], // " ", "X", or "O"
                "currentPlayer": "X" | "O",
                "status": "in_progress" | "won" | "draw",
                "winner": "X" | "O" | null
            }
    */
    return Results.Ok(game.GetState());
})
.WithName("GetGameState")
.WithTags("Game")
.WithSummary("Get game state")
.WithDescription("Returns the current Tic Tac Toe game state.");

/*
 PUBLIC_INTERFACE
 POST /api/game/move
 Accepts JSON { "row": <0..2>, "col": <0..2> } and applies a move for the current player.
*/
app.MapPost("/api/game/move", (GameService game, MoveRequest req) =>
{
    /*
        Body:
            {
              "row": number, // 0..2
              "col": number  // 0..2
            }
        Returns:
            200 OK with updated game state on success
            400 BadRequest with { error: string } on invalid move
    */
    if (req == null)
    {
        return Results.BadRequest(new { error = "Invalid payload." });
    }

    var success = game.TryMakeMove(req.Row, req.Col, out var error);
    if (!success)
    {
        return Results.BadRequest(new { error });
    }

    return Results.Ok(game.GetState());
})
.WithName("MakeMove")
.WithTags("Game")
.WithSummary("Make a move")
.WithDescription("Attempts to play a move at the specified row and column.");

/*
 PUBLIC_INTERFACE
 POST /api/game/reset
 Resets the game to the initial state.
*/
app.MapPost("/api/game/reset", (GameService game) =>
{
    /*
        Returns:
            200 OK with a fresh game state
    */
    game.Reset();
    return Results.Ok(game.GetState());
})
.WithName("ResetGame")
.WithTags("Game")
.WithSummary("Reset the game")
.WithDescription("Resets the current game to a new empty game.");

app.Run();