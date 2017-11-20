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
        public UserData GetUserData(User user) => Context.UserData.First(data => data.Id == user.Id);
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
            if (Context.Users.Contains(user))
                throw new ArgumentException("Cannot add an object for the seconds time.");
            Context.Users.Add(user);
            Context.UserData.Add(data);
            Context.SaveChanges();
        }
        public void CleanUserCooldowns(User user)
        {
            var data = GetUserData(user);
            if (data.Arcooldown.HasValue && data.Arcooldown.Value > DateTime.Now) data.Arcooldown = null;
            if (data.Srcooldown.HasValue && data.Srcooldown.Value > DateTime.Now) data.Srcooldown = null;
            if (data.Hrcooldown.HasValue && data.Hrcooldown.Value > DateTime.Now) data.Hrcooldown = null;
            Context.SaveChanges();
        }
    }
}