using System;

namespace LegacyApp
{
    public class UserService
    {
        public static bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            var user = CreateUser(firstName, lastName, email, dateOfBirth, clientId);
            SetUserCreditLimit(user, new UserCreditService());
            
            if (!UserAddable(user))
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private static bool UserAddable(User user)
        {
            return NamesEmpty(user.FirstName, user.LastName) || !IsEmail(user.EmailAddress) || GetAge(user.DateOfBirth) < 21 || UserCreditIsTooLow(user);
        }

        private static bool NamesEmpty(string firstName, string lastName)
        {
            return string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName);
        }

        private static bool IsEmail(string email)
        {
            return !email.Contains("@") && !email.Contains(".");
        }

        private static int GetAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            var age = now.Year - dateOfBirth.Year;
            
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) 
                age--;

            return age;
        }

        private static User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }
        
        private static void SetUserCreditLimit(User user, UserCreditService userCreditService)
        {
            if (user.Client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (user.Client.Type == "ImportantClient")
            {
                user.CreditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth) * 2;
            }
            else
            {
                user.HasCreditLimit = true;
                user.CreditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
            }
        }

        private static bool UserCreditIsTooLow(User user)
        {
            return user.HasCreditLimit && user.CreditLimit < 500;
        }
    }
}
