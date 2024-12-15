﻿using MatrixTextClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// load environment variables or a .env file
DotNetEnv.Env.TraversePath().Load();
var userid = Environment.GetEnvironmentVariable("MATRIX_USER_ID");
var password = Environment.GetEnvironmentVariable("MATRIX_PASSWORD");
var device = Environment.GetEnvironmentVariable("MATRIX_DEVICE_ID");
if (string.IsNullOrWhiteSpace(userid) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(device))
{
    throw new ArgumentException("Please provide the required environment variables: MATRIX_USER_ID, MATRIX_PASSWORD, MATRIX_DEVICE_ID");
}

//set up dependency injection
var services = new ServiceCollection()
    .AddHttpClient()
    .AddLogging(services => services.AddSimpleConsole()).BuildServiceProvider();

//Using CancellationToken as a shutdown mechanism
var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{ // allows shutting down the app using Ctrl+C
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    using var client = await MatrixClient.ConnectAsync(userid, password, device, services.GetRequiredService<IHttpClientFactory>(), services.GetRequiredService<ILogger<MatrixClient>>());
    client.BeginSyncLoop();
    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token); // Keep the console open
}
catch (OperationCanceledException)
{
    Console.WriteLine("Shutdown requested...");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    await services.DisposeAsync();
}