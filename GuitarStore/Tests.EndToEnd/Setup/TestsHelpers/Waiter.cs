namespace Tests.EndToEnd.Setup.TestsHelpers;
internal class Waiter
{
    public static async Task WaitForCondition(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            if (await condition())
                return;

            await Task.Delay(100);
        }

        throw new TimeoutException("Condition not met within timeout.");
    }
}
