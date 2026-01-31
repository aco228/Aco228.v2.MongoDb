// See https://aka.ms/new-console-template for more information

using Aco228.Common;
using Aco228.Common.Extensions;
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
var serviceProvider = builder.BuildServiceProvider();
ServiceProviderHelper.Initialize(serviceProvider);

var userRepo = MongoRepoHelpers.CreateRepo<AdsetTitleDocument>();
var allAdsets = await userRepo.Load()
    .FilterBy(x => !string.IsNullOrEmpty(x.BatchName))
    .ToListAsync();

Console.WriteLine("Hello, World!");