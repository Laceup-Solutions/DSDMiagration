using CoreGraphics;
using Foundation;
using LaceupMigration.Business.Interfaces;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UIKit;

namespace LaceupMigration.Platforms.iOS
{
    public class DatePickerService : IDatePickerService
    {
        public Task<DateTime?> ShowDatePickerAsync(string title, DateTime? initialDate = null, DateTime? minimumDate = null, DateTime? maximumDate = null)
        {
            var tcs = new TaskCompletionSource<DateTime?>();
            var selectedDate = initialDate ?? DateTime.Today;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var viewController = Platform.GetCurrentUIViewController();
                    if (viewController == null)
                    {
                        tcs.SetResult(null);
                        return;
                    }

                    var reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                    // Full-screen overlay with dimmed background
                    var overlayVC = new UIViewController();
                    overlayVC.ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
                    overlayVC.View!.BackgroundColor = UIColor.FromWhiteAlpha(0, 0.4f);

                    // White rounded card (centered modal)
                    var cardView = new UIView
                    {
                        BackgroundColor = UIColor.White,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    cardView.Layer.CornerRadius = 14;
                    cardView.Layer.MasksToBounds = true;
                    overlayVC.View.AddSubview(cardView);

                    // Title - centered, black, bold
                    var titleLabel = new UILabel
                    {
                        Text = title ?? "Select Date",
                        Font = UIFont.BoldSystemFontOfSize(17),
                        TextColor = UIColor.Black,
                        TextAlignment = UITextAlignment.Center,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    cardView.AddSubview(titleLabel);

                    // Date picker wheels
                    var datePicker = new UIDatePicker
                    {
                        Mode = UIDatePickerMode.Date,
                        PreferredDatePickerStyle = UIDatePickerStyle.Wheels,
                        BackgroundColor = UIColor.Clear,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    datePicker.Date = NSDate.FromTimeIntervalSinceReferenceDate(
                        (selectedDate.ToUniversalTime() - reference).TotalSeconds);

                    if (minimumDate.HasValue)
                    {
                        datePicker.MinimumDate = NSDate.FromTimeIntervalSinceReferenceDate(
                            (minimumDate.Value.ToUniversalTime() - reference).TotalSeconds);
                    }

                    if (maximumDate.HasValue)
                    {
                        datePicker.MaximumDate = NSDate.FromTimeIntervalSinceReferenceDate(
                            (maximumDate.Value.ToUniversalTime() - reference).TotalSeconds);
                    }

                    cardView.AddSubview(datePicker);

                    // Cancel and Done - blue text, at bottom
                    var cancelButton = UIButton.FromType(UIButtonType.System);
                    cancelButton.SetTitle("Cancel", UIControlState.Normal);
                    cancelButton.TranslatesAutoresizingMaskIntoConstraints = false;
                    cancelButton.TouchUpInside += (s, e) =>
                    {
                        overlayVC.DismissViewController(true, null);
                        tcs.SetResult(null);
                    };

                    var doneButton = UIButton.FromType(UIButtonType.System);
                    doneButton.SetTitle("Done", UIControlState.Normal);
                    doneButton.TranslatesAutoresizingMaskIntoConstraints = false;
                    doneButton.TouchUpInside += (s, e) =>
                    {
                        var nsDate = datePicker.Date;
                        var utcDate = reference.AddSeconds(nsDate.SecondsSinceReferenceDate);
                        overlayVC.DismissViewController(true, null);
                        tcs.SetResult(utcDate.ToLocalTime());
                    };

                    var buttonStack = new UIStackView(new UIView[] { cancelButton, doneButton })
                    {
                        Axis = UILayoutConstraintAxis.Horizontal,
                        Distribution = UIStackViewDistribution.EqualSpacing,
                        Alignment = UIStackViewAlignment.Center,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    cardView.AddSubview(buttonStack);

                    var cardWidth = (NFloat)Math.Min(320.0, (double)(UIScreen.MainScreen.Bounds.Width - 40));

                    NSLayoutConstraint.ActivateConstraints(new[]
                    {
                        cardView.CenterXAnchor.ConstraintEqualTo(overlayVC.View.CenterXAnchor),
                        cardView.CenterYAnchor.ConstraintEqualTo(overlayVC.View.CenterYAnchor),
                        cardView.WidthAnchor.ConstraintEqualTo(cardWidth),
                        cardView.LeadingAnchor.ConstraintGreaterThanOrEqualTo(overlayVC.View.LeadingAnchor, 20),
                        overlayVC.View.TrailingAnchor.ConstraintGreaterThanOrEqualTo(cardView.TrailingAnchor, 20),

                        titleLabel.TopAnchor.ConstraintEqualTo(cardView.TopAnchor, 20),
                        titleLabel.LeadingAnchor.ConstraintEqualTo(cardView.LeadingAnchor, 20),
                        titleLabel.TrailingAnchor.ConstraintEqualTo(cardView.TrailingAnchor, -20),

                        datePicker.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, 8),
                        datePicker.CenterXAnchor.ConstraintEqualTo(cardView.CenterXAnchor),

                        buttonStack.TopAnchor.ConstraintEqualTo(datePicker.BottomAnchor, 12),
                        buttonStack.LeadingAnchor.ConstraintEqualTo(cardView.LeadingAnchor, 24),
                        buttonStack.TrailingAnchor.ConstraintEqualTo(cardView.TrailingAnchor, -24),
                        buttonStack.BottomAnchor.ConstraintEqualTo(cardView.BottomAnchor, -20),
                        buttonStack.HeightAnchor.ConstraintEqualTo(44)
                    });

                    var tapGesture = new UITapGestureRecognizer(() =>
                    {
                        overlayVC.DismissViewController(true, null);
                        tcs.SetResult(null);
                    });
                    overlayVC.View.AddGestureRecognizer(tapGesture);

                    viewController.PresentViewController(overlayVC, true, null);
                }
                catch
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(null);
                }
            });

            return tcs.Task;
        }
    }
}
