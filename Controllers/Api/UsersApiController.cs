using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StartupProject_Asp.NetCore_PostGRE.AuthorizationRequirement;
using StartupProject_Asp.NetCore_PostGRE.Data;
using StartupProject_Asp.NetCore_PostGRE.Data.Enums;
using StartupProject_Asp.NetCore_PostGRE.Data.Models.AppData;

namespace StartupProject_Asp.NetCore_PostGRE.Controllers.Api
{
	[Route("api/[controller]")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UsersApi/5
        [AuthorizePolicy(EClaim.UsersApi_Get)]
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveApplication>> GetLeaveApplication(Guid? id)
        {
            var leaveApplication = await _context.LeaveApplications.FindAsync(id);

            if (leaveApplication == null)
            {
                return NotFound();
            }

            return leaveApplication;
        }

        // GET: api/UsersApi
        [AuthorizePolicy(EClaim.UsersApi_GetAll)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveApplication>>> GetLeaveApplications(int startIndex = 0, int itemsPerPage = 2)
        {
            return await _context.LeaveApplications.ToListAsync();
        }

        // PUT: api/UsersApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [AuthorizePolicy(EClaim.UsersApi_Update)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLeaveApplication(Guid? id, LeaveApplication leaveApplication)
        {
            if (id != leaveApplication.Id)
            {
                return BadRequest();
            }

            _context.Entry(leaveApplication).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeaveApplicationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UsersApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [AuthorizePolicy(EClaim.UsersApi_Create)]
        [HttpPost]
        public async Task<ActionResult<LeaveApplication>> PostLeaveApplication(LeaveApplication leaveApplication)
        {
            _context.LeaveApplications.Add(leaveApplication);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLeaveApplication", new { id = leaveApplication.Id }, leaveApplication);
        }

        // DELETE: api/UsersApi/5
        [AuthorizePolicy(EClaim.UsersApi_Delete)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<LeaveApplication>> DeleteLeaveApplication(Guid? id)
        {
            var leaveApplication = await _context.LeaveApplications.FindAsync(id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

            _context.LeaveApplications.Remove(leaveApplication);
            await _context.SaveChangesAsync();

            return leaveApplication;
        }

        private bool LeaveApplicationExists(Guid? id)
        {
            return _context.LeaveApplications.Any(e => e.Id == id);
        }
    }
}
