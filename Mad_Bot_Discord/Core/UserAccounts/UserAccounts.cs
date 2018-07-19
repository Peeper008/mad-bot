using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Core.UserAccounts
{
    public static class UserAccounts
    {
        private static List<UserAccount> accounts;

        // The path to the accounts file.
        private static string accountsFile = "Resources/accounts.json";

        // Runs immediately.
        static UserAccounts()
        {

            // Checks if the file accountsFile exists.
            if (DataStorage.SaveExists(accountsFile))
            {
                // If it does, it loads the entire file (every single account) into a UserAccount list.
                accounts = DataStorage.LoadUserAccounts(accountsFile).ToList();
            }
            else
            {
                // If it doesn't, it creates a new UserAccount list and saves that.
                accounts = new List<UserAccount>();
                SaveAccounts();
            }

            // The accountsFile file will always exist after this is run.
        }

        // Saves all user accounts.
        public static void SaveAccounts()
        {
            DataStorage.SaveUserAccounts(accounts, accountsFile);
        }

        // Gets a specific account.
        public static UserAccount GetAccount(SocketUser user)
        {
            return GetOrCreateAccount(user.Id);
        }


        private static UserAccount GetOrCreateAccount(ulong id)
        {
            // LINQ searching.
            var result = from a in accounts
                         where a.ID == id
                         select a;

            // Takse the first (or default) account from the result variable.
            var account = result.FirstOrDefault();

            // If the account is null (as in it didn't find anything), it creates the user account.
            if (account == null) account = CreateUserAccount(id);

            // Returns the account.
            return account;
        }

        // Creates a user account.
        private static UserAccount CreateUserAccount(ulong id)
        {
            // Creates a new account with the UserAccount constructor.
            var newAccount = new UserAccount()
            {
                ID = id,
                XP = 0
            };

            // Adds the new account to the accounts list.
            accounts.Add(newAccount);

            // Saves all accounts (including the new one).
            SaveAccounts();

            // Returns the new account.
            return newAccount;
        }
    }
}
