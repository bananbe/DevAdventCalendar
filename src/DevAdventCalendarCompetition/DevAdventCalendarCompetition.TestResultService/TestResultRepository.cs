using System;
using System.Collections.Generic;
using System.Linq;
using DevAdventCalendarCompetition.Repository.Context;
using DevAdventCalendarCompetition.Repository.Models;
using DevAdventCalendarCompetition.TestResultService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevAdventCalendarCompetition.TestResultService
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TestResultRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string[] GetUsersId()
        {
            return this._dbContext
                .Users
                .Select(u => u.Id)
                .ToArray();
        }

        public double GetAnsweringTimeSum(string userId, DateTime dateFrom, DateTime dateTo)
        {
            return _dbContext
                .UserTestCorrectAnswers
                .Where(a => a.UserId == userId && a.AnsweringTime > dateFrom && a.AnsweringTime <= dateTo)
                .Sum(a => a.AnsweringTimeOffset.TotalMilliseconds);
        }
        
        public IEnumerable<DateTime> GetCorrectAnswersDates(string userId, DateTime dateFrom, DateTime dateTo)
        {
            return _dbContext
                .UserTestCorrectAnswers
                .Where(a => a.UserId == userId)
                .Where(a => a.Test.StartDate.Value >= dateFrom && a.Test.StartDate.Value < dateTo)
                .Where(a => a.AnsweringTime > dateFrom && a.AnsweringTime <= dateTo)
                .Select(a => a.Test.StartDate.Value.Date);
        }

        public List<Result> GetFinalResults()
        {
            return _dbContext
                .Results
                .ToList();
        }

        public IEnumerable<WrongAnswerData> GetWrongAnswersCountPerDay(string userId, DateTime dateFrom, DateTime dateTo)
        {
            return _dbContext
                .UserTestWrongAnswers
                .Where(a => a.Time >= dateFrom && a.Time <= dateTo && a.UserId == userId)
                .Select(t => new { TestStartDate = t.Test.StartDate.Value.Date })
                .AsEnumerable()
                .GroupBy(a => a.TestStartDate)
                .Select(a => new WrongAnswerData(a.Key, a.Count()))
                .ToArray();
        }

        public void SaveUserFinalPlace(string userId, int place)
        {
            var userResult = _dbContext
                .Results
                .FirstOrDefault(r => r.UserId == userId);

            if (userResult != null)
            {
                userResult.FinalPlace = place;
                _dbContext.Entry(userResult).Property(p => p.FinalPlace).IsModified = true;
                _dbContext.Update(userResult);
            }
            else
            {
                _dbContext.Results.Add(new Result { UserId = userId, FinalPlace = place });
            }

            _dbContext.SaveChanges();
        }

        public void SaveUserFinalScore(string userId, int score)
        {
            var userResult = _dbContext
                .Results
                .FirstOrDefault(r => r.UserId == userId);

            if (userResult != null)
            {
                userResult.FinalPoints = score;
                _dbContext.Entry(userResult).Property(p => p.FinalPoints).IsModified = true;
                _dbContext.Update(userResult);
            }
            else
            {
                _dbContext.Results.Add(new Result { UserId = userId, FinalPoints = score });
            }

            _dbContext.SaveChanges();
        }

        public void SaveUserWeeklyPlace(string userId, int weekNumber, int place)
        {
            Console.WriteLine($"\nGoing to save place for user { userId } and week { weekNumber }...");

            var userResult = _dbContext
                .Results
                .FirstOrDefault(r => r.UserId == userId);

            try
            {
                if (userResult != null)
                {
                    var property = userResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.Name.Contains($"Week{weekNumber.ToString()}Place"));

                    if (property != null)
                        property.SetValue(userResult, place);
                    else
                        throw new ArgumentException($"Missing week {weekNumber} in model.");

                    _dbContext.Update(userResult);
                }
                else
                {
                    var newResult = new Result {UserId = userId};

                    var property = newResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.Name.Contains($"Week{weekNumber.ToString()}Place"));

                    if (property != null)
                        property.SetValue(newResult, place);
                    else
                        throw new ArgumentException($"Missing week {weekNumber} in model.");

                    _dbContext.Results.Add(newResult);
                }

                _dbContext.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine($"\nAn error occurred during saving place for user { userId } and week { weekNumber }: { e.Message }");
            }
        }

        public void SaveUserWeeklyScore(string userId, int weekNumber, int score)
        {
            Console.WriteLine($"\nGoing to save score for user { userId } and week { weekNumber }...");

            var userResult = _dbContext
                .Results
                .FirstOrDefault(r => r.UserId == userId);

            try
            {
                if (userResult != null)
                {
                    var property = userResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.Name.Contains($"Week{weekNumber.ToString()}Points"));

                    if (property != null)
                        property.SetValue(userResult, score);
                    else
                        throw new ArgumentException($"Missing week {weekNumber} in model.");

                    _dbContext.Update(userResult);
                }
                else
                {
                    var newResult = new Result {UserId = userId};

                    var property = newResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.Name.Contains($"Week{weekNumber.ToString()}Points"));

                    if (property != null)
                        property.SetValue(newResult, score);
                    else
                        throw new ArgumentException($"Missing week {weekNumber} in model.");

                    _dbContext.Results.Add(newResult);
                }

                _dbContext.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine($"\nAn error occurred during saving score for user { userId } and week { weekNumber }: { e.Message }");
            }
        }

        public ApplicationUser GetUserById(string id)
        {
            return _dbContext
                .Users
                .FirstOrDefault(r => r.Id == id);
        }

        /*  [in #1] ID of user that wants to know count of wrong answers
         *  [in #2] ID of test that user wants to know count of wrong answers
         * 
         *  [out] Count of submitted wrong answers
         */
        public int GetWrongAnswersToTest(string userId, int testID) // get count of wrong answers
        {
            return _dbContext
                .UserTestWrongAnswers
                .Where(a => a.TestId == testID)
                .Where(a => a.UserId == userId)
                .Count();
        }

        /*  [in]#1 ID of user that wants to know when he submitted correct answer
         *  [in]#2 ID of test that user wants to know when he submitted correct answer
         * 
         *  [out] Date of submission of corect answer
         */
        public DateTime GetCorrectAnswerDate(string userId, int testID) // get DateTime of correct answer
        {
            return _dbContext
                .UserTestCorrectAnswers
                .Where(a => a.UserId == userId)
                .Where(a => a.TestId == testID)
                .Select(a => a.AnsweringTime) 
                .First(); // I assume there is only one row
        }
    }
}
