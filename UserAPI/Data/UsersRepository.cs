using UserAPI.Models.Core;
using UserAPI.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UserAPI.Data;
/// <summary>
/// Репозиторий пользователей на EF Core.
/// </summary>
public sealed class UsersRepository : IUsersRepository
    {
        private readonly UsersDbContext _db;

        public UsersRepository(UsersDbContext db) => _db = db;

        public async Task<User> CreateAsync(
            string login,
            string password,
            string name,
            Gender gender,
            DateTime? birthday,
            bool isAdmin,
            Guid createdBy,
            CancellationToken cancellationToken = default)
        {
            if (await _db.Users.AnyAsync(u => u.Login == login, cancellationToken))
                throw new InvalidOperationException($"Login '{login}' is already taken.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = login,
                Password = password,  
                Name = name,
                Gender = gender,
                Birthday = birthday,
                Admin = isAdmin,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _db.Users.AddAsync(user, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<IReadOnlyList<User>> GetActiveOrderedAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Users
                .AsNoTracking()
                .Where(u => u.RevokedOn == null)
                .OrderBy(u => u.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken) =>
            _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login, cancellationToken);

        public Task<User?> GetByCredentialsAsync(string login, string password, CancellationToken cancellationToken = default) =>
            _db.Users.AsNoTracking()
                     .FirstOrDefaultAsync(u => u.Login == login &&
                                               u.Password == password &&
                                               u.RevokedOn == null,
                                                 cancellationToken);

        public async Task<IReadOnlyList<User>> GetOlderThanAsync(int ageYears, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.Date.AddYears(-ageYears);

            return await _db.Users.AsNoTracking()
                                  .Where(u => u.Birthday != null && u.Birthday <= cutoffDate)
                                  .ToListAsync(cancellationToken);
        }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);


        public async Task<bool> UpdateProfileAsync(
            Guid userId,
            string? newName,
            Gender? newGender,
            DateTime? newBirthday,
            Guid modifiedBy,
            CancellationToken cancellationToken)
        {
            var user = await _db.Users.FindAsync(userId, cancellationToken);
            if (user == null || user.RevokedOn != null) return false;

            if (newName != null) user.Name = newName;
            if (newGender != null) user.Gender = newGender.Value;
            user.Birthday = newBirthday;

            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string newPassword, Guid modifiedBy, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.RevokedOn != null) return false;

            user.Password = newPassword;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ChangeLoginAsync(Guid userId, string newLogin, Guid modifiedBy, CancellationToken cancellationToken = default)
        {
            if (await _db.Users.AnyAsync(u => u.Login == newLogin && u.Id != userId))
                return false;

            var user = await _db.Users.FindAsync(userId, cancellationToken);
            if (user == null || user.RevokedOn != null) return false;

            user.Login = newLogin;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> SoftDeleteByLoginAsync(string login, Guid revokedBy, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
            if (user == null || user.RevokedOn != null) return false;

            user.RevokedOn = DateTime.UtcNow;
            user.RevokedBy = revokedBy;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
            if (user == null) return false;

            _db.Users.Remove(user);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RestoreAsync(Guid userId, Guid restoredBy, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FindAsync(userId, cancellationToken);
            if (user == null || user.RevokedOn == null) return false;

            user.RevokedOn = null;
            user.RevokedBy = null;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = restoredBy;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }


