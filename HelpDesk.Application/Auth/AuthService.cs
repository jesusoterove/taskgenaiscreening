using HelpDesk.Application.Common;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Repositories;

namespace HelpDesk.Application.Auth;

public sealed class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IClock _clock;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IClock clock)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _clock = clock;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new AppException("Email is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new AppException("Name is required.", 400);
        }

        if (!PasswordPolicy.IsValid(request.Password))
        {
            throw new AppException("Password does not meet complexity requirements.", 400);
        }

        if (await _userRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new AppException("A user with that email already exists.", 400);
        }

        var now = _clock.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = request.Name.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            CreatedDt = now,
            UpdatedDt = now
        };

        var created = await _userRepository.CreateAsync(user, cancellationToken);
        return BuildAuthResponse(created);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException("Invalid email or password.", 401);
        }

        return BuildAuthResponse(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!PasswordPolicy.IsValid(request.NewPassword))
        {
            throw new AppException("New password does not meet complexity requirements.", 400);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new AppException("User not found.", 404);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new AppException("Current password is invalid.", 401);
        }

        var newHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdatePasswordHashAsync(userId, newHash, _clock.UtcNow, cancellationToken);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var actor = new ActorContext
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role
        };

        return new AuthResponse(user.Id, user.Email, user.Name, user.Role.ToString(), _jwtTokenGenerator.Generate(actor));
    }
}
