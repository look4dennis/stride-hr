using System;
using System.Collections.Generic;
using StrideHR.Core.Entities;
using StrideHR.Core.Models;

namespace StrideHR.Test
{
    public class TrainingSystemTest
    {
        public static void TestTrainingEntities()
        {
            // Test TrainingModule entity
            var trainingModule = new TrainingModule
            {
                Title = "Test Training Module",
                Description = "Test Description",
                Type = TrainingType.OnlineModule,
                Level = TrainingLevel.Beginner,
                EstimatedDurationMinutes = 60,
                IsMandatory = true,
                IsActive = true
            };

            // Test Assessment entity
            var assessment = new Assessment
            {
                TrainingModuleId = 1,
                Title = "Test Assessment",
                Type = AssessmentType.Quiz,
                TimeLimit = 30,
                PassingScore = 70,
                MaxAttempts = 3,
                IsActive = true
            };

            // Test AssessmentQuestion entity
            var question = new AssessmentQuestion
            {
                AssessmentId = 1,
                QuestionText = "What is 2+2?",
                Type = QuestionType.MultipleChoice,
                Options = new List<string> { "3", "4", "5" },
                CorrectAnswers = new List<string> { "4" },
                Points = 1,
                OrderIndex = 1,
                IsActive = true
            };

            // Test Certification entity
            var certification = new Certification
            {
                TrainingModuleId = 1,
                EmployeeId = 1,
                CertificationName = "Test Certification",
                CertificationNumber = "CERT-2024-000001",
                IssuedDate = DateTime.UtcNow,
                Status = CertificationStatus.Active
            };

            // Test DTOs
            var moduleDto = new TrainingModuleDto
            {
                Id = 1,
                Title = "Test Module",
                Type = TrainingType.OnlineModule,
                Level = TrainingLevel.Beginner
            };

            var assessmentDto = new AssessmentDto
            {
                Id = 1,
                Title = "Test Assessment",
                Type = AssessmentType.Quiz,
                PassingScore = 70
            };

            Console.WriteLine("Training system entities and DTOs created successfully!");
            Console.WriteLine($"Training Module: {trainingModule.Title}");
            Console.WriteLine($"Assessment: {assessment.Title}");
            Console.WriteLine($"Question: {question.QuestionText}");
            Console.WriteLine($"Certification: {certification.CertificationName}");
        }
    }
}