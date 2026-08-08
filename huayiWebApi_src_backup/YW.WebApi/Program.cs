using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.TenPayV3;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 配置 Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterAssemblyTypes(System.Reflection.Assembly.Load("YW.Service"));
    //containerBuilder.RegisterType<UserInfoService>();
});

// 配置 Configuration
var configuration = builder.Configuration;
LogHelper.Configure(); //使用前先配置

// 添加服务
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    o.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
    options.SerializerSettings.DateFormatString = "yyyy/MM/dd HH:mm:ss";
    //options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilterAttribute>();
    options.Filters.Add<ApiExceptionFilterAttribute>();
});
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// 加载数据库连接字符串
PubConstant.ConnectionString = ConfigHelper.GetSectionValue("AppSettings:ConnectionString");
// 加载系统配置
PubConstant.Config = XmlHelper.ReadXml<ApiConfigDto>($"{System.IO.Directory.GetCurrentDirectory()}/Config/apiConfig.xml");

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
var redisConnectionString = ConfigHelper.GetSectionValue("RedisConnectionStrings:Connection");
var redisInstanceName = ConfigHelper.GetSectionValue("RedisConnectionStrings:InstanceName");
builder.Services.AddSingleton(new RedisCacheHelper(redisConnectionString, redisInstanceName));

// 注册跨域策略
builder.Services.AddCorsPolicy(configuration);
// 注册webcore服务
builder.Services.AddWebCoreService(configuration);
// 注册jwt服务
builder.Services.AddJwtService(configuration);
// 微信相关服务
builder.Services.AddSenparcGlobalServices(configuration).AddSenparcWeixinServices(configuration);
//builder.Services.AddCertHttpClient(PubConstant.Config.mch_id + "_", PubConstant.Config.mch_id, $"{System.IO.Directory.GetCurrentDirectory()}/{PubConstant.Config.certPath}");

// 注册定时任务服务
builder.Services.AddQuartz(q =>
{
    //var everydayJobKey = new JobKey("everydayTask");
    //q.AddJob<everydayTask>(opts => opts.WithIdentity(everydayJobKey));
    //q.AddTrigger(opts => opts.ForJob(everydayJobKey).WithIdentity("everydayTask-trigger").WithCronSchedule("0 0 0 * * ?"));
    var perMinuteJobKey = new JobKey("perMinuteTask");
    q.AddJob<perMinuteTask>(opts => opts.WithIdentity(perMinuteJobKey));
    q.AddTrigger(opts => opts.ForJob(perMinuteJobKey).WithIdentity("perMinuteTask-trigger").WithCronSchedule("0 0/1 * * * ?"));

});
builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true;
});

// 文件上传限制
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MemoryBufferThreshold = int.MaxValue;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});

// 添加 限流 服务
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = (context, cancellationToken) =>
    {
        //// 获取策略名称
        //var policyName = context.HttpContext.GetEndpoint()?
        //    .Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName
        //    ?? "unknown";
        //context.Lease?.RetryAfter?.TotalSeconds
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var result = System.Text.Json.JsonSerializer.Serialize(new ResultModel
        {
            data = "操作过于频繁",
            msg = "操作频率超过限制",
            code = (int)ResultEnum.fail
        });
        context.HttpContext.Response.ContentType = "application/json";
        return new System.Threading.Tasks.ValueTask(context.HttpContext.Response.WriteAsync(result, cancellationToken));
    };

    options.AddPolicy("UserPayOrder", context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "anonymous";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: userId,
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,// 按顺序处理队列
                PermitLimit = 1,// 只允许1次请求
                Window = TimeSpan.FromSeconds(5),
                SegmentsPerWindow = 5,
                QueueLimit = 0// 不允许排队                
            });
    });
});

var app = builder.Build();

// 静态文件和默认首页
DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});
PubConstant.Accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
app.UseRouting();
app.UseCors(WebCoreExtensions.MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
// 添加这一行 - 必须在 UseRouting 和 MapControllers 之间
app.UseRateLimiter();// 启用 限流中间件

app.MapControllers();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Upload")),
    RequestPath = "/Upload",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=36000");
    }
});

var senparcSetting = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SenparcSetting>>().Value;
var senparcWeixinSetting = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SenparcWeixinSetting>>().Value;
IRegisterService register = RegisterService.Start(senparcSetting).UseSenparcGlobal();
register.UseSenparcWeixin(senparcWeixinSetting, senparcSetting).RegisterTenpayApiV3(senparcWeixinSetting, "微信支付（ApiV3）");//微信全局注册，必须！
//await Senparc.Weixin.MP.Containers.AccessTokenContainer.RegisterAsync(senparcWeixinSetting.WeixinAppId, senparcWeixinSetting.WeixinAppSecret);
await Senparc.Weixin.WxOpen.Containers.AccessTokenContainer.RegisterAsync(senparcWeixinSetting.WxOpenAppId, senparcWeixinSetting.WxOpenAppSecret);

app.Run();
