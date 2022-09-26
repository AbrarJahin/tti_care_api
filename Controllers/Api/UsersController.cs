using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StartupProject_Asp.NetCore_PostGRE.Data;
using StartupProject_Asp.NetCore_PostGRE.Data.Models.Identity;
using StartupProject_Asp.NetCore_PostGRE.ViewModels.Api;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StartupProject_Asp.NetCore_PostGRE.Controllers.Api
{
	[Route("api/[controller]/{action}/{id?}")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<Role> _roleManager;
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;

		public UsersController(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context, IConfiguration configuration)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_context = context;
			_configuration = configuration;
		}

		//[HttpPost("register")]
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register([FromBody] RegisterViewModel registerVM)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Validation Error", success = false });
			}
			User userExists = await _userManager.FindByEmailAsync(registerVM.Email);
			if (userExists != null)
			{
				return BadRequest(new { message = $"User {registerVM.Email} already exists!", success = false });
			}

			User newUser = new User()
			{
				SecurityStamp = Guid.NewGuid().ToString(),
				FirstName = registerVM.FirstName,
				LastName = registerVM.LastName,
				Email = registerVM.Email,
				PhoneNumber = registerVM.PhoneNumber,
				UserName = registerVM.Email,
				EmailConfirmed = true
			};
			IdentityResult result = await _userManager.CreateAsync(newUser, registerVM.Password);
			if (result.Succeeded)
			{
				//As role is already created, we can only add user
				IdentityResult addRoleResult = await _userManager.AddToRoleAsync(newUser, registerVM.Role);
				if (!addRoleResult.Succeeded)
				{
					return BadRequest(new { message = "User created but role does not assigned", success = false });
				}
				return Ok(new { message = $"User created as {registerVM.Role}", success = true });
			}
			else
			{
				return BadRequest(new { message = result.Errors, success = false });
			}
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel loginVM)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Please provide all required fields", success = false });
			}

			User foundUser = await _userManager.FindByEmailAsync(loginVM.Email);
			if(foundUser!=null && await _userManager.CheckPasswordAsync(foundUser, loginVM.Password))
			{
				//Password is good, so need to add access token in here
				var tokenValue = await GenerateJWTTokenAsync(foundUser);

				return Ok(new { message = "User logged in", success = true, token = tokenValue });
			}
			else
			{
				return Unauthorized(new { message = "User failed loggin in", success = false });
			}
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAllUsersAsync(string userType)
		{
			if (string.IsNullOrEmpty(userType))
			{
				return Ok(new { message = "All users list", success = true });
			}
			else if(userType == "Doctor")
			{
				return Ok("All Doctor's list");
			}
			else if (userType == "Patient")
			{
				return Ok("All Patient list");
			}
			else
			{
				return BadRequest($"Can't return {userType} users");
			}
			throw new ArgumentException($"'{nameof(userType)}' cannot be null or empty.", nameof(userType));
		}

		private async Task<JwtViewModel> GenerateJWTTokenAsync(User user)
		{
			var authClaims = new List<Claim>() {
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.NameIdentifier, user.Email),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};
			var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secreat"]));
			var token = new JwtSecurityToken(
					issuer: _configuration["JWT:Issuer"],
					audience: _configuration["JWT:Audience"],
					expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JWT:AccessTokenTimeoutInMinutes"])),
					claims: authClaims,
					signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
				);
			//int minuites = Convert.ToInt32(_configuration["JWT:AccessTokenTimeoutInMinutes"]);//////
			string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
			JwtViewModel response = new JwtViewModel() {
				Token = jwtToken,
				ExpiresAt = token.ValidTo
			};
			return response;
		}
	}
}
