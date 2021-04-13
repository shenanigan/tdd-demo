using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TDD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : Controller
    {
        private readonly DataContext _context;

        public PatientController(DataContext context)
        {
            _context = context;
        }
        
        [HttpPost]
        public async Task<IActionResult> AddPatientAsync([FromBody] Patient Patient)
        {
            var FetchedPatient = await _context.Patient.FirstOrDefaultAsync(x => x.PhoneNumber == Patient.PhoneNumber);
            // If the patient doesn't exist create a new one
            if (FetchedPatient == null)
            {
                _context.Patient.Add(Patient);
                await _context.SaveChangesAsync();
                return Created($"/patient/{Patient.Id}", Patient);
            }
            // Else throw a bad request
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public ActionResult<List<Patient>> GetPatients([FromQuery] String Search)
        {
            // Search for a patient based on Name and PhoneNumber
            return Ok(_context.Patient.Where(x => x.PhoneNumber.Contains(Search) || x.Name.Contains(Search)));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AdmitAsync([FromBody] RoomPatient RoomPatient)
        {
            var Patient = await _context.Patient.FirstOrDefaultAsync(x => x.Id == RoomPatient.PatientId && x.IsAdmitted == false);
            // If we are not able to find the patient or if the patient is already admitted return BadRequest
            if (Patient == null)
            {
                return BadRequest();
            }

            var Room = await _context.Room.FirstOrDefaultAsync(x => x.Id == RoomPatient.RoomId && x.CurrentCapacity > 0);
            // If we are not able to find the room or the rooms capacity is 0 return BadRequest
            if (Room == null)
            {
                return BadRequest();
            }

            // Admit the patient
            Patient.IsAdmitted = true;

            // Decrease the capacity of the room by 1
            Room.CurrentCapacity = Room.CurrentCapacity - 1;

            _context.Patient.Update(Patient);
            _context.Room.Update(Room);

            // Log that a Patient is now added to a particular Room
            _context.Add(RoomPatient);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CheckoutAsync([FromBody] Patient P)
        {
            var Patient = await _context.Patient.FirstOrDefaultAsync(x => x.Id == P.Id && x.IsAdmitted == true);
            // If we are not able to find the patient or if the patient is not admitted return BadRequest
            if (Patient == null)
            {
                return BadRequest();
            }

            var RoomPatient = await _context.RoomPatient.Include(x => x.Room).FirstOrDefaultAsync(x => x.PatientId == P.Id);
            // If we are not able to find a room in which the patient is admitted return BadRequest
            if (RoomPatient == null)
            {
                return BadRequest();
            }

            var Room = RoomPatient.Room;
            // Increase the current capacity of the room
            Room.CurrentCapacity = Room.CurrentCapacity + 1;
            
            // Checkout the patient
            Patient.IsAdmitted = false;
            
            // Remove the entry that says the patient is admitted to a particular room
            _context.RoomPatient.Remove(RoomPatient);
            _context.Patient.Update(Patient);
            _context.Room.Update(Room);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}