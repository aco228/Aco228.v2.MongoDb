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
var serviceProvider = builder.BuildServiceProvider();
ServiceProviderHelper.Initialize(serviceProvider);

IMongoRepo<UserDocument> userRepo = MongoRepoHelpers.CreateRepo<UserDocument>();
var transaction = userRepo.GetTransactionalManager().SetLimit(50);

var allUsers = await userRepo.LoadAllAsync();
Console.WriteLine("AllUsersCount:: " + allUsers.Count);
foreach (var user in allUsers)
{
    user.SomeData = Guid.NewGuid().ToString("N");
    transaction.InsertOrUpdate(user);
}

await transaction.FinishAsync();

userRepo.InsertOrUpdate(new()
{
    Username = Guid.NewGuid().ToString(),
    SomeIndex = new Random().Next(1, 10),
});

var users = await userRepo.Project<UserProjection>()
    .FilterBy(x => x.SomeIndex < 5)
    .Limit(5)
    .OrderByPropertyNameDesc(nameof(UserDocument.SomeIndex))
    .ToListAsync();

Console.WriteLine("Hello, World!");