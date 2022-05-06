using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tracker.Persistence;

namespace Tracker.Core.Users;

public class CheckUserExistQuery : IRequest<CheckUserExistResult>
{
    public string UserName { get; set; }
    
    public string Email { get; set; }
}

public class CheckUserExistValidator : AbstractValidator<CheckUserExistQuery>
{
    public CheckUserExistValidator()
    {
        RuleFor(q => q.UserName).NotEmpty();
        RuleFor(q => q.Email).NotEmpty().EmailAddress();
    }
}

public class CheckUserExistResult
{
    public bool IsUserNameTaken { get; set; }
    
    public bool IsEmailTaken { get; set; }
}

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

        return new CheckUserExistResult()
        {
            IsUserNameTaken = isUserNameTaken,
            IsEmailTaken = isEmailTaken
        };
    }
}