public class Startup 
{
        public Startup(IHostingEnvironment env)
        {
            CurrentEnvironment = env;
        }
        private IHostingEnvironment CurrentEnvironment { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {

			services.AddTransient<PayPalService, PayPalService>();
			IConfigurationSection ppSection;
			if (CurrentEnvironment.IsDevelopment())
			{
				ppSection = Configuration.GetSection("Dev:PayPal");
			}
			else
			{ // Prod
				ppSection = Configuration.GetSection("PayPal");
			}
			var ppConf = new PaypalConfig();
			ppSection.Bind(ppConf);
			// method 1
			services.Configure<PaypalConfig>(o => ppSection.Bind(o));
			// method 2
			PayPal.Api.ConfigManager.Instance.SetConfig(ppConf.Mode, ppConf.ClientId, ppConf.ClientSecret);
			

			// dev password policy
            PasswordOptions po = new PasswordOptions() { RequireDigit = false, RequiredLength = 4, RequireLowercase = false, RequireNonAlphanumeric = false, RequireUppercase = false };
			
			services.AddIdentity<AppUser, AppRole>(e => { e.Password = po; })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

			services.AddAuthentication();

		}
		
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();
		}
}