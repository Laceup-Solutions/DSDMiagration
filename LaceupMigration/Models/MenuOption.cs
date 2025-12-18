namespace LaceupMigration
{
    public record MenuOption(string Title, Func<Task> Action);
}

