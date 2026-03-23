using HelpDesk.Domain.IdentityAccess.ValueObjects;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.IdentityAccess.Aggregates
{
    public sealed class User : AggregateRoot<int>
    {
        public UserName Name { get; private set; }
        public EmailAddress Email { get; private set; }
        public UserRole Role { get; private set; }

        private User() { }

        private User(int id, UserName name, EmailAddress email, UserRole role) : base(id)
        {
            Name = name;
            Email = email;
            Role = role;
        }

        public static User CreateNew(UserName name, EmailAddress email, UserRole role)
            => new(0, name, email, role);

        public void UpdateName(UserName newName) => Name = newName;
        public void UpdateEmail(EmailAddress newEmail) => Email = newEmail;
        public void UpdateRole(UserRole newRole) => Role = newRole;
    }
}
