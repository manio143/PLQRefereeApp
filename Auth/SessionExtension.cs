using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public static class SessionExtension
    {
        public static User GetUser(this ISession session, UserRepository userRepository) {
            return userRepository.GetUser(session.GetInt32("UserId").Value);
        }
        public static Test GetTest(this ISession session, TestRepository testRepository) {
            return testRepository.GetTest(session.GetInt32("TestId").Value);
        }
    }
}