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
var user = userRepo.Track().FirstOrDefault(x => x.Username == "aco")!; // - here is tracking called


user.Hash.Add("tri");
user.Extra.Name = "Aleksandar";

await user.UpdateFieldsAsync();
Console.WriteLine("Hello, World!");