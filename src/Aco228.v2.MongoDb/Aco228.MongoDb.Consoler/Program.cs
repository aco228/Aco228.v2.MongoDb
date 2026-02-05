// See https://aka.ms/new-console-template for more information

using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Consoler.Database;
using Aco228.MongoDb.Consoler.Database.Documents;
using Aco228.MongoDb.Extensions;
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
var user = await userRepo.Load().FirstOrDefaultAsync(x => x.Username == "aco");
if (user == null) throw new Exception("User not found");
user.StartTracking();

user.SomeData = "RokiBejbe + Some new data " + Guid.NewGuid();
user.SomeExtraData = "Extra + Some new data " + Guid.NewGuid();

await userRepo.InsertOrUpdateFieldsAsync(user);
Console.WriteLine("Hello, World!");