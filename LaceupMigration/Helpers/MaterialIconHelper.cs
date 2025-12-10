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
			_ => "\uEB94"
		};
	}
}

