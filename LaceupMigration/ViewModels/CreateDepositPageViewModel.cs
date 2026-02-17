using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LaceupMigration.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace LaceupMigration.ViewModels
{
	public partial class CreateDepositPageViewModel : ObservableObject
	{
		private readonly DialogService _dialogService;
		private readonly ILaceupAppService _appService;
		private BankDeposit _deposit;

		[ObservableProperty]
		private string _batchNumber = string.Empty;

		[ObservableProperty]
		private string _invoicesText = string.Empty;

		[ObservableProperty]
		private string _userText = string.Empty;

		[ObservableProperty]
		private string _depositTotalText = string.Empty;

		[ObservableProperty]
		private string _comments = string.Empty;

		[ObservableProperty]
		private DateTime _postedDate = DateTime.Now;

		[ObservableProperty]
		private string _imageRangeText = "0/0";

		[ObservableProperty]
		private ImageSource _currentImage;

		[ObservableProperty]
		private ObservableCollection<string> _bankList = new();

		[ObservableProperty]
		private int _selectedBankIndex = 0;

		[ObservableProperty]
		private bool _hasImages = false;

		[ObservableProperty]
		private int _selectedImageIndex = 0;

		private List<string> _images = new List<string>();

		public CreateDepositPageViewModel(DialogService dialogService, ILaceupAppService appService)
		{
			_dialogService = dialogService;
			_appService = appService;
		}

		public async Task InitializeAsync()
		{
			_deposit = BankDeposit.currentDeposit;

			if (_deposit == null)
			{
				await _dialogService.ShowAlertAsync("No deposit found.", "Alert", "OK");
				await Shell.Current.GoToAsync("..");
				return;
			}

			LoadDepositData();
			LoadBanks();
			LoadImages();
		}

		public async Task<bool> OnBackButtonPressedAsync()
		{
				var confirmed = await _dialogService.ShowConfirmationAsync(
					"Alert",
					"You will lose any changes you made in this screen. Are you sure you want to leave?",
					"Yes",
					"No");
				
				if (confirmed && _deposit != null)
					_deposit.Delete();
				
				if (!confirmed)
					return true;

				return false;
		}
		
		private void LoadDepositData()
		{
			BatchNumber = "Batch #" + _deposit.UniqueId;

			var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
			UserText = "User: " + (salesman != null ? salesman.Name : string.Empty);

			PostedDate = _deposit.PostedDate != DateTime.MinValue ? _deposit.PostedDate : DateTime.Now;
			Comments = _deposit.Comment;

			var temp_Ids = string.Empty;
			foreach (var p in _deposit.Payments)
			{
				var invoices = p.Invoices();
				if (!string.IsNullOrEmpty(p.InvoicesId) && invoices != null)
				{
					foreach (var i in invoices)
					{
						if (string.IsNullOrEmpty(temp_Ids))
							temp_Ids = i.InvoiceNumber;
						else
							temp_Ids += "," + i.InvoiceNumber;
					}
				}
				else
				{
					var orders = p.Orders();
					if (orders != null)
					{
						foreach (var o in orders)
						{
							if (string.IsNullOrEmpty(temp_Ids))
								temp_Ids = o.PrintedOrderId;
							else
								temp_Ids += "," + o.PrintedOrderId;
						}
					}
					else
					{
						if (string.IsNullOrEmpty(temp_Ids))
							temp_Ids = p.OrderId;
						else
							temp_Ids += "," + p.OrderId;
					}
				}
			}

			InvoicesText = "Invoices: " + temp_Ids;
			DepositTotalText = "Deposit Total: " + _deposit.TotalAmount.ToCustomString();
		}

		private void LoadBanks()
		{
			BankList.Clear();
			BankList.Add("None");

			foreach (var bank in BankAccount.List)
			{
				BankList.Add(bank.Name);
			}

			if (_deposit.bankAccountId > 0)
			{
				var bank = BankAccount.List.FirstOrDefault(x => x.Id == _deposit.bankAccountId);
				if (bank != null)
				{
					var index = BankList.IndexOf(bank.Name);
					if (index >= 0)
						SelectedBankIndex = index;
				}
			}
		}

		private void LoadImages()
		{
			_images.Clear();

			if (!string.IsNullOrEmpty(_deposit.ImageId))
			{
				var parts = _deposit.ImageId.Split(',');

				foreach (var p in parts)
				{
					var temp_path = System.IO.Path.Combine(Config.DepositImagesPath, p);
					if (File.Exists(temp_path))
						_images.Add(temp_path);
				}
			}

			HasImages = _images.Count > 0;
			SelectedImageIndex = 0;

			if (_images.Count == 0)
			{
				ImageRangeText = "0/0";
				CurrentImage = ImageSource.FromFile("placeholder.png");
			}
			else
			{
				ImageRangeText = "1/" + _images.Count;
				LoadCurrentImage();
			}
		}

		private void LoadCurrentImage()
		{
			if (_images.Count == 0 || SelectedImageIndex < 0 || SelectedImageIndex >= _images.Count)
			{
				// Set placeholder image when no images available
				CurrentImage = ImageSource.FromFile("placeholder.png");
				return;
			}

			var imagePath = _images[SelectedImageIndex];
			if (File.Exists(imagePath))
			{
				CurrentImage = ImageSource.FromFile(imagePath);
			}
			else
			{
				CurrentImage = ImageSource.FromFile("placeholder.png");
			}
		}

		partial void OnCommentsChanged(string value)
		{
			if (_deposit != null)
			{
				_deposit.Comment = value;
				_deposit.Save();
			}
		}

		partial void OnPostedDateChanged(DateTime value)
		{
			if (_deposit != null)
			{
				_deposit.PostedDate = value;
				_deposit.Save();
			}
		}

		partial void OnSelectedBankIndexChanged(int value)
		{
			if (_deposit == null || value < 0 || value >= BankList.Count)
				return;

			var bankName = BankList[value];
			if (bankName == "None")
			{
				_deposit.bankAccountId = 0;
			}
			else
			{
				var bank = BankAccount.List.FirstOrDefault(x => x.Name == bankName);
				_deposit.bankAccountId = bank != null ? bank.Id : 0;
			}

			_deposit.Save();
		}

		[RelayCommand]
		private async Task PreviousImage()
		{
			if (_images.Count == 0)
				return;

			if ((SelectedImageIndex - 1) < 0)
				return;

			SelectedImageIndex--;
			ImageRangeText = (SelectedImageIndex + 1).ToString() + "/" + _images.Count;
			LoadCurrentImage();
		}

		[RelayCommand]
		private async Task NextImage()
		{
			if (_images.Count == 0)
				return;

			if (_images.Count <= (SelectedImageIndex + 1))
				return;

			SelectedImageIndex++;
			ImageRangeText = (SelectedImageIndex + 1).ToString() + "/" + _images.Count;
			LoadCurrentImage();
		}

		[RelayCommand]
		private async Task AddImage()
		{
			try
			{
				var photo = await MediaPicker.CapturePhotoAsync();
				if (photo == null)
					return;

				var imageId = Guid.NewGuid().ToString("N");
				var filePath = System.IO.Path.Combine(Config.DepositImagesPath, imageId);

				// Ensure directory exists
				var directory = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				// Copy photo to deposit images path
				using (var sourceStream = await photo.OpenReadAsync())
				using (var fileStream = File.Create(filePath))
				{
					await sourceStream.CopyToAsync(fileStream);
				}

				if (!string.IsNullOrEmpty(_deposit.ImageId))
					_deposit.ImageId += "," + imageId;
				else
					_deposit.ImageId = imageId;

				_images.Add(filePath);
				SelectedImageIndex = _images.Count - 1;
				ImageRangeText = (SelectedImageIndex + 1).ToString() + "/" + _images.Count;
				HasImages = true;
				LoadCurrentImage();

				_deposit.Save();
			}
			catch (Exception ex)
			{
				await _dialogService.ShowAlertAsync($"Error adding image: {ex.Message}", "Error", "OK");
				_appService.TrackError(ex);
			}
		}

		[RelayCommand]
		private async Task DeleteImage()
		{
			if (_images.Count == 0)
			{
				await _dialogService.ShowAlertAsync("There are no images to delete", "Warning", "OK");
				return;
			}

			var result = await _dialogService.ShowConfirmationAsync("Are you sure you want to delete the selected image?", "Warning", "Yes", "No");
			if (!result)
				return;

			var toDelete = _images[SelectedImageIndex];
			var imageIdToDelete = Path.GetFileName(toDelete);

			var depositEx = _deposit.ImageId;
			var listofImages = depositEx.Split(',').ToList();
			listofImages.Remove(imageIdToDelete);

			_deposit.ImageId = string.Join(",", listofImages);
			_images.RemoveAt(SelectedImageIndex);

			_deposit.Save();

			SelectedImageIndex = 0;

			if (_images.Count > 0)
			{
				ImageRangeText = (SelectedImageIndex + 1).ToString() + "/" + _images.Count;
				LoadCurrentImage();
			}
			else
			{
				ImageRangeText = "0/0";
				CurrentImage = null;
				HasImages = false;
			}
		}

		[RelayCommand]
		private async Task Print()
		{
			try
			{
				PrinterProvider.PrintDocument((int copies) =>
				{
					CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];
					IPrinter printer = PrinterProvider.CurrentPrinter();

					bool allWent = true;
					for (int i = 0; i < copies; i++)
					{
						if (!printer.PrintPaymentBatch())
							allWent = false;
					}

					if (!allWent)
						return "Error printing Deposit";
					return string.Empty;
				});
			}
			catch (Exception ex)
			{
				await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
				_appService.TrackError(ex);
			}
		}

		[RelayCommand]
		private async Task Send()
		{
			if (_deposit.bankAccountId == 0)
			{
				await _dialogService.ShowAlertAsync("You must select a bank for the deposit before continuing.", "Warning", "OK");
				return;
			}

			try
			{
				DataProvider.SendDeposit();
				DataProvider.SendInvoicePaymentsBySource(_deposit.Payments, true);

				await _dialogService.ShowAlertAsync("Deposit Sent Successfully", "Success", "OK");

				if (_deposit != null)
					_deposit.Delete();

				await Shell.Current.GoToAsync("..");
			}
			catch (Exception ex)
			{
				await _dialogService.ShowAlertAsync("An error occurred trying to send deposit. Please try again.", "Alert", "OK");
				Logger.CreateLog(ex);
				_appService.TrackError(ex);
			}
		}

		[RelayCommand]
		private async Task SendByEmail()
		{
			try
			{
				PdfHelper.SendDepositByEmail(_deposit);
			}
			catch (Exception ex)
			{
				await _dialogService.ShowAlertAsync($"Error sending email: {ex.Message}", "Error", "OK");
				_appService.TrackError(ex);
			}
		}

		[RelayCommand]
		private async Task Delete()
		{
			var result = await _dialogService.ShowConfirmationAsync( "Warning", "Are you sure you want to leave this deposit?","Yes", "No");
			if (!result)
				return;

			if (_deposit != null)
				_deposit.Delete();

			await Shell.Current.GoToAsync("..");
		}

		[RelayCommand]
		private async Task SelectPostedDate()
		{
			var date = await _dialogService.ShowDatePickerAsync("Select Posted Date", PostedDate);
			if (date.HasValue)
			{
				PostedDate = date.Value;
			}
		}
	}
}

