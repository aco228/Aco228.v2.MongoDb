// See https://aka.ms/new-console-template for more information

using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Consoler.Database;
using Aco228.MongoDb.Consoler.Database.Documents;
using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Extensions.MongoDocuments;
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
var users = userRepo.TrackProject<UserProjection>().ToList();

int index = 0;
var rnd = new Random();
foreach (var userProjection in users)
{
    index++;
    if (index % 2 == 0) continue;
    userProjection.DasIstIndex = rnd.Next(1, 100);
}


await users.UpdateAsync();

// await allUsers.UpdateFieldsAsync();
Console.WriteLine("Hello, World!");