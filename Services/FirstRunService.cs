namespace ssk.Services;

public class FirstRunService
{
    private readonly UserProfileRepository _userProfileRepo;

    public FirstRunService(UserProfileRepository userProfileRepo)
    {
        _userProfileRepo = userProfileRepo;
    }

    public Task<bool> IsFirstRunAsync() => _userProfileRepo.IsFirstRunAsync();

    public Task MarkCompletedAsync() => _userProfileRepo.MarkFirstRunCompletedAsync();
}
