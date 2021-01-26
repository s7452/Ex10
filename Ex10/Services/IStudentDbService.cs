using Ex10.DTOs.Requests;
using Ex10.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ex10.Services
{
    public interface IStudentDbService
    {

        IEnumerable<Student> GetStudents();
        Student GetStudent(string index);
        void EnrollStudent(EnrollStudentRequest request);
        void PromoteStudnet(int semester, string studies);
    }
}
