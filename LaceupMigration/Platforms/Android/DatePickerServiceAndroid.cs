using Android.App;
using Android.OS;
using LaceupMigration.Business.Interfaces;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;
using DatePickerDialog = Android.App.DatePickerDialog;

namespace LaceupMigration.Platforms.Android
{
    public class DatePickerService : LaceupMigration.Business.Interfaces.IDatePickerService
    {
        public Task<DateTime?> ShowDatePickerAsync(string title, DateTime? initialDate = null, DateTime? minimumDate = null, DateTime? maximumDate = null)
        {
            var tcs = new TaskCompletionSource<DateTime?>();
            var selectedDate = initialDate ?? DateTime.Today;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                    if (activity == null)
                    {
                        tcs.SetResult(null);
                        return;
                    }

                    // Create Android DatePickerDialog
                    var datePickerDialog = new DatePickerDialog(
                        activity,
                        (sender, e) =>
                        {
                            // Date was selected
                            var date = new DateTime(e.Year, e.MonthOfYear + 1, e.DayOfMonth);
                            tcs.SetResult(date);
                        },
                        selectedDate.Year,
                        selectedDate.Month - 1, // Android months are 0-based
                        selectedDate.Day);

                    // Set minimum date if provided
                    if (minimumDate.HasValue)
                    {
                        var minDateMillis = (long)(minimumDate.Value - new DateTime(1970, 1, 1)).TotalMilliseconds;
                        datePickerDialog.DatePicker.MinDate = minDateMillis;
                    }

                    // Set maximum date if provided
                    if (maximumDate.HasValue)
                    {
                        var maxDateMillis = (long)(maximumDate.Value - new DateTime(1970, 1, 1)).TotalMilliseconds;
                        datePickerDialog.DatePicker.MaxDate = maxDateMillis;
                    }

                    // Set title if provided
                    if (!string.IsNullOrEmpty(title))
                    {
                        datePickerDialog.SetTitle(title);
                    }

                    // Handle cancellation
                    datePickerDialog.CancelEvent += (sender, e) =>
                    {
                        if (!tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(null);
                        }
                    };

                    // Show the dialog
                    datePickerDialog.Show();
                }
                catch (Exception ex)
                {
                    // If anything fails, return null
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.SetResult(null);
                    }
                }
            });

            return tcs.Task;
        }
    }
}

