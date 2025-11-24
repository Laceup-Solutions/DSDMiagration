namespace LaceupMigration.Business.Interfaces
{
    public interface IDatePickerService
    {
        Task<DateTime?> ShowDatePickerAsync(string title, DateTime? initialDate = null, DateTime? minimumDate = null, DateTime? maximumDate = null);
    }
}

