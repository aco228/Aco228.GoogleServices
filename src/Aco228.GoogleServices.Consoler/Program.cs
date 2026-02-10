// See https://aka.ms/new-console-template for more information

using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.GoogleServices;
using Aco228.GoogleServices.Consoler.Buckets;
using Aco228.GoogleServices.Services;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = await ServiceProviderHelper.Construct(typeof(Program), builder =>
{
    builder.RegisterServicesFromAssembly(typeof(Program).Assembly);
    builder.RegisterGoogleServices(new()
    {
        ServiceAccountCredentialsName = "arbo-487008-38359e7d2b41"
    });
});

var tempBucket = serviceProvider.GetService<ITempBucket>()!;
var file = new FileInfo(@"C:\Users\Lenovo\Documents\ArbitrageSoulsStorage\Application\_CanvaSamples\cnv_1975b61a3c3b46e6bcc4fbbd34bb8277.jpg");
var uploadedFile = await tempBucket.UploadFileAsync(file);

Console.WriteLine("Hello, World!");