﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShopApp.Core.Entities;
using ShopApp.Service.Dtos.AccountDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShopApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountsController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }
        //[HttpGet("createrole")]
        //public async Task<IActionResult> CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));

        //    return Ok();
        //}

        //[HttpGet("createadmin")]
        //public async Task<IActionResult> CreateAdmin()
        //{
        //    AppUser admin = new AppUser
        //    {
        //        Email = "admin@gmail.com",
        //        FullName = "Hikmet Abbasov",
        //        UserName = "hikmet_admin"
        //    };

        //    var result = await _userManager.CreateAsync(admin, "Admin123");
        //    await _userManager.AddToRoleAsync(admin, "Admin");

        //    return Ok(result);
        //}


        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto loginDto)
        {
            AppUser admin = await _userManager.FindByNameAsync(loginDto.UserName);

            if(admin == null)
                return BadRequest();

            if (!await _userManager.CheckPasswordAsync(admin, loginDto.Password))
                return BadRequest();

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,admin.Id),
                new Claim(ClaimTypes.Name,admin.UserName),
                new Claim(ClaimTypes.Email,admin.Email),
                new Claim("FullName",admin.FullName),
            };

            var roleClaims = (await _userManager.GetRolesAsync(admin)).Select(x=>new Claim(ClaimTypes.Role,x)).ToList();
            claims.AddRange(roleClaims);

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("JWT:Secret").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);


            var token = new JwtSecurityToken(
                   signingCredentials: creds,
                   claims: claims,
                   expires: DateTime.UtcNow.AddDays(3),
                   issuer: _configuration.GetSection("JWT:Issuer").Value,
                   audience: _configuration.GetSection("JWT:Audience").Value
                   );


            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new {token=tokenStr});

        }
    }
}
