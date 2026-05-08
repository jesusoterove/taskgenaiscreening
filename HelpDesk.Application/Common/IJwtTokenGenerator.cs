namespace HelpDesk.Application.Common;

public interface IJwtTokenGenerator
{
    string Generate(ActorContext actor);
}
