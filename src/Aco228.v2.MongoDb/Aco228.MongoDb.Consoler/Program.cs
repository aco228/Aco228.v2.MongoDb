// See https://aka.ms/new-console-template for more information

using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Consoler.Database;
using Aco228.MongoDb.Consoler.Database.Documents;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Helpers;
using Aco228.MongoDb.Services;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;

Env.Load();

var builder = new ServiceCollection();
builder.RegisterServicesFromAssembly(typeof(Program).Assembly);
builder.RegisterRepositoriesFromAssembly<ILocalDbContext>();
var serviceProvider = await builder.BuildCollection();

var userRepo = serviceProvider.GetService<IMongoRepo<UserDocument>>()!;
var tmng = userRepo.GetTransactionalManager().SetLimit(3);
foreach (var user in userRepo.LoadAll())
{
    user.SetBck = Guid.NewGuid().ToString();
    tmng.InsertOrUpdate(user);
}
await tmng.FinishAsync();

Console.WriteLine("Hello, World!");