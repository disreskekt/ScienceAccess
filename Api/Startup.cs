using Api.Data;
using Api.Extensions;
using Api.Models.NvidiaSmiModels;
using Api.Options;
using Api.Services;
using Api.Services.BackgroundServices;
using Api.Services.BackgroundServices.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Context>(options =>
                options.UseNpgsql(this.Configuration.GetConnectionString("ScienceAccess")));
            
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            
            services.AddCors();

            services.AddOptions<LinuxCredentials>(this.Configuration, "Linux");
            services.AddOptions<BaseFolder>(this.Configuration, "UserFolder");
            services.AddOptions<ProgramVersionsFolder>(this.Configuration, "ProgramVersionsFolder");
            
            AuthOptions authOptions = services.AddOptions<AuthOptions>(this.Configuration, "Auth");
            services.AddJwtAuthentication(authOptions);
            services.AddSwagger("ScienceAccess");
            services.AddAutoMapper(typeof(MappingProfile));
            
            services.AddTransient<SftpService>();
            services.AddTransient<SshService>();

            services.AddTransient<AuthService>();
            services.AddTransient<UserService>();
            services.AddTransient<TicketService>();
            services.AddTransient<TaskService>();
            services.AddTransient<FileService>();
            services.AddTransient<GlobalParametersService>();
            services.AddTransient<QueueService>();

            // services.AddTransient<ITaskManagerImplementation, MguTaskManager>();
            services.AddTransient<ITaskManagerImplementation, TestTaskManager>();
            
            services.AddHostedService<TaskManager>();
            services.AddTransient<BackgroundServiceManager>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}