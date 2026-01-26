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
			// QrCodeScanner: Since \uE8B6 shows magnifier, try alternative codes
			// Material Icons qr_code_scanner should be \uE8B6, but MauiIcons might map differently
			// Try \uE8B7, \uE8B8, or check what the XAML actually uses
			MaterialOutlinedIcons.QrCodeScanner => "\uF206", // Try E8B8 (if still wrong, may need to inspect XAML-generated code)
			_ => "\uEB94"
		};
	}
}

