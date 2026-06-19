namespace SkillSwap.Core.Exceptions;

public class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(message, 404) { }
    public NotFoundException(string entity, object key)
        : base($"{entity} with key '{key}' was not found.", 404) { }
}
