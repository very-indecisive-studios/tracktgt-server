using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Core.Users.Register;

public record CheckUserExistQuery(
    string UserName,
    string Email
) : IRequest<CheckUserExistResult>;

public class CheckUserExistValidator : AbstractValidator<CheckUserExistQuery>
{
    public CheckUserExistValidator()
    {
        RuleFor(q => q.UserName).NotEmpty();
        RuleFor(q => q.Email).NotEmpty().EmailAddress();
    }
}

public record CheckUserExistResult(bool IsUserNameTaken, bool IsEmailTaken);

public class CheckUserExistHandler : IRequestHandler<CheckUserExistQuery, CheckUserExistResult>
{
    private readonly DatabaseContext _dbContext;

    public CheckUserExistHandler(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<CheckUserExistResult> Handle(CheckUserExistQuery query, CancellationToken cancellationToken)
    {
        var isUserNameTaken = await _dbContext.Users
            .Where(u => u.UserName.Equals(query.UserName))
            .AnyAsync(cancellationToken);
        
        var isEmailTaken = await _dbContext.Users
            .Where(u => u.Email.Equals(query.Email))
            .AnyAsync(cancellationToken);

        return new CheckUserExistResult(isUserNameTaken, isEmailTaken);
    }
}