using MauiIcons.Material.Outlined;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LaceupMigration.Helpers;

public static class MaterialIconHelper
{
	public static ImageSource GetImageSource(MaterialOutlinedIcons icon, Color? color = null, double size = 24)
	{
		var glyph = GetIconGlyph(icon);
		
		return new FontImageSource
		{
			Glyph = glyph,
			FontFamily = "MaterialOutlinedIcons",
			Size = size,
			Color = color ?? Colors.Black
		};
	}
	
	private static string GetIconGlyph(MaterialOutlinedIcons icon)
	{
		return icon switch
		{
			MaterialOutlinedIcons.PersonPinCircle => "\uE56A",
			MaterialOutlinedIcons.LocalShipping => "\uE558",
			MaterialOutlinedIcons.CheckCircle => "\uE86C",
			MaterialOutlinedIcons.Description => "\uE873",
			MaterialOutlinedIcons.QrCodeScanner => "\uF206",
			MaterialOutlinedIcons.ArrowBack => "\uE5C4",
			MaterialOutlinedIcons.FlashOff => "\uE3E6",
			MaterialOutlinedIcons.FlashOn => "\uE3E7",
			MaterialOutlinedIcons.PermContactCalendar => "\uE8A3",
			MaterialOutlinedIcons.AddLocationAlt => "\uEF3A",
			MaterialOutlinedIcons.PhoneInTalk => "\uE61D",
			MaterialOutlinedIcons.Payments => "\uEF63",
			MaterialOutlinedIcons.StickyNote2 => "\uF1FC",
			MaterialOutlinedIcons.CalendarMonth => "\uEBCC",
			MaterialOutlinedIcons.DateRange => "\uE916",
			MaterialOutlinedIcons.AccessTime => "\uE192",
			MaterialOutlinedIcons.Email => "\uE0BE",
			MaterialOutlinedIcons.DataArray => "\uEAD1",
			MaterialOutlinedIcons.ContentCopy => "\uE190",
			_ => "\uEB94"
		};
	}
}

