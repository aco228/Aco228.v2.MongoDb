// See https://aka.ms/new-console-template for more information

using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Consoler.Database.Documents;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Helpers;
using Aco228.MongoDb.Services;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;

Env.Load();

var builder = new ServiceCollection();
builder.RegisterServicesFromAssembly(typeof(Program).Assembly);
builder.RegisterRepositoriesFromAssembly(typeof(Program).Assembly);
var serviceProvider = builder.BuildServiceProvider();
ServiceProviderHelper.Initialize(serviceProvider);
await typeof(Program).Assembly.ConfigureMongoIndexesFromAssembly();

var userRepo = serviceProvider.GetService<IMongoRepo<UserDocument>>();

for (;;)
{
    
}

Console.WriteLine("Hello, World!");