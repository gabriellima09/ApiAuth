using System;
using Microsoft.AspNetCore.Identity;
using ApiAuth.Data;
using ApiAuth.Models;

namespace ApiAuth.Security
{
    public class IdentityInitializer
    {
        private const string ADMIN_USERNAME = "admin_ApiAuth";
        private const string ADMIN_EMAIL = "admin_ApiAuth@teste.com";
        private const string ADMIN_PASSWORD = "AdminApiAuth01!";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async void Initialize()
        {
            if (_context.Database.EnsureCreated() 
                && await _userManager.FindByNameAsync(ADMIN_USERNAME) == null)
            {
                if (!await _roleManager.RoleExistsAsync(Roles.ROLE_API_AUTH))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(Roles.ROLE_API_AUTH));
                    
                    if (!result.Succeeded)
                        throw new Exception($"Error. Cloud not create the role {Roles.ROLE_API_AUTH}.");
                }

                CreateUser(
                    new ApplicationUser()
                    {
                        UserName = ADMIN_USERNAME,
                        Email = ADMIN_EMAIL,
                        EmailConfirmed = true
                    }, ADMIN_PASSWORD, Roles.ROLE_API_AUTH);
            }
        }

        private async void CreateUser(
            ApplicationUser user,
            string password,
            string initialRole = null)
        {
            if (await _userManager.FindByNameAsync(user.UserName) == null)
            {
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded && !string.IsNullOrWhiteSpace(initialRole))
                    await _userManager.AddToRoleAsync(user, initialRole);                
            }
        }
    }
}