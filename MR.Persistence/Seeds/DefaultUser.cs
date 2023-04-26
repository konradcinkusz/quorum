namespace MR.Persistence.Seeds;

public static class DefaultUser
{
    public static List<ApplicationUser> IdentityBasicUserList()
    {
        return new List<ApplicationUser>()
            {
                new ApplicationUser
                {
                    Id = Constants.SuperAdminUser,
                    UserName = "superadmin@gmail.com",
                    Email = "superadmin@gmail.com",
                    FirstName = "Amit",
                    LastName = "Naik",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    // Password@123
                    PasswordHash = "AQAAAAEAACcQAAAAEBLjouNqaeiVWbN0TbXUS3+ChW3d7aQIk/BQEkWBxlrdRRngp14b0BIH0Rp65qD6mA==",
                    NormalizedEmail= "SUPERADMIN@GMAIL.COM",
                    NormalizedUserName="SUPERADMIN@GMAIL.COM",
                    ConcurrencyStamp = "B87AD966-7EA1-4696-8107-B190AEAB837D",
                    SecurityStamp = "A94F51C0-E605-43E6-819D-95C43ACE65D3"
                },
                new ApplicationUser
                {
                    Id = Constants.BasicUser,
                    UserName = "basicuser@gmail.com",
                    Email = "basicuser@gmail.com",
                    FirstName = "Basic",
                    LastName = "User",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    // Password@123
                    PasswordHash = "AQAAAAEAACcQAAAAEBLjouNqaeiVWbN0TbXUS3+ChW3d7aQIk/BQEkWBxlrdRRngp14b0BIH0Rp65qD6mA==",
                    NormalizedEmail= "BASICUSER@GMAIL.COM",
                    NormalizedUserName="BASICUSER@GMAIL.COM",
                    ConcurrencyStamp = "A63E25A3-1237-4BD0-A5FC-5C8359885E9E",
                    SecurityStamp = "33135628-31F0-4995-861D-CE867EBA0FBE"
                },
            };
    }
}