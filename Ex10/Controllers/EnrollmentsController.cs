using Ex10.DTOs.Requests;
using Ex10.DTOs.Responses;
using Ex10.Models;
using Ex10.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ex10.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {

        private readonly s7452Context _context;

        public EnrollmentsController(s7452Context context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> EnrollStudentAsync(EnrollStudentRequest request)
        {

            var studies = await _context.Studies.FirstOrDefaultAsync(s => s.Name == request.Studies);
            if (studies == null)
            {
                return BadRequest();
            }

            var lastEnrollment = _context.Enrollments.AsNoTracking().OrderByDescending(e => e.IdEnrollment).First().IdEnrollment;
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.IdStudy == studies.IdStudy && e.Semester == 1);

            if (enrollment == null)
            {

                var newEnrolment = new Enrollment
                {
                    IdEnrollment = lastEnrollment+1,
                    IdStudy = studies.IdStudy,
                    Semester = 1,
                    StartDate = DateTime.Now

                };

                enrollment = newEnrolment;

                await _context.Enrollments.AddAsync(enrollment);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return Conflict();
                }
            }


            var student = await _context.Students.FirstOrDefaultAsync(stud => stud.IndexNumber == request.IndexNumber);

            if (student == null)
            {
                var newStudent = new Student
                {
                    IndexNumber = request.IndexNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.BirthDate,
                    IdEnrollment = enrollment.IdEnrollment
                };
                
                await _context.Students.AddAsync(newStudent);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return Conflict();
                }
            }

            return Ok("Enrolled");
        }

        [HttpPost("promotions")]
        public async Task<ActionResult> PromoteStudentAsync(PromoteStudentRequest request)
        {

            var studies = await _context.Studies.FirstOrDefaultAsync(s => s.Name == request.Studies);

            var lastEnrollment = _context.Enrollments.AsNoTracking().OrderByDescending(e => e.IdEnrollment).First().IdEnrollment;
            var enrollment = _context.Enrollments.AsNoTracking().FirstOrDefault(e => e.IdStudy == studies.IdStudy && e.Semester == request.Semester);

            if (enrollment == null)
            {
                return BadRequest();
            }

            var nextEnrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.IdStudy == studies.IdStudy && e.Semester == enrollment.Semester + 1);

            if (nextEnrollment == null)
            {

                var newEnrolment = new Enrollment
                {
                    IdEnrollment = lastEnrollment + 1,
                    IdStudy = studies.IdStudy,
                    Semester = enrollment.Semester+1,
                    StartDate = DateTime.Now

                };

                nextEnrollment = newEnrolment;

                await _context.Enrollments.AddAsync(nextEnrollment);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return Conflict();
                }
            }

            var students = await _context.Students.ToListAsync();

            foreach (var s in students)
            {
                if (s.IdEnrollment == enrollment.IdEnrollment)
                {
                    s.IdEnrollment = nextEnrollment.IdEnrollment;
                }
                _context.Entry(s).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Conflict();
                }
            }
            return Ok("Promoted");
        }
    }
}
