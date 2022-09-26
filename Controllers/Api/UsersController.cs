using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StartupProject_Asp.NetCore_PostGRE.Data;
using StartupProject_Asp.NetCore_PostGRE.Data.Models.Identity;
using StartupProject_Asp.NetCore_PostGRE.ViewModels.Api;
using System;
using System.Collections.Generic;
using System.Linq;
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
			if(!ModelState.IsValid)
			{
				return BadRequest(new { message = "Validation Error", success = false });
			}
			User userExists = await _userManager.FindByEmailAsync(registerVM.Email);
			if(userExists != null)
			{
				return BadRequest(new { message = $"User {registerVM.Email} already exists!", success = false });
			}

			User newUser = new User() {
				SecurityStamp = Guid.NewGuid().ToString(),
				FirstName = registerVM.FirstName,
				LastName = registerVM.LastName,
				Email = registerVM.Email,
				PhoneNumber = registerVM.PhoneNumber,
				UserName = registerVM.Email
			};
			IdentityResult result = await _userManager.CreateAsync(newUser, registerVM.Password);
			if(result.Succeeded)
			{
				return Ok(new { message = "User created", success = true });
			}
			else
			{
				return BadRequest(new { message = result.Errors, success = false });
			}
		}
	}
}
