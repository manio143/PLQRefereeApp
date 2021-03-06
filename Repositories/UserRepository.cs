using System;
using System.Collections.Generic;
using System.Linq;

namespace PLQRefereeApp
{
    public class UserRepository
    {
        public UserRepository(MainContext mainCtx)
        {
            Context = mainCtx;
        }

        private MainContext Context { get; }

        public User GetUser(string email) => Context.Users.First(user => user.Email == email);
        public User GetUser(int id) => Context.Users.FirstOrDefault(user => user.Id == id);
        public UserData GetUserData(User user) => GetUserData(user.Id);
        public UserData GetUserData(int id) => Context.UserData.First(data => data.Id == id);
        public IEnumerable<UserData> GetAllUserData()
        {
            return (from data in Context.UserData
                    orderby data.Surname
                    orderby data.Team
                    select data);
        }

        public bool UserExists(string email) => Context.Users.Any(user => user.Email == email);

        public void AddUser(User user, UserData data)
        {
            if (Context.Users.Any(u => u.Id == user.Id))
                throw new ArgumentException("Cannot add an object for the seconds time.");
            Context.Users.Add(user);
            Context.SaveChanges();
            data.Id = user.Id;
            Context.UserData.Add(data);
            Context.SaveChanges();
        }
        public void CleanUserCooldowns(User user)
        {
            var data = GetUserData(user);
            if (data.Arcooldown.HasValue && data.Arcooldown.Value < DateTime.Now) data.Arcooldown = null;
            if (data.Srcooldown.HasValue && data.Srcooldown.Value < DateTime.Now) data.Srcooldown = null;
            if (data.Hrcooldown.HasValue && data.Hrcooldown.Value < DateTime.Now) data.Hrcooldown = null;
            Context.SaveChanges();
        }

        internal void AddCertificate(User user, QuestionType questionType, Test test)
        {
            var data = GetUserData(user);
            switch (questionType)
            {
                case QuestionType.AR: data.Ar = test.Id; break;
                case QuestionType.SR: data.Sr = test.Id; break;
                case QuestionType.HR: data.Hr = test.Id; break;
            }
            Context.SaveChanges();
        }

        internal void ChangePassword(User user, string passphrase)
        {
            user.Passphrase = passphrase;
            Context.SaveChanges();
        }

        internal void SetCooldown(User user, QuestionType questionType)
        {
            var data = GetUserData(user);
            switch (questionType)
            {
                case QuestionType.AR: data.Arcooldown = DateTime.Now.AddDays(1); break;
                case QuestionType.SR: data.Srcooldown = DateTime.Now.AddDays(1); break;
                case QuestionType.HR: data.Hrcooldown = DateTime.Now.AddDays(2); break;
            }
            Context.SaveChanges();
        }

        internal void ChangeTeam(User user, string team)
        {
            var data = GetUserData(user);
            data.Team = String.IsNullOrWhiteSpace(team) ? String.Empty : team;
            Context.SaveChanges();
        }
    }
}
