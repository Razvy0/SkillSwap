namespace SkillSwap.Core.Exceptions;

public class BadRequestException : ApiException
{
    public BadRequestException(string message) : base(message, 400) { }
}
